using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USB_ISSApi
{
    public class USBISS_IO
    {
        private SerialPort USB_PORT = new SerialPort();
        
        public USBISS_IO()  
        {
            
        }
        public static string[] getPorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Close()
        {
            if (USB_PORT != null && USB_PORT.IsOpen == true)
            {
                USB_PORT.Close();
            }
        }
        public bool connect(string serAddr, int BaudRate = 9600)
        { 
            USB_PORT.Close(); // close any existing handle
            USB_PORT.BaudRate = BaudRate;
            USB_PORT.PortName = serAddr;
            USB_PORT.StopBits = StopBits.One;
            USB_PORT.ReadTimeout = 2000;
            USB_PORT.WriteTimeout = 2000;
            USB_PORT.Open();
            return USB_PORT.IsOpen;
        }

        public void set_io_mode()
        {
            if (USB_PORT.IsOpen == false)
            {
                throw (new SystemException("Port is closed"));
            }
            byte[] sbuf = new byte[100];
            int i  = 0;
	        sbuf[i++] = 0x5A;
	        sbuf[i++] = 0x02;							// Set mode command
	        sbuf[i++] = 0x00;							// set IO mode
	        sbuf[i++] = 0x00;							// All IO pins output low

            USB_PORT.Write(sbuf, 0,i);
            Read(1, sbuf);
            if (sbuf[0] != 0xFF)						// If first returned byte is not 0xFF then an error has occured
                throw (new SystemException("set_io_mode: Error setting IO mode " + sbuf[0].ToString() + sbuf[1].ToString()));
        }

        public void set_io(byte b)
        {
            byte[] sbuf = new byte[10];
            sbuf[0] = 0x63;						// Set pins command
            sbuf[1] = b;						// Value to set pins to

            USB_PORT.Write(sbuf, 0, 2); 
            
        }
        private byte[] readData = null;
        public bool Read(int numBytes)
        {
            readData = new byte[numBytes];

            // this will call the read function for the passed number times, 
            // this way it ensures each byte has been correctly recieved while still using timeouts
            for (int i = 0; i < numBytes; i++)
            {
                try
                {
                    USB_PORT.Read(readData, i, 1);
                }
                catch (Exception)
                {
                    readData = null; return true;
                } // timeout or other error occured, set lost comms indicator
            }
            return false;
        }

        public bool Read(int numBytes, byte[] readData)
        {
            // this will call the read function for the passed number times, 
            // this way it ensures each byte has been correctly recieved while still using timeouts
            for (int i = 0; i < numBytes; i++)
            {
                try
                {
                    USB_PORT.Read(readData, i, 1);
                }
                catch (Exception e)
                {
                    throw (new SystemException("Error reading from I2C " + e.Message));
                } // timeout or other error occured, set lost comms indicator
            }
            return false;
        }

        int CheckResponse()
        {
            byte[] SerBuf = new byte[10];
            Read(2 , SerBuf); 
            if (SerBuf[0] == 0)
            {
                switch (SerBuf[1])
                {
                    case 0x1:
                        throw (new SystemException("Device Error	0x01	No ACK from device"));

                    case 0x2:
                        throw (new SystemException("Buffer Overflow	0x02	You must limit the frame to < 60 bytes"));

                    case 0x3:
                        throw (new SystemException("Buffer Underflow	0x03	More write data was expected than sent"));

                    case 0x4:
                        throw (new SystemException("Unknown command	0x04	Probably your write count is wrong"));
                    default:
                        throw (new SystemException("unknown error"));
                }
            }
            else
            {
                return SerBuf[1];
            }
        }

    }
}

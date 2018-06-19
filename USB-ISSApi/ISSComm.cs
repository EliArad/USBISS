// Written by: Fekete Andras
// This code is protected by GPLv3.
// You may edit and redistribute derivative works as long as you share them.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Collections;
using System.Threading;

namespace USB_ISSApi
{
    public class ISSComm
    {
        private SerialPort USB_PORT = new SerialPort();

        private static object myselfLock = new object();
        private static ISSComm myself = null;
        public static ISSComm getComm()
        { 
            if (myself == null)
            {
                lock (myselfLock)
                {
                    myself = new ISSComm();
                }
            }
            return myself;              
        }

        protected ISSComm() 
        { 
        }

        public class ISS_VERSION 
        {
            private byte[] data;
            public ISS_VERSION() 
            {
                ISSComm.getComm().Write(new byte[] {0x5A, 0x01});
                ISSComm.getComm().Read(3);
                data = ISSComm.getComm().readData;
            }
            public bool isValid
            { 
                get { 
                    return (data != null); 
                } 
            }
            public byte moduleID
            { 
                get 
                { 
                    return data[0]; 
                } 
            }
            public byte fwVersion
            {
                get
                { 
                    return data[1];
                } 
            }
            public byte operMode
            {
                get 
                { 
                    return data[2]; 
                } 
            }
        }

        public class GET_SER_NUM
        {
            private byte[] data;
            public GET_SER_NUM() 
            {
                ISSComm.getComm().Write(new byte[] { 0x5A, 0x03 });
                ISSComm.getComm().Read(8);
                data = ISSComm.getComm().readData;
            }

            public bool isValid {
                get { 
                    return (data != null); 
                }
            }
            public string getSerNum() {
                if (data == null) throw new NullReferenceException("Attepted to access data from an invalid transaction");
                StringBuilder ret = new StringBuilder(8);
                for (int i = 0; i < data.Length; i++) ret.Append((char)data[i]);
                return ret.ToString();
            }
        }

        public class ISS_MODE
        {
            public enum ISS_MODES { IO_MODE = 0x00, IO_CHANGE = 0x10, SERIAL = 0x01 }
            public enum ISS_MODES_I2C { I2C_S_20KHZ = 0x20, I2C_S_50KHZ = 0x30, 
                                        I2C_S_100KHZ = 0x40, I2C_S_500KHZ = 0x50, I2C_H_100KHZ = 0x60, 
                                        I2C_H_400KHZ = 0x70, I2C_H_1000KHZ = 0x80 }
            public enum ISS_MODES_SPI { A2I_L = 0x90, A2I_H = 0x91, I2A_L = 0x92, I2A_H = 0x93 }
            public enum IO_TYPES { OUT_LOW = 0x00, OUT_HIGH = 0x01, IN_DIG = 0x02, IN_ANA = 0x03 }
            public enum SERIAL_BAUD_RATES { _300 = 9999, _1200 = 2499, _2400 = 1249, _9600 = 311, _19200 = 155, _38400 = 77, _57600 = 51, _115200 = 25, _250000 = 11, _1000000 = 3 }
            public enum RESULTS { SUCCESS = 0x00, UNK_CMD = 0x05, INT_ERR1 = 0x06, INT_ERR2 = 0x07 }

            public static RESULTS setIO_MODE(IO_TYPES pin1, IO_TYPES pin2, IO_TYPES pin3, IO_TYPES pin4) { return setMode(ISS_MODES.IO_MODE, pin1, pin2, pin3, pin4); }
            public static RESULTS setIO_MODE_SERIAL(IO_TYPES pin3, IO_TYPES pin4, SERIAL_BAUD_RATES baud) { return setModeSerial(ISS_MODES.IO_MODE, pin3, pin4, baud); }
            public static RESULTS setIO_MODE_SERIAL(IO_TYPES pin3, IO_TYPES pin4, uint baud) { return setModeSerial(ISS_MODES.IO_MODE, pin3, pin4, baud); }

            private static RESULTS setMode(ISS_MODES mode, IO_TYPES pin1, IO_TYPES pin2, IO_TYPES pin3, IO_TYPES pin4)
            {
                ISSComm comm = ISSComm.getComm();
                comm.Write(new byte[] { 0x5A, 0x02, (byte)mode, (byte) (((int)pin4 << 6) | ((int)pin3 << 4) | ((int)pin2 << 2) | (int)pin1) });
                comm.Read(2);
                //if (comm.readData[0] == 0xFF) // is Ack
                return (RESULTS)comm.readData[1];
            }
            /// <summary>This uses the standard baud rates defined by the SERIAL_BAUD enumerated type</summary>
            private static RESULTS setModeSerial(ISS_MODES mode, IO_TYPES pin3, IO_TYPES pin4, SERIAL_BAUD_RATES baud) 
            {
                ISSComm comm = ISSComm.getComm();
                comm.Write(new byte[] { 0x5A, 0x02, (byte)((byte)mode | (byte)ISS_MODES.SERIAL), (byte)(((uint)baud) >> 8), (byte)baud, (byte)(((int)pin4 << 6) | ((int)pin3 << 4)) });
                comm.Read(2);
                //if (comm.readData[0] == 0xFF) // is Ack
                return (RESULTS)comm.readData[1];
            }
            /// <summary>This uses the custom specified baud rate. Eg: baud = 57600</summary>
            private static RESULTS setModeSerial(ISS_MODES mode, IO_TYPES pin3, IO_TYPES pin4, uint baud) 
            {
                baud = (48000000/(16+baud))-1; // convert to baud rate divisor
                ISSComm comm = ISSComm.getComm();
                comm.Write(new byte[] { 0x5A, 0x02, (byte)((byte)mode | (byte)ISS_MODES.SERIAL), (byte)(((uint)baud) >> 8), (byte)baud, (byte)(((int)pin4 << 6) | ((int)pin3 << 4)) });
                comm.Read(2);
                //if (comm.readData[0] == 0xFF) // is Ack
                return (RESULTS)comm.readData[1];
            }
            /// <summary>Should only be used when in Serial or I2C mode</summary>
            public static RESULTS setIO_CHANGE(IO_TYPES pin1, IO_TYPES pin2, IO_TYPES pin3, IO_TYPES pin4) 
            {
                ISSComm comm = ISSComm.getComm();
                comm.Write(new byte[] { 0x5A, 0x02, (byte)ISS_MODES.IO_CHANGE, (byte)(((int)pin4 << 6) | ((int)pin3 << 4) | ((int)pin2 << 2) | (int)pin1) });
                comm.Read(2);
                //if (comm.readData[0] == 0xFF) // is Ack
                return (RESULTS)comm.readData[1];
            }

            public static RESULTS setI2C_MODE(ISS_MODES_I2C mode, IO_TYPES pin4, IO_TYPES pin3) 
            { 
                return setMode((ISS_MODES)mode, 0 , 0 , pin3, pin4);
            }
            /// <summary>This uses the custom specified baud rate. Eg: baud = 57600</summary>
            public static RESULTS setI2C_MODE(ISS_MODES_I2C mode, uint baud)
            {
                baud = (48000000 / (16 + baud)) - 1; // convert to baud rate divisor
                ISSComm comm = ISSComm.getComm();
                comm.Write(new byte[] { 0x5A, 0x02, (byte)((byte)mode | (byte)ISS_MODES.SERIAL), (byte)(((uint)baud) >> 8), (byte)baud });
                comm.Read(2);
                //if (comm.readData[0] == 0xFF) // is Ack
                return (RESULTS)comm.readData[1];
            }
            /// <summary>This uses the standard baud rates defined by the SERIAL_BAUD enumerated type</summary>
            public static RESULTS setI2C_MODE(ISS_MODES_I2C mode, SERIAL_BAUD_RATES baud)
            {
                ISSComm comm = ISSComm.getComm();
                comm.Write(new byte[] { 0x5A, 0x02, (byte)((byte)mode | (byte)ISS_MODES.SERIAL), (byte)(((uint)baud) >> 8), (byte)baud });
                comm.Read(2);
                //if (comm.readData[0] == 0xFF) // is Ack
                return (RESULTS)comm.readData[1];
            }
            public static byte getSPI_clkDiv(double freq) 
            {
                if (freq > 3000000) freq = 3000000;
                else if (freq < 23400) freq = 23400;
                return (byte)((6000000 / freq) - 1);
            }
            public static double getSPI_freq(byte div)
            { 
                return 6000000 / (div + 1); 
            }
            public static RESULTS setSPI_MODE(ISS_MODES_SPI mode, byte clkDiv)
            {
                ISSComm comm = ISSComm.getComm();
                comm.Write(new byte[] { 0x5A, 0x02, (byte)mode, clkDiv });
                comm.Read(2);
                //if (comm.readData[0] == 0xFF) // is Ack
                return (RESULTS)comm.readData[1];
            }
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
            USB_PORT.ReadTimeout = 4000;
            USB_PORT.WriteTimeout = 4000;
            USB_PORT.Open();
            return USB_PORT.IsOpen;
        }

        public bool Write(byte[] SerBuf) 
        {
            try {
                USB_PORT.Write(SerBuf, 0, SerBuf.Length); 
            } 
            catch (Exception) 
            { 
                return false; 
            }
            return true;
        }

        public bool IsPortOpen()
        {
            return USB_PORT.IsOpen;
        }

        public bool Write(byte[] SerBuf, int size)
        {
            try
            {
                byte [] d = {0};
                USB_PORT.Write(SerBuf, 0, size);
                return true;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        public bool WriteSlow(byte[] SerBuf, int size)
        {
            try
            {
                for (int i = 0; i < size; i++)
                {
                    USB_PORT.Write(SerBuf , i , 1);
                }
                return true;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using USB_ISSApi;
using System.Threading;
using System.Management;
using System.IO.Ports;
using System.Runtime.InteropServices;
using SLogApi;


namespace NxpPAApiLib
{
    public class NxpPAApi
    {

        protected enum eOPCODES
        {

            Reset = 0,
            ReadInfo = 0x40,
            AssignID = 0x1,
            ReadAlerts = 0x46,
            PulseTest = 0x6,
            ReadMode = 0x45,
            SetMode = 0x5,
            ReadTXEnable = 0x42,
            WriteTXEnable = 0x2,
            ReadAlertSources = 0x41,
            WriteParam = 0x10,
            ReadParam = 0x50,
            WriteLimit = 0x20,
            ReadLimit = 0x60,
            ReadSensor = 0xC0,
            ReadADC = 0x80
        }


        public enum READ_MODE_VALUES
        {
            Standby_Mode = 0x0,
            Operating_Mode_Pre_PulseTest = 0x01,
            Operating_Mode_with_active_ShutdownState = 0x2,
            Operating_Mode_Post_PulseTest = 0x3
        }

        public enum PA_OPERATION_MODE
        {
            STANDBY = 0,
            OPERATING = 1

        }
        public enum eWriteParams
        {
            Phase = 0x1,
            Frequency = 0x2,
            OutputPower = 0x4,
        }

        public enum eReadParams
        {
            Phase = 0x1,
            Frequency = 0x2,
            OutputPower = 0x4,
        }

        public enum eWriteLimits
        {
            PATemperature = 0x1,
            PACurrent = 0x2,
            VSWR = 0x4,
            MaximumReflectedPower = 0x8,
            MaximumHardwareReflectedPower = 0x10
        }

        public enum eReadADC
        {
            CPUTemperature = 0x20,
            PAVoltage = 0x10,
            ForwardPower = 0x8,
            PATemperature = 0x4,
            IDD = 0x2,
            ReflectedPower = 0x1
        }

        public enum eReadSensor
        {
            ForwardPower = 0x20,
            ReflectedPower = 0x10,
            VSWR = 0x8,
            PAVoltage = 0x4,
            PACurrent = 0x2,
            PATemperature = 0x1,
        }

        string _comPort = string.Empty;
        protected ISSComm comm;
        protected int m_inOperatingMode = -1;
        protected byte BROADCAST_DEVICE_ID = 0;

        const byte m_write_i2c_address = 0x7F;
        const byte m_write_i2c_address_shift = (0x7F << 1);
        const byte m_read_i2c_address_shift = (byte)((0x7F << 1) + 1);

        enum IssCmds
        {
            ISS_VER = 1, 			// returns version num, 1 byte
            ISS_MODE,				// returns ACK, NACK, 1 byte
            GET_SER_NUM,

            I2C_SGL = 0x53,		    // 0x53 Read/Write single byte for non-registered devices
            I2C_AD0,				// 0x54 Read/Write multiple bytes for devices without internal address register
            I2C_AD1,				// 0x55 Read/Write multiple bytes for 1 byte addressed devices 
            I2C_AD2,				// 0x56 Read/Write multiple bytes for 2 byte addressed devices
            I2C_DIRECT,				// 0x57 Direct control of I2C start, stop, read, write.
            ISS_CMD = 0x5A,		    // 0x5A 
            SPI_IO = 0x61,			// 0x61 SPI I/O
            SERIAL_IO,              // 0x62
            SETPINS,				// 0x63 [SETPINS] [pin states]
            GETPINS,				// 0x64 
            GETAD,					// 0x65 [GETAD] [pin to convert]
        };

        // I2C DIRECT commands
        enum I2Cdirect
        {
            I2CSRP = 0x00,			// Start/Stop Codes - 0x01=start, 0x02=restart, 0x03=stop, 0x04=nack
            I2CSTART,				// send start sequence
            I2CRESTART,				// send restart sequence
            I2CSTOP,				// send stop sequence
            I2CNACK,				// send NACK after next read
            I2CREAD = 0x20,		    // 0x20-0x2f, reads 1-16 bytes
            I2CWRITE = 0x30,		// 0x30-0x3f, writes next 1-16 bytes
        };

        public NxpPAApi()
        {
            comm = ISSComm.getComm();
            SLog log = SLog.Instance();
            log.Initialize("pasimple.txt");
        }

        string GetUSB_ISS_PortName()
        {

            using (var searcher = new ManagementObjectSearcher
               ("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList = (from n in portnames
                             join p in ports on n equals p["DeviceID"].ToString()
                             select n + " - " + p["Caption"]).ToList();
                int i = 0;
                foreach (string s in tList)
                {
                    SLog log = SLog.Instance();
                    log.Write(s);
                    if (s.Contains(" - Communications Port (") == true)
                    {
                        return portnames[i];
                    }
                    i++;
                }
            }
            return string.Empty;

        }
        public string Port
        {
            get
            {
                return _comPort;
            }
        }
        public void connect(string comPort, int boudRate, bool autoDetect = false)
        {
            try
            {

                if (autoDetect == true)
                {
                    comPort = GetUSB_ISS_PortName();
                    if (comPort == string.Empty)
                    {
                        throw (new SystemException("Cannot auto detect the com port"));
                    }
                }
                _comPort = comPort;
                if (ISSComm.getComm().connect(comPort, boudRate) == false)
                    throw (new SystemException("Failed to open comport " + comPort));


                ISSComm.ISS_VERSION data = new ISSComm.ISS_VERSION();

                if ((!data.isValid) || (data.moduleID != 7))
                {
                    // if the module id is not that of the USB-ISS
                    throw (new SystemException("Device not found"));
                }
                string txtMode;

                string lblDeviceData = "USB-ISS V" + data.fwVersion + ", SN: " + (new ISSComm.GET_SER_NUM()).getSerNum(); //print the software version on screen
                switch (data.operMode & 0xFE)
                {
                    case (int)ISSComm.ISS_MODE.ISS_MODES.IO_MODE: txtMode = "IO_MODE"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_I2C.I2C_H_1000KHZ: txtMode = "I2C 1MHz HW"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_I2C.I2C_H_100KHZ: txtMode = "I2C 100KHz HW"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_I2C.I2C_H_400KHZ: txtMode = "I2C 400KHz HW"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_I2C.I2C_S_100KHZ: txtMode = "I2C 100KHz SW"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_I2C.I2C_S_20KHZ: txtMode = "I2C 20KHz SW"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_I2C.I2C_S_500KHZ: txtMode = "I2C 500KHz SW"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_I2C.I2C_S_50KHZ: txtMode = "I2C 50KHz SW"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_SPI.A2I_L: txtMode = "SPI TX on Act->Idle, Clock idle = low"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_SPI.A2I_H: txtMode = "SPI TX on Act->Idle, Clock idle = high"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_SPI.I2A_L: txtMode = "SPI TX on Idle->Act, Clock idle = low"; break;
                    case (int)ISSComm.ISS_MODE.ISS_MODES_SPI.I2A_H: txtMode = "SPI TX on Idle->Act, Clock idle = high"; break;
                    default: txtMode = "Unknown mode: 0x" + data.operMode.ToString("X2"); break;
                }
                if ((data.operMode & (int)ISSComm.ISS_MODE.ISS_MODES.SERIAL) == (int)ISSComm.ISS_MODE.ISS_MODES.SERIAL) txtMode += ", with Serial";

                ISSComm comm = ISSComm.getComm();
                comm.Write(new byte[] { 0x5A, 0x02, 0x40, 0xaa });
                comm.Read(2);

            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        public void SetPhase(ushort phaseOffset)
        {
            byte phaseIndex = (byte)(phaseOffset / 5);

            if (phaseOffset > 355 || phaseOffset < 0)
            {
                throw (new SystemException("Phase offset is out of range"));
            }
 
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[15];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 2;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.WriteParam | (byte)eWriteParams.Phase;
                    SerBuf[i++] = phaseIndex;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    CheckResponse();
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }   

        }


        public void WriteParams(AmplifierParams par)
        {
            byte phaseIndex = (byte)(par.Phase / 5);

            if (phaseIndex > 71 || phaseIndex < 0)
            {
                throw (new SystemException("Phase offset is out of range"));
            }

            byte freqIndex = (byte)((par.Frequency - 2386.0) * 2);
            if (freqIndex > 254 )
            {
                throw (new SystemException("frequnecy is out of range"));
            }

            if (par.OutputPower > 100)
            {
                throw (new SystemException("OutputPower is out of range"));
            }

            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 5;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.WriteParam | (byte)0xF;
                    SerBuf[i++] = phaseIndex;
                    SerBuf[i++] = freqIndex;
                    SerBuf[i++] = par.OutputPower;
                    SerBuf[i++] = 0;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    CheckResponse();
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }

        }




        public virtual void SetFrequency(float frequency)
        {
            byte freqIndex = (byte)((frequency - 2386.0) * 2);

            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[15];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 2;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.WriteParam | (byte)eWriteParams.Frequency;
                    SerBuf[i++] = freqIndex;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    CheckResponse();
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }   


        }
        byte[] getBytes_ex<T>(T structure)
        {
            int len = Marshal.SizeOf(structure);
            byte[] result = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(structure, ptr, false);
            Marshal.Copy(ptr, result, 0, len);
            Marshal.DestroyStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return result;
        }
        byte[] getBytes<T>(T str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        protected void ByteArrayToStruct<T>(byte[] packet, ref T str)
        {
            GCHandle pinnedPacket = GCHandle.Alloc(packet, GCHandleType.Pinned);
            str = (T)Marshal.PtrToStructure(
                pinnedPacket.AddrOfPinnedObject(),
                typeof(T));
            pinnedPacket.Free();
        }
        public void WriteLimit(eWriteLimits limit, byte value)
        {
            try
            {
                byte i = 0;
                byte[] buf = new byte[40];
                
                buf[i++] = (byte)((byte)eOPCODES.WriteLimit | (byte)limit);
                buf[i++] = value;
                comm.Write(buf, i);
                return;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        public void WriteLimit(PALimits limits)
        {
            lock(this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[10];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 6;       // write 1+1=2 bytes
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)((byte)eOPCODES.WriteLimit | (byte)0x1F);
                    SerBuf[i++] = limits.PATemperature;
                    SerBuf[i++] = limits.PACurrent;
                    SerBuf[i++] = limits.VSWR;
                    SerBuf[i++] = limits.MaxReflectedPower;
                    SerBuf[i++] = limits.HardwareMaxReflectedPower;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public void SetOutputPower(byte outputPowerInPercentage)
        {
          

            if (outputPowerInPercentage < 0 || outputPowerInPercentage > 100)
                throw (new SystemException("outputPowerInPercentage  range is 0 - 100"));

            try
            {
                byte i = 0;
                byte[] SerBuf = new byte[10];
                SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 2;       // write 1+1=2 bytes
                SerBuf[i++] = m_write_i2c_address_shift;
                SerBuf[i++] = (byte)eOPCODES.WriteParam | (byte)eWriteParams.OutputPower;
                SerBuf[i++] = outputPowerInPercentage;
                SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                comm.Write(SerBuf, i);
                int toRead = CheckResponse();
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        public byte GetOutputPower()
        {          
            try
            {
                byte i = 0;
                byte[] SerBuf = new byte[20];
                SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;       // write 1+1=2 bytes
                SerBuf[i++] = m_write_i2c_address_shift;
                SerBuf[i++] = (byte)eOPCODES.ReadParam | (byte)eWriteParams.OutputPower;


                SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                SerBuf[i++] = m_read_i2c_address_shift;
                SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                comm.Write(SerBuf, i);
                int toRead = CheckResponse();
                comm.Read(toRead, SerBuf);
                return SerBuf[0];
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        public float GetFrequency()
        {
            try
            {
                byte i = 0;
                byte[] SerBuf = new byte[20];
                SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;       // write 1+1=2 bytes
                SerBuf[i++] = m_write_i2c_address_shift;
                SerBuf[i++] = (byte)eOPCODES.ReadParam | (byte)eWriteParams.Frequency;


                SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                SerBuf[i++] = m_read_i2c_address_shift;
                SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                comm.Write(SerBuf, i);
                int toRead = CheckResponse();
                comm.Read(toRead, SerBuf);

                float freq = (float)(2380 + SerBuf[0] * 0.5);
                return freq;
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        public byte GetPhase()
        {
            try
            {
                byte i = 0;
                byte[] SerBuf = new byte[20];
                SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;       // write 1+1=2 bytes
                SerBuf[i++] = m_write_i2c_address_shift;
                SerBuf[i++] = (byte)eOPCODES.ReadParam | (byte)eWriteParams.Phase;


                SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                SerBuf[i++] = m_read_i2c_address_shift;
                SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                comm.Write(SerBuf, i);
                int toRead = CheckResponse();
                comm.Read(toRead, SerBuf);
                return SerBuf[0];
            }
            catch (Exception err)
            {
                throw (new SystemException(err.Message));
            }
        }

        public virtual void Reset()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;       // write 1+1=2 bytes
                    SerBuf[i++] = BROADCAST_DEVICE_ID;
                    SerBuf[i++] = (byte)eOPCODES.Reset;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;

                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Thread.Sleep(300);
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public virtual void PulseTest()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[10];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;       // write 1+1=2 bytes
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.PulseTest;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;

                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Thread.Sleep(200);
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public virtual void CheckI2CDeviceExistance()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[10];
                    SerBuf[i++] = (byte)0x58;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    comm.Write(SerBuf, i);
                    comm.Read(1, SerBuf);
                    if (SerBuf[0] == 0)
                    {
                        throw (new SystemException("No device is detected"));
                    }

                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual PALimits ReadLimit()
        {
            lock (this)
            {
                try
                {
                    PALimits l = new PALimits();

                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.ReadLimit | 0x1F;
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD + 3;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);

                    ByteArrayToStruct(SerBuf, ref l);
                    return l;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual AmplifierParams ReadParams()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)((byte)eOPCODES.ReadParam | 0xF);
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD + 2;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);
                    ReadParamsStruct r = new ReadParamsStruct();
                    ByteArrayToStruct(SerBuf, ref r);

                    AmplifierParams a = new AmplifierParams();
                    a.OutputPower = r.OutputPower;

                    float freq = (float)(2386 + (r.Frequency * 0.5));
                    a.Frequency = freq;
                    a.Phase = (short)(r.Phase * 5);
                    return a;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual ADCReadings ReadADC()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)((byte)eOPCODES.ReadADC | (byte)0x3F);
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD + 10;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);
                    ADCReadings r = new ADCReadings();
                    ByteArrayToStruct(SerBuf, ref r);
                    return r;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual ADCForRedReadings ReadADC_ForRef()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)((byte)eOPCODES.ReadADC | (byte)(eReadADC.ReflectedPower | eReadADC.ForwardPower));
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD + 2;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);
                    ADCForRedReadings r = new ADCForRedReadings();
                    ByteArrayToStruct(SerBuf, ref r);
                    return r;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual ushort ReadOneADC(eReadADC detector)
        {
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)((byte)eOPCODES.ReadADC | (byte)(detector));
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);
                    return Helper.GetShort(SerBuf);
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
        public virtual SensorReadings ReadSensor()
        {
            lock (this)
            {
                try
                {
                    SensorReadings s = new SensorReadings();
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.ReadSensor | (byte)0x3F; // all bits
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD + 4;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);
                    ByteArrayToStruct(SerBuf, ref s);
                    return s;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }


        public virtual SensorForRefReadings ReadForRefSensor()
        {
            lock (this)
            {
                try
                {
                    SensorForRefReadings s = new SensorForRefReadings();
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.ReadSensor | (byte)(eReadSensor.ForwardPower | eReadSensor.ReflectedPower); 
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD + 2;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);
                    ByteArrayToStruct(SerBuf, ref s);
                    return s;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }


        public virtual byte ReadOneSensor(eReadSensor sensor)
        {

            lock (this)
            {

                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)((byte)eOPCODES.ReadSensor | (byte)(sensor));
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);
                    return SerBuf[0];
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            } 
        }

        public virtual byte [] ReadAlertSources()
        {

            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.ReadAlertSources;
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    byte[] result = { 0, 0 };
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    comm.Read(toRead, result);
                    return result;
                }
                catch (Exception err)
                {
                    throw (new SystemException("ReadAlertSources:" + err.Message));
                }
            }
        }


        public virtual bool ReadTXEnable()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;
                    byte[] SerBuf = new byte[20];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.ReadTXEnable;
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int toRead = CheckResponse();
                    Array.Clear(SerBuf, 0, SerBuf.Length);
                    comm.Read(toRead, SerBuf);
                    return SerBuf[0] == 1 ? true : false;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        int CheckResponse()
        {
            byte[] SerBuf = new byte[10];
            comm.Read(2, SerBuf);
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

        public virtual string ReadAlerts(out string errCode)
        {
            lock (this)
            {
                try
                {
                    int i = 0;
                    byte[] SerBuf = new byte[40];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;       // write 1+1=2 bytes
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.ReadAlerts;
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD + 1;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    Thread.Sleep(10);
                    SerBuf[0] = 0x0;
                    int toRead = CheckResponse();
                    comm.Read(toRead, SerBuf);
                    errCode = ErrorCodes.GetError(SerBuf[2]);
                    if (errCode != "No Error")
                    {
                        Console.WriteLine(errCode);
                    }
                    if (SerBuf[0] != 0xFF)
                    {
                        return ErrorCodes.getAlertReasonBit(SerBuf[0]);
                    }
                    else
                        return "No Error";
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual ReadInfoStruct ReadInfo()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;

                    byte[] SerBuf = new byte[40];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;       // write 1+1=2 bytes
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.ReadInfo;
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD + 10;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int readNo = CheckResponse();
                    comm.Read(readNo, SerBuf);
                    ReadInfoStruct r = new ReadInfoStruct();
                    ByteArrayToStruct(SerBuf, ref r);
                    if (r.Type != 1)
                    {
                        throw (new SystemException("Type expected to be 1 indicates its a 250W MWO module"));
                    }
                    if (r.Protocol != 1)
                    {
                        throw (new SystemException("Communication protocol version invalid. This will always read as 1"));
                    }
                    return r;
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual void WriteTXEnable(bool enable)
        {
            if (m_inOperatingMode != 1)
            {
                throw (new SystemException("Module is not in operating mode"));
            }

            lock (this)
            {
                try
                {
                    byte i = 0;

                    byte[] SerBuf = new byte[15];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 2;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.WriteTXEnable;
                    SerBuf[i++] = (byte)(enable == true ? 1 : 0);
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    CheckResponse();
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }   
        }

        public virtual void SetMode(PA_OPERATION_MODE mode)
        {
            lock (this)
            {
                try
                {
                    byte i = 0;

                    byte[] SerBuf = new byte[15];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 2;
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.SetMode;
                    SerBuf[i++] = (byte)mode;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    CheckResponse();
                    Thread.Sleep(200);
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }      
        }

        public virtual void AssignID()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;

                    byte[] SerBuf = new byte[15];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 2;       // write 1+1=2 bytes
                    SerBuf[i++] = BROADCAST_DEVICE_ID;
                    SerBuf[i++] = (byte)eOPCODES.AssignID;
                    SerBuf[i++] = (byte)0x7F;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    CheckResponse();
                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }

        public virtual READ_MODE_VALUES ReadMode()
        {
            lock (this)
            {
                try
                {
                    byte i = 0;

                    byte[] SerBuf = new byte[40];
                    SerBuf[i++] = (byte)IssCmds.I2C_DIRECT;
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE + 1;       // write 1+1=2 bytes
                    SerBuf[i++] = m_write_i2c_address_shift;
                    SerBuf[i++] = (byte)eOPCODES.ReadMode;
                    SerBuf[i++] = (byte)I2Cdirect.I2CRESTART;
                    SerBuf[i++] = (byte)I2Cdirect.I2CWRITE;
                    SerBuf[i++] = m_read_i2c_address_shift;
                    SerBuf[i++] = (byte)I2Cdirect.I2CNACK;
                    SerBuf[i++] = (byte)I2Cdirect.I2CREAD; // read one back byte only
                    SerBuf[i++] = (byte)I2Cdirect.I2CSTOP;
                    comm.Write(SerBuf, i);
                    int readNo = CheckResponse();
                    comm.Read(readNo, SerBuf);
                    READ_MODE_VALUES mode = (READ_MODE_VALUES)SerBuf[0];
                    if (mode != READ_MODE_VALUES.Standby_Mode)
                    {
                        m_inOperatingMode = 1;
                    }
                    else
                    {
                        m_inOperatingMode = 0;
                    }
                    return mode;

                }
                catch (Exception err)
                {
                    throw (new SystemException(err.Message));
                }
            }
        }
    }
}

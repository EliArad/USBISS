using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NxpPAApiLib
{
   [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ReadInfoStruct
    {
        public byte Lockk;
        public byte Protocol;
        public byte I2CAddress;
        public byte Type;
        public ushort SWVersion;
        public ushort HWVersion;
        public int    SerialNumVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Name;        
 
    }

    public struct SensorForRefReadings
    {
        public byte ReflectedPower;
        public byte ForwardPower;

    }

     public struct SensorReadings
    {
        public byte PATemperature;
        public byte PACurrent;
        public byte PAVoltage;
        public byte VSWR;
        public byte ReflectedPower;
        public byte ForwardPower;     
    }

    public struct ADCReadings
    {

        public ushort ReflectedPower;
        public ushort IDD;
        public ushort PATemperature;
        public ushort ForwardPower;
        public ushort PAVoltage;
        public ushort CPUTemperature;                        
    }

    public struct ADCForRedReadings
    {
        public ushort ReflectedPower;
        public ushort ForwardPower;
    }

    public struct PALimits
    {
        public byte PATemperature;
        public byte PACurrent;
        public byte VSWR;
        public byte MaxReflectedPower;              
        public byte HardwareMaxReflectedPower;
    }


    public struct ReadParamsStruct
    {
        public byte Phase;
        public byte Frequency;
        public byte OutputPower;
        public byte Reserved;
    }

    public struct AmplifierParams
    {
        public short Phase;
        public float Frequency;
        public byte OutputPower;
        public byte reserved;
    }

}

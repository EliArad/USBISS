#pragma once 

#include "types.h"

struct ReadInfoStruct
{
	byte Lockk;
	byte Protocol;
	byte I2CAddress;
	byte Type;
	ushort SWVersion;
	ushort HWVersion;
	int    SerialNumVersion;
	byte * Name;

};

struct SensorForRefReadings
{
	byte ReflectedPower;
	byte ForwardPower;
};

struct SensorReadings
{
	byte PATemperature;
	byte PACurrent;
	byte PAVoltage;
	byte VSWR;
	byte ReflectedPower;
	byte ForwardPower;
};

struct ADCReadings
{

	ushort ReflectedPower;
	ushort IDD;
	ushort PATemperature;
	ushort ForwardPower;
	ushort PAVoltage;
	ushort CPUTemperature;
};

struct ADCForRedReadings
{
	ushort ReflectedPower;
	ushort ForwardPower;
};

struct PALimits
{
	byte PATemperature;
	byte PACurrent;
	byte VSWR;
	byte MaxReflectedPower;
	byte HardwareMaxReflectedPower;
};


struct ReadParamsStruct
{
	byte Phase;
	byte Frequency;
	byte OutputPower;
	byte Reserved;
};

struct AmplifierParams
{
	short Phase;
	float Frequency;
	byte OutputPower;
	byte reserved;
};

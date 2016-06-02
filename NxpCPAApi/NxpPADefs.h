#pragma once

#include "types.h"

typedef enum READ_MODE_VALUES
{
	Standby_Mode = 0x0,
	Operating_Mode_Pre_PulseTest = 0x01,
	Operating_Mode_with_active_ShutdownState = 0x2,
	Operating_Mode_Post_PulseTest = 0x3
}READ_MODE_VALUES;



enum  class eOPCODES
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
};

enum class eWriteLimits
{
	PATemperature = 0x1,
	PACurrent = 0x2,
	VSWR = 0x4,
	MaximumReflectedPower = 0x8,
	MaximumHardwareReflectedPower = 0x10
};

 

enum class PA_OPERATION_MODE
{
	STANDBY = 0,
	OPERATING = 1

};
enum class eWriteParams
{
	Phase = 0x1,
	Frequency = 0x2,
	OutputPower = 0x4,
};

enum class eReadParams
{
	Phase = 0x1,
	Frequency = 0x2,
	OutputPower = 0x4,
};

typedef struct _ISSVersionInfo
{
	byte moduleID;
	byte fwVersion;
	byte operMode;

}ISSVersionInfo;

enum class eReadADC
{
	CPUTemperature = 0x20,
	PAVoltage = 0x10,
	ForwardPower = 0x8,
	PATemperature = 0x4,
	IDD = 0x2,
	ReflectedPower = 0x1
};

enum class eReadSensor
{
	ForwardPower = 0x20,
	ReflectedPower = 0x10,
	VSWR = 0x8,
	PAVoltage = 0x4,
	PACurrent = 0x2,
	PATemperature = 0x1,
};


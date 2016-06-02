#pragma once

#include "types.h"
#include "NxpPaUSBI2C.h"
#include "pacommandsdefs.h"
#include <thread>         // std::thread
#include <mutex>          // std::mutex
#include "SerialComm.h"
#include "windo.h"
#include <iostream>
#include <exception>
#include "ErrorCodes.h"
#include "Helper.h"
#include <memory>
#include <chrono>
#include "SerialWindo.h"
#include "NxpPADefs.h"
#include "USBISSDefs.h"
 
using namespace std;
 
class CNxpPaUSBI2CSim : public CNxpPaUSBI2C
{

public:
	CNxpPaUSBI2CSim(string comPort, int boudRate = 9600);
	virtual ~CNxpPaUSBI2CSim();

	virtual void ReadADC(ADCReadings *a);
	virtual void SetPhase(u16_t phaseOffset);
	virtual void SetFrequency(float frequency);
	virtual ushort ReadOneADC(eReadADC detector);
	virtual void ReadSensor(SensorReadings *r);
	virtual byte ReadOneSensor(eReadSensor sensor);
	virtual bool ReadTXEnable();
	virtual string ReadAlerts(string errCode);
	virtual void ReadAlertSources(byte *result);
	virtual void WriteParams(AmplifierParams par);
	virtual void WriteLimit(eWriteLimits limit, byte value);
	virtual void WriteLimit(PALimits limits);
	virtual void SetOutputPower(byte outputPowerInPercentage);
	virtual byte GetOutputPower();
	virtual float GetFrequency();
	virtual byte GetPhase();
	virtual void Reset();
	virtual void ReadInfo(ReadInfoStruct *r);
	virtual void ReadADC_ForRef(ADCForRedReadings *a);
	virtual void ReadForRefSensor(SensorForRefReadings *s);
	virtual void WriteTXEnable(bool enable);
	virtual void AssignID();
	virtual void SetMode(PA_OPERATION_MODE mode);
	virtual READ_MODE_VALUES ReadMode();
	virtual void ReadParams(AmplifierParams *a);
	virtual int CheckResponse();
	virtual void ReadLimit(PALimits *l);
	virtual void PulseTest();
	virtual void CheckI2CDeviceExistance();
	virtual void ISS_VERSION(ISSVersionInfo *version);
	virtual void InitUSBISS();
	virtual void GET_SER_NUM(byte *data);

private:
	std::mutex m_mtx;           // mutex for critical section
	int m_inOperatingMode = -1;
	const byte BROADCAST_DEVICE_ID = 0;
	const byte m_write_i2c_address = 0x7F;
	const byte m_write_i2c_address_shift = (0x7F << 1);
	const byte m_read_i2c_address_shift = (byte)((0x7F << 1) + 1);


	std::shared_ptr<CSerialWindo> m_serial;
	
};


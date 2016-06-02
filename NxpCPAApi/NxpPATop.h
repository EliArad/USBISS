#pragma once
#include "../Common/AutoResetEvent.h"
#include "NxpPaUSBI2C.h"
#include "NxpPaUSBI2CSim.h"
#include <memory>
#include <chrono>

class CNxpPATop
{
public:
	CNxpPATop(string ComPort, bool simulator = false, int boudRate = 9600);
	virtual ~CNxpPATop();
	void Reset();
	void SetOperational();
	void showAlerts();
	void readInfo();
	void ShowMode(READ_MODE_VALUES pamode);
	void Init();
	void ReadADC(ADCReadings *a);
	void SetPhase(u16_t phaseOffset);
	void SetFrequency(float frequency);
	ushort ReadOneADC(eReadADC detector);
	void ReadSensor(SensorReadings *r);
	byte ReadOneSensor(eReadSensor sensor);
	bool ReadTXEnable();
	string ReadAlerts(string errCode);
	void ReadAlertSources(byte *result);
	void WriteParams(AmplifierParams par);
	void WriteLimit(eWriteLimits limit, byte value);
	void WriteLimit(PALimits limits);
	void SetOutputPower(byte outputPowerInPercentage);
	byte GetOutputPower();
	float GetFrequency();
	byte GetPhase();
	void ScanTime(int time);
	void Scan(float startFreq, float stopFreq, float stepFreq, byte outputPowerCode, int sleepBetween);
	void ScanFrequnecies(float startFreq, float stopFreq, float stepFreq, int sleepBetween);
	void ReadInfo(ReadInfoStruct *r);
	void ReadADC_ForRef(ADCForRedReadings *a);
	void ReadForRefSensor(SensorForRefReadings *s);
	void WriteTXEnable(bool enable);
	void SetMode(PA_OPERATION_MODE mode);
	READ_MODE_VALUES ReadMode();
	void ReadParams(AmplifierParams *a);
	int CheckResponse();
	void ReadLimit(PALimits *l);
	void PulseTest();
	void StopScan();

private:
	  std::shared_ptr<CNxpPaUSBI2C> m_pa;
	  
	  shared_ptr<thread> m_scanThread;
	  
	  byte m_outputPowerCode;

	  bool m_scanning;
	  AutoResetEvent m_scanEvent;
	  int m_scanTime;
};


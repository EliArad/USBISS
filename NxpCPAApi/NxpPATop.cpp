#include "stdafx.h"
#include "NxpPATop.h"
#include <iostream>
#include <string>
#include <thread>
#include <chrono>



using namespace std;


CNxpPATop::CNxpPATop(string ComPort, bool simulator, int boudRate) :
			m_scanning(false),
			m_scanTime(-1),
			m_scanEvent(false),
			m_outputPowerCode(0)
{
	try
	{
		if (simulator == false)
			m_pa = std::make_shared<CNxpPaUSBI2C>(ComPort, boudRate);
		else
			m_pa = std::make_shared<CNxpPaUSBI2CSim>(ComPort, boudRate);
	}
	catch (const std::exception &e)
	{
		throw (e);
	}

}


CNxpPATop::~CNxpPATop()
{
}

void CNxpPATop::Init()
{
	try
	{
		Reset();
		SetOperational();
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

void CNxpPATop::Reset()
{
	try
	{
		m_pa->Reset();
		m_pa->AssignID();
		showAlerts();
		readInfo();
		showAlerts();
		bool txEnable = m_pa->ReadTXEnable();
		showAlerts();
		PALimits l;
		m_pa->ReadLimit(&l);

		cout << l.PATemperature;
		cout << l.PACurrent;
		cout << l.VSWR;
		cout << l.MaxReflectedPower;
		cout << l.HardwareMaxReflectedPower;

		showAlerts();

		READ_MODE_VALUES pamode = m_pa->ReadMode();
		ShowMode(pamode);
		if (pamode == READ_MODE_VALUES::Standby_Mode)
			cout << "PA is standby mode" << endl;
		else if (pamode == READ_MODE_VALUES::Operating_Mode_Post_PulseTest)
			cout << "PS is in Post PuleTest" << endl;

		showAlerts();

		AmplifierParams par;
		m_pa->ReadParams(&par);

		showAlerts();
		/*
		if (m_thread == null)
		{
		m_thread = new Thread(ReadSensors);
		m_thread.Start();
		}
		if (m_threadfreqScan == null)
		{
		m_threadfreqScan = new Thread(FrequencyScan);
		m_threadfreqScan.Start();
		}
		*/
	}
	catch (string& caught)
	{
		throw string(caught);
	}
}
void CNxpPATop::ShowMode(READ_MODE_VALUES pamode)
{
	switch (pamode)
	{
	case READ_MODE_VALUES::Operating_Mode_Post_PulseTest:
		cout << "Operating_Mode_Post_PulseTest" << endl;
		break;
	case READ_MODE_VALUES::Operating_Mode_with_active_ShutdownState:
		cout << "Operating_Mode_with_active_ShutdownState" << endl;
		break;
	case READ_MODE_VALUES::Operating_Mode_Pre_PulseTest:
		cout << "Operating_Mode_Pre_PulseTest" << endl;
		break;
	case READ_MODE_VALUES::Standby_Mode:
		cout << "Standby_Mode" << endl;
		break;
	}
}

void CNxpPATop::readInfo()
{
	try
	{
		ReadInfoStruct r;
		m_pa->ReadInfo(&r);
	}
	catch (...)
	{

	}
}

void CNxpPATop::showAlerts()
{
	byte result[2];
	m_pa->ReadAlertSources(result);

	string alerts;
	string errCode;
	alerts = m_pa->ReadAlerts(errCode);
	if (errCode != "No Error")
	{
		cout << errCode;
	}
	if (alerts == "No Error")
	{
		return;
	}
	cout << alerts;
}

void CNxpPATop::SetOperational()
{
	m_pa->Reset();
}

void CNxpPATop::ReadADC(ADCReadings *a)
{
	m_pa->Reset();
}
void CNxpPATop::SetPhase(u16_t phaseOffset)
{
	m_pa->Reset();
}
void CNxpPATop::SetFrequency(float frequency)
{
	m_pa->Reset();
}
ushort CNxpPATop::ReadOneADC(eReadADC detector)
{
	return m_pa->ReadOneADC(detector);
}
void CNxpPATop::ReadSensor(SensorReadings *r)
{
	m_pa->ReadSensor(r);
}
byte CNxpPATop::ReadOneSensor(eReadSensor sensor)
{
	return m_pa->ReadOneSensor(sensor);
}
bool CNxpPATop::ReadTXEnable()
{
	return m_pa->ReadTXEnable();
}
string CNxpPATop::ReadAlerts(string errCode)
{
	return m_pa->ReadAlerts(errCode);
}
void CNxpPATop::ReadAlertSources(byte *result)
{
	m_pa->ReadAlertSources(result);
}
void CNxpPATop::WriteParams(AmplifierParams par)
{
	m_pa->WriteParams(par);
}
void CNxpPATop::WriteLimit(eWriteLimits limit, byte value)
{
	m_pa->WriteLimit(limit, value);
}
void CNxpPATop::WriteLimit(PALimits limits)
{
	m_pa->Reset();
}
void CNxpPATop::SetOutputPower(byte outputPowerInPercentage)
{
	m_pa->Reset();
}
byte CNxpPATop::GetOutputPower()
{
	return m_pa->GetOutputPower();
}
float CNxpPATop::GetFrequency()
{
	return m_pa->GetFrequency();
}
byte CNxpPATop::GetPhase()
{
	return m_pa->GetPhase();
}
void CNxpPATop::ReadInfo(ReadInfoStruct *r)
{
	m_pa->ReadInfo(r);
}
void CNxpPATop::ReadADC_ForRef(ADCForRedReadings *a)
{
	m_pa->ReadADC_ForRef(a);
}
void CNxpPATop::ReadForRefSensor(SensorForRefReadings *s)
{
	m_pa->ReadForRefSensor(s);
}
void CNxpPATop::WriteTXEnable(bool enable)
{
	m_pa->WriteTXEnable(enable);
}

void CNxpPATop::SetMode(PA_OPERATION_MODE mode)
{
	m_pa->SetMode(mode);
}
READ_MODE_VALUES CNxpPATop::ReadMode()
{
	return m_pa->ReadMode();
}
void CNxpPATop::ReadParams(AmplifierParams *a)
{
	m_pa->ReadParams(a);
}

void CNxpPATop::ReadLimit(PALimits *l)
{
	m_pa->ReadLimit(l);
}

void CNxpPATop::PulseTest()
{
	m_pa->PulseTest();
}

void CNxpPATop::ScanFrequnecies(float startFreq, 
								float stopFreq, 
								float stepFreq, 
								int sleepBetween)
{


	while (m_scanning)
	{
		if (sleepBetween > 0)
			m_scanEvent.WaitOne(sleepBetween);
		//this_thread::sleep_for(std::chrono::seconds(sleepBetween));
		for (float f = startFreq; f <= stopFreq; f += stepFreq)
		{
			m_pa->SetFrequency(f);
			this_thread::sleep_for(std::chrono::milliseconds(1));
			m_pa->SetOutputPower(m_outputPowerCode);
			this_thread::sleep_for(std::chrono::milliseconds(100));
			if (m_scanning == false)
				return;
		}
	}
}
void CNxpPATop::Scan(float startFreq, float stopFreq, float stepFreq, byte outputPowerCode, int sleepBetween)
{
	if (m_scanTime < 0)
	{
		throw string("Scan time not set , choose 0 for free running or time in seconds");
	}
	m_outputPowerCode = outputPowerCode;
	m_scanning = true;
	m_scanThread = std::make_shared<thread>(&CNxpPATop::ScanFrequnecies, this, startFreq, stopFreq, stepFreq, sleepBetween);

}
void CNxpPATop::StopScan()
{
	m_scanning = false;
	m_scanEvent.Set();
	if (m_scanThread != nullptr)
		m_scanThread->join();
}
void CNxpPATop::ScanTime(int time)
{
	m_scanTime = time;
}

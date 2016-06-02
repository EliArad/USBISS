#include "stdafx.h"
#include "NxpPaUSBI2CSim.h"
#include <string>

CNxpPaUSBI2CSim::CNxpPaUSBI2CSim(string ComPort, int boudRate) :CNxpPaUSBI2C(ComPort, boudRate)
{
	try
	{
		 
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

void CNxpPaUSBI2CSim::ISS_VERSION(ISSVersionInfo *version)
{
	byte buf[2] = { 0x5A, 0x01 };
	m_serial->Write(buf, 2);
	m_serial->Read(3, (byte *)version);
}
void CNxpPaUSBI2CSim::GET_SER_NUM(byte *data)
{
	byte buf[2] = { 0x5A, 0x03 };
	m_serial->Write(buf , 2);
	m_serial->Read(8, data);
	
}
void CNxpPaUSBI2CSim::InitUSBISS()
{
	 
	ISSVersionInfo version;
	ISS_VERSION(&version);

	if (version.moduleID != 7)
	{
		// if the module id is not that of the USB-ISS
		throw ("Device not found");
	}
	string txtMode;
	byte serialNum[8];
	GET_SER_NUM(serialNum);
	//string lblDeviceData = "USB-ISS V" + atoi(version.fwVersion) + ", SN: "; //print the software version on screen
	switch (version.operMode & 0xFE)
	{
	case (int)ISS_MODES::IO_MODE: txtMode = "IO_MODE"; break;
	case (int)ISS_MODES_I2C::I2C_H_1000KHZ: txtMode = "I2C 1MHz HW"; break;
	case (int)ISS_MODES_I2C::I2C_H_100KHZ: txtMode = "I2C 100KHz HW"; break;
	case (int)ISS_MODES_I2C::I2C_H_400KHZ: txtMode = "I2C 400KHz HW"; break;
	case (int)ISS_MODES_I2C::I2C_S_100KHZ: txtMode = "I2C 100KHz SW"; break;
	case (int)ISS_MODES_I2C::I2C_S_20KHZ: txtMode = "I2C 20KHz SW"; break;
	case (int)ISS_MODES_I2C::I2C_S_500KHZ: txtMode = "I2C 500KHz SW"; break;
	case (int)ISS_MODES_I2C::I2C_S_50KHZ: txtMode = "I2C 50KHz SW"; break;
	case (int)ISS_MODES_SPI::A2I_L: txtMode = "SPI TX on Act->Idle, Clock idle = low"; break;
	case (int)ISS_MODES_SPI::A2I_H: txtMode = "SPI TX on Act->Idle, Clock idle = high"; break;
	case (int)ISS_MODES_SPI::I2A_L: txtMode = "SPI TX on Idle->Act, Clock idle = low"; break;
	case (int)ISS_MODES_SPI::I2A_H: txtMode = "SPI TX on Idle->Act, Clock idle = high"; break;
	default: txtMode = "Unknown mode: 0x" + version.operMode; break;
	}
	if ((version.operMode & (int)ISS_MODES::SERIAL) == (int)ISS_MODES::SERIAL) txtMode += ", with Serial";
	 
	byte buf[4] = { 0x5A, 0x02, 0x40, 0xaa };
	m_serial->Write(buf, 4);
	m_serial->Read(2, buf);
}

CNxpPaUSBI2CSim::~CNxpPaUSBI2CSim()
{
	cout << "Destruct CNxpPaUSBI2C" << endl;
}

void CNxpPaUSBI2CSim::SetPhase(u16_t phaseOffset)
{
	byte phaseIndex = (byte)(phaseOffset / 5);

	if (phaseOffset > 355 || phaseOffset < 0)
	{
		throw ("Phase offset is out of range");
	}

	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}

}


void CNxpPaUSBI2CSim::WriteParams(AmplifierParams par)
{
	byte phaseIndex = (byte)(par.Phase / 5);

	if (phaseIndex > 71 || phaseIndex < 0)
	{
		throw ("Phase offset is out of range");
	}

	byte freqIndex = (byte)((par.Frequency - 2386.0) * 2);
	if (freqIndex > 254)
	{
		throw ("frequnecy is out of range");
	}

	if (par.OutputPower > 100)
	{
		throw ("OutputPower is out of range");
	}

	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
} 

void CNxpPaUSBI2CSim::SetFrequency(float frequency)
{
	byte freqIndex = (byte)((frequency - 2386.0) * 2);

	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
 
void CNxpPaUSBI2CSim::WriteLimit(eWriteLimits limit, byte value)
{
	try
	{
		byte i = 0;
		byte SerBuf[15];

		SerBuf[i++] = (byte)((byte)eOPCODES::WriteLimit | (byte)limit);
		SerBuf[i++] = value;
		m_serial->Write(SerBuf, i);
		return;
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

void CNxpPaUSBI2CSim::WriteLimit(PALimits limits)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::SetOutputPower(byte outputPowerInPercentage)
{


	if (outputPowerInPercentage < 0 || outputPowerInPercentage > 100)
		throw ("outputPowerInPercentage  range is 0 - 100");

	try
	{
		 
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

byte CNxpPaUSBI2CSim::GetOutputPower()
{
	try
	{
		return 0;
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

float CNxpPaUSBI2CSim::GetFrequency()
{
	try
	{
		return 2400;
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

byte CNxpPaUSBI2CSim::GetPhase()
{
	try
	{
		return 0;
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

void CNxpPaUSBI2CSim::Reset()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
void CNxpPaUSBI2CSim::PulseTest()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
void CNxpPaUSBI2CSim::CheckI2CDeviceExistance()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::ReadLimit(PALimits *l)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::ReadParams(AmplifierParams *a)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::ReadADC(ADCReadings *r)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::ReadADC_ForRef(ADCForRedReadings *a)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 		 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

ushort CNxpPaUSBI2CSim::ReadOneADC(eReadADC detector)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
void CNxpPaUSBI2CSim::ReadSensor(SensorReadings *r)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::ReadForRefSensor(SensorForRefReadings *s)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}


byte CNxpPaUSBI2CSim::ReadOneSensor(eReadSensor sensor)
{

	std::lock_guard<std::mutex> lock(m_mtx);
	{

		try
		{
			return 0;
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::ReadAlertSources(byte *result)
{

	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}


bool CNxpPaUSBI2CSim::ReadTXEnable()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

int CNxpPaUSBI2CSim::CheckResponse()
{
	return 0;
}

string CNxpPaUSBI2CSim::ReadAlerts(string errCode)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{			 
		    return "No Error";
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::ReadInfo(ReadInfoStruct *r)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
void CNxpPaUSBI2CSim::WriteTXEnable(bool enable)
{
	if (m_inOperatingMode != 1)
	{
		throw ("Module is not in operating mode");
	}

	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::SetMode(PA_OPERATION_MODE mode)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2CSim::AssignID()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

READ_MODE_VALUES CNxpPaUSBI2CSim::ReadMode()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			  
			return READ_MODE_VALUES::Operating_Mode_Post_PulseTest;

		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

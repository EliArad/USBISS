#include "stdafx.h"
#include "NxpPaUSBI2C.h"
#include <string>

CNxpPaUSBI2C::CNxpPaUSBI2C(string ComPort, int boudRate)
{
	try
	{
		m_serial = std::make_shared<CSerialWindo>(ComPort, boudRate);
		InitUSBISS();
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

void CNxpPaUSBI2C::ISS_VERSION(ISSVersionInfo *version)
{
	byte buf[2] = { 0x5A, 0x01 };
	m_serial->Write(buf, 2);
	m_serial->Read(3, (byte *)version);
}
void CNxpPaUSBI2C::GET_SER_NUM(byte *data)
{
	byte buf[2] = { 0x5A, 0x03 };
	m_serial->Write(buf , 2);
	m_serial->Read(8, data);
	
}
void CNxpPaUSBI2C::InitUSBISS()
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

CNxpPaUSBI2C::~CNxpPaUSBI2C()
{
	cout << "Destruct CNxpPaUSBI2C" << endl;
}

void CNxpPaUSBI2C::SetPhase(u16_t phaseOffset)
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
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 2;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::WriteParam | (byte)eWriteParams::Phase;
			SerBuf[i++] = phaseIndex;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			CheckResponse();
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}

}


void CNxpPaUSBI2C::WriteParams(AmplifierParams par)
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
			byte i = 0;
			byte SerBuf[20];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 5;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::WriteParam | (byte)0xF;
			SerBuf[i++] = phaseIndex;
			SerBuf[i++] = freqIndex;
			SerBuf[i++] = par.OutputPower;
			SerBuf[i++] = 0;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			CheckResponse();
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
} 

void CNxpPaUSBI2C::SetFrequency(float frequency)
{
	byte freqIndex = (byte)((frequency - 2386.0) * 2);

	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 2;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::WriteParam | (byte)eWriteParams::Frequency;
			SerBuf[i++] = freqIndex;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			CheckResponse();
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
 
void CNxpPaUSBI2C::WriteLimit(eWriteLimits limit, byte value)
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

void CNxpPaUSBI2C::WriteLimit(PALimits limits)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[20];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 6;       // write 1+1=2 bytes
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)((byte)eOPCODES::WriteLimit | (byte)0x1F);
			SerBuf[i++] = limits.PATemperature;
			SerBuf[i++] = limits.PACurrent;
			SerBuf[i++] = limits.VSWR;
			SerBuf[i++] = limits.MaxReflectedPower;
			SerBuf[i++] = limits.HardwareMaxReflectedPower;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::SetOutputPower(byte outputPowerInPercentage)
{


	if (outputPowerInPercentage < 0 || outputPowerInPercentage > 100)
		throw ("outputPowerInPercentage  range is 0 - 100");

	try
	{
		byte i = 0;
		byte SerBuf[15];
		SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
		SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
		SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 2;       // write 1+1=2 bytes
		SerBuf[i++] = m_write_i2c_address_shift;
		SerBuf[i++] = (byte)eOPCODES::WriteParam | (byte)eWriteParams::OutputPower;
		SerBuf[i++] = outputPowerInPercentage;
		SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
		m_serial->Write(SerBuf, i);
		int toRead = CheckResponse();
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

byte CNxpPaUSBI2C::GetOutputPower()
{
	try
	{
		byte i = 0;
		byte SerBuf[15];
		SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
		SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
		SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;       // write 1+1=2 bytes
		SerBuf[i++] = m_write_i2c_address_shift;
		SerBuf[i++] = (byte)eOPCODES::ReadParam | (byte)eWriteParams::OutputPower;

		SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
		SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
		SerBuf[i++] = m_read_i2c_address_shift;
		SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
		SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
		SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
		m_serial->Write(SerBuf, i);
		int toRead = CheckResponse();
		m_serial->Read(toRead, SerBuf);
		return SerBuf[0];
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

float CNxpPaUSBI2C::GetFrequency()
{
	try
	{
		byte i = 0;
		byte SerBuf[15];
		SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
		SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
		SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;       // write 1+1=2 bytes
		SerBuf[i++] = m_write_i2c_address_shift;
		SerBuf[i++] = (byte)eOPCODES::ReadParam | (byte)eWriteParams::Frequency;


		SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
		SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
		SerBuf[i++] = m_read_i2c_address_shift;
		SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
		SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
		SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
		m_serial->Write(SerBuf, i);
		int toRead = CheckResponse();
		m_serial->Read(toRead, SerBuf);

		float freq = (float)(2380 + SerBuf[0] * 0.5);
		return freq;
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

byte CNxpPaUSBI2C::GetPhase()
{
	try
	{
		byte i = 0;
		byte SerBuf[15];
		SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
		SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
		SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;       // write 1+1=2 bytes
		SerBuf[i++] = m_write_i2c_address_shift;
		SerBuf[i++] = (byte)eOPCODES::ReadParam | (byte)eWriteParams::Phase;


		SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
		SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
		SerBuf[i++] = m_read_i2c_address_shift;
		SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
		SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
		SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
		m_serial->Write(SerBuf, i);
		int toRead = CheckResponse();
		m_serial->Read(toRead, SerBuf);
		return SerBuf[0];
	}
	catch (const std::exception &e)
	{
		throw (e);
	}
}

void CNxpPaUSBI2C::Reset()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;       // write 1+1=2 bytes
			SerBuf[i++] = BROADCAST_DEVICE_ID;
			SerBuf[i++] = (byte)eOPCODES::Reset;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;

			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			Sleep(300);
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
void CNxpPaUSBI2C::PulseTest()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;       // write 1+1=2 bytes
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::PulseTest;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;

			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			Sleep(200);
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
void CNxpPaUSBI2C::CheckI2CDeviceExistance()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)0x58;
			SerBuf[i++] = m_read_i2c_address_shift;
			m_serial->Write(SerBuf, i);
			m_serial->Read(1, SerBuf);
			if (SerBuf[0] == 0)
			{
				throw ("No device is detected");
			}
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::ReadLimit(PALimits *l)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			 
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::ReadLimit | 0x1F;
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD + 3;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			memcpy(l, (PALimits *)SerBuf, sizeof(PALimits));
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::ReadParams(AmplifierParams *a)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)((byte)eOPCODES::ReadParam | 0xF);
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD + 2;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			ReadParamsStruct *r = (ReadParamsStruct *)SerBuf;
			a->OutputPower = r->OutputPower;
			float freq = (float)(2386 + (r->Frequency * 0.5));
			a->Frequency = freq;
			a->Phase = (short)(r->Phase * 5);
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::ReadADC(ADCReadings *r)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)((byte)eOPCODES::ReadADC | (byte)0x3F);
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD + 10;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);		
			memcpy(r, (ADCReadings *)SerBuf, sizeof(ADCReadings));
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::ReadADC_ForRef(ADCForRedReadings *a)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)((byte)eOPCODES::ReadADC | (byte)eReadADC::ReflectedPower | (byte)eReadADC::ForwardPower);
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD + 2;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			memcpy(a, (ADCForRedReadings *)SerBuf, sizeof(ADCForRedReadings));			 
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

ushort CNxpPaUSBI2C::ReadOneADC(eReadADC detector)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)((byte)eOPCODES::ReadADC | (byte)(detector));
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			return Helper::GetShort(SerBuf);
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
void CNxpPaUSBI2C::ReadSensor(SensorReadings *r)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::ReadSensor | (byte)0x3F; // all bits
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD + 4;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			memcpy(r, (SensorReadings *)SerBuf, sizeof(SensorReadings));
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::ReadForRefSensor(SensorForRefReadings *s)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::ReadSensor | ((byte)eReadSensor::ForwardPower | (byte)eReadSensor::ReflectedPower);
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD + 2;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			memcpy(s, SerBuf, sizeof(SensorForRefReadings));
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}


byte CNxpPaUSBI2C::ReadOneSensor(eReadSensor sensor)
{

	std::lock_guard<std::mutex> lock(m_mtx);
	{

		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)((byte)eOPCODES::ReadSensor | (byte)(sensor));
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			return SerBuf[0];
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::ReadAlertSources(byte *result)
{

	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::ReadAlertSources;
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, result);
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}


bool CNxpPaUSBI2C::ReadTXEnable()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;
			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::ReadTXEnable;
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			return SerBuf[0] == 1 ? true : false;
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

int CNxpPaUSBI2C::CheckResponse()
{
	byte SerBuf[15];
	m_serial->Read(2, SerBuf);
	if (SerBuf[0] == 0)
	{
		switch (SerBuf[1])
		{
		case 0x1:
			throw string("Device Error	0x01	No ACK from device");

		case 0x2:
			throw string("Buffer Overflow	0x02	You must limit the frame to < 60 bytes");

		case 0x3:
			throw string("Buffer Underflow	0x03	More write data was expected than sent");

		case 0x4:
			throw string("Unknown command	0x04	Probably your write count is wrong");
		default:
			throw ("unknown error");
		}
	}
	else
	{
		return SerBuf[1];
	}
}

string CNxpPaUSBI2C::ReadAlerts(string errCode)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			int i = 0;
			byte SerBuf[40];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;       // write 1+1=2 bytes
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::ReadAlerts;
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD + 1;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			Sleep(10);
			SerBuf[0] = 0x0;
			int toRead = CheckResponse();
			m_serial->Read(toRead, SerBuf);
			errCode = CErrorCodes::GetError(SerBuf[2]);
			if (errCode != "No Error")
			{
				cout << errCode;
			}
			if (SerBuf[0] != 0xFF)
			{
				return CErrorCodes::getAlertReasonBit(SerBuf[0]);
			}
			else
				return "No Error";
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::ReadInfo(ReadInfoStruct *r)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;

			byte SerBuf[40];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;       // write 1+1=2 bytes
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::ReadInfo;
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD + 10;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int readNo = CheckResponse();
			m_serial->Read(readNo, SerBuf);
			memcpy(r, (ReadInfoStruct*)SerBuf, sizeof(ReadInfoStruct));
			if (r->Type != 1)
			{
				throw ("Type expected to be 1 indicates its a 250W MWO module");
			}
			if (r->Protocol != 1)
			{
				throw ("Communication protocol version invalid. This will always read as 1");
			}
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}
void CNxpPaUSBI2C::WriteTXEnable(bool enable)
{
	if (m_inOperatingMode != 1)
	{
		throw ("Module is not in operating mode");
	}

	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;

			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 2;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::WriteTXEnable;
			SerBuf[i++] = (byte)(enable == true ? 1 : 0);
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			CheckResponse();
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::SetMode(PA_OPERATION_MODE mode)
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;

			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 2;
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::SetMode;
			SerBuf[i++] = (byte)mode;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			CheckResponse();
			Sleep(200);
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

void CNxpPaUSBI2C::AssignID()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;

			byte SerBuf[15];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 2;       // write 1+1=2 bytes
			SerBuf[i++] = BROADCAST_DEVICE_ID;
			SerBuf[i++] = (byte)eOPCODES::AssignID;
			SerBuf[i++] = (byte)0x7F;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			CheckResponse();
		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

READ_MODE_VALUES CNxpPaUSBI2C::ReadMode()
{
	std::lock_guard<std::mutex> lock(m_mtx);
	{
		try
		{
			byte i = 0;

			byte SerBuf[40];
			SerBuf[i++] = (byte)IssCmds::I2C_DIRECT;
			SerBuf[i++] = (byte)I2Cdirect::I2CSTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE + 1;       // write 1+1=2 bytes
			SerBuf[i++] = m_write_i2c_address_shift;
			SerBuf[i++] = (byte)eOPCODES::ReadMode;
			SerBuf[i++] = (byte)I2Cdirect::I2CRESTART;
			SerBuf[i++] = (byte)I2Cdirect::I2CWRITE;
			SerBuf[i++] = m_read_i2c_address_shift;
			SerBuf[i++] = (byte)I2Cdirect::I2CNACK;
			SerBuf[i++] = (byte)I2Cdirect::I2CREAD; // read one back byte only
			SerBuf[i++] = (byte)I2Cdirect::I2CSTOP;
			m_serial->Write(SerBuf, i);
			int readNo = CheckResponse();
			m_serial->Read(readNo, SerBuf);
			READ_MODE_VALUES mode = (READ_MODE_VALUES)SerBuf[0];
			if (mode != READ_MODE_VALUES::Standby_Mode)
			{
				m_inOperatingMode = 1;
			}
			else
			{
				m_inOperatingMode = 0;
			}
			return mode;

		}
		catch (const std::exception &e)
		{
			throw (e);
		}
	}
}

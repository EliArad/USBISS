#include "stdafx.h"
#include "ErrorCodes.h"


std::map<int, string> CErrorCodes::m_errCodes;   // initialize static member here



CErrorCodes::CErrorCodes()
{

	// first insert function version (single parameter):
	m_errCodes.insert(std::pair<int, string>(0, "No Error"));
	m_errCodes.insert(std::pair<int, string>(1, "Illegal Command – the command byte is not recognized"));
	m_errCodes.insert(std::pair<int, string>(2, "Illegal Command Format – an I2C read was attempted without having set the            command using an I2C write"));
	m_errCodes.insert(std::pair<int, string>(3, "Incorrect Byte Count – the number of data bytes written does not match the command"));
	m_errCodes.insert(std::pair<int, string>(4, "Invalid State – the requested command cannot be executed in the current state / mode"));
	m_errCodes.insert(std::pair<int, string>(5, "Illegal Mode – the mode requested by the Set Mode command is not supported"));
	m_errCodes.insert(std::pair<int, string>(6, "Invalid I2C Address – the I2C address supplied in the Assign ID command is less than 2 or is more than 127."));
	m_errCodes.insert(std::pair<int, string>(7, "Invalid DAC Bit – the Write DAC command attempted to write to an unsupported reserved DAC channel"));
	m_errCodes.insert(std::pair<int, string>(8, "Invalid Parameter Bit – the Write Param command attempted to write to anunsupported / reserved parameter"));
	m_errCodes.insert(std::pair<int, string>(9, "Invalid Parameter Value – the Write Param command attempted to write a parameter value that was outside the supported range"));
	m_errCodes.insert(std::pair<int, string>(10, "CRC Error – the checksum for the EEPROM data in the Write EEPROM command data did not match"));
	m_errCodes.insert(std::pair<int, string>(11, "EEPROM Locked – the Write EEPROM data command attempted to write to the EEPROM but it is locked"));
	m_errCodes.insert(std::pair<int, string>(12, "Pulse Test, SW Check – failure detected during the Pulse Test"));
	m_errCodes.insert(std::pair<int, string>(13, "Pulse Test, PLL Lock – failure detected during the Pulse Test"));
	m_errCodes.insert(std::pair<int, string>(14, "Reserved"));
	m_errCodes.insert(std::pair<int, string>(15, "Shutdown – a command was received to modify the RF output but the module is in Shutdown state so the command was not performed"));
	m_errCodes.insert(std::pair<int, string>(16, "Incorrect Read Byte Count – the number of bytes read exceeds the requested data size"));
	m_errCodes.insert(std::pair<int, string>(17, "NVM Size – attempted to read EEPROM Data from beyond the EEPROM size"));
	m_errCodes.insert(std::pair<int, string>(18, "NVM Erase – An internal error occurred while erasing the EEPROM"));
	m_errCodes.insert(std::pair<int, string>(19, "NVM Write – An internal error occurred while writing EEPROM data to the NVM"));
	m_errCodes.insert(std::pair<int, string>(20, "NVM Invalid – Attempted to perform Pulse Test but the EEPROM contents are invalid"));
}


CErrorCodes::~CErrorCodes()
{
}
	 
string CErrorCodes::GetError(int id)
{
	std::map<int, string>::iterator it;
	it = m_errCodes.find(id);
	if (it != m_errCodes.end())
		return(it->second);

	throw ("Error, not id match in the list");
}
string CErrorCodes::getAlertReasonBit(u8_t alert)
{
	string msg = "";

	if ((alert & 0x1) == 0x0)
	{
		msg += "TEMP / VOLT \nPA exceeds the PA temperature\n"
			"limit, or the PA voltage minimum and maximum limits, at\n"
			"which time the PA is also powered down. This bit is cleared\n";
	}

	if ((alert & 0x2) == 0x0)
	{
		msg += "\nIDD\n"
			"PA exceeds the current limit, at\n"\
			"which time the PA is also powered down. This bit is cleared\n"\
			"when the Alert register is read\n";
	}

	if ((alert & 0x4) == 0x0)
	{
		msg += "\nVSWR\n"\
			"VSWR limit, at\n"
			"which time the PA is also powered down. This bit is cleared\n"\
			"when the Alert register is read.\n";
	}

	if ((alert & 0x8) == 0x0)
	{
		msg += "\nREF_PWR\n \
			reflected power\n\
			limit, at which time the PA is also powered down.This bit is\n\
			cleared when the Alert register is read.";
	}

	if ((alert & 0x10) == 0x0)
	{
		msg += "\nHW_PWR"\
			"This bit is set active when the PA exceeds the hardware\n" \
			"reflected power limit, at which time the PA is also powered \n"\
			"down and the SHUTDOWN_B signal is asserted low to shut\n"\
			"down all other modules. This bit is cleared when the Alert\n "\
			"register is read\n";
	}


	if ((alert & 0x20) == 0)
	{
		msg += "PULSE_FAIL\n"
			"Failure is detected during the pulse\n"
			"test. This bit is cleared when the Alert register is read.\n";
	}

	if ((alert & 0x40) == 0)
	{
		msg += "CMD_ERR\n"\
			"When a command error is detected by the slave module this\n"
			"bit is set active and the command is ignored. Possible causes\n"
			"of a command error include:\n"
			"- Invalid command byte\n"\
			"- Broadcast of a command that does not support it\n"
			"- Too few or too many data bytes for a command\n"
			"- I2C read is not preceded by an I2C write\n"
			"This bit is cleared when the Alert register is read\n";
	}
	if ((alert & 0x80) == 0)
	{
		msg = "NEED_ID This bit is set active when the module completes its startup\n";
			  "sequence and is ready for an ID to be assigned. The bit is\n"\
			  "cleared when the module ID is successfully assigned with the\n"\
			  "Assign ID command.\n";
	}

	return msg;
}

 
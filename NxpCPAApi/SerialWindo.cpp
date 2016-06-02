#include "stdafx.h"
#include "SerialWindo.h"
#include <Windows.h>

#pragma warning(disable: 4996)

CSerialWindo::CSerialWindo(string ComPort, int baudrate)
{

	// Open serial port
	char com[100];
	sprintf(com, "\\\\.\\%s", ComPort.c_str());
	m_serialHandle  = CreateFile(com, GENERIC_READ | GENERIC_WRITE, 0, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
	if (m_serialHandle == NULL)
	{
		throw ("Cannot open com port " + ComPort);
	}

	// Do some basic settings
	DCB serialParams = { 0 };
	serialParams.DCBlength = sizeof(serialParams);

	GetCommState(m_serialHandle, &serialParams);
	serialParams.BaudRate = baudrate;
	serialParams.ByteSize = 8;
	serialParams.StopBits = 1;
	serialParams.Parity = 0;
	SetCommState(m_serialHandle, &serialParams);

	// Set timeouts
	COMMTIMEOUTS timeout = { 0 };
	timeout.ReadIntervalTimeout = 50;
	timeout.ReadTotalTimeoutConstant = 50;
	timeout.ReadTotalTimeoutMultiplier = 50;
	timeout.WriteTotalTimeoutConstant = 50;
	timeout.WriteTotalTimeoutMultiplier = 10;
	SetCommTimeouts(m_serialHandle, &timeout);
}

CSerialWindo::~CSerialWindo()
{
	Close();
}

void CSerialWindo::Write(byte *SerBuf, int size)
{
	DWORD dwBytesWritten;
	int isWritten = WriteFile(m_serialHandle, SerBuf, size, (DWORD *)&dwBytesWritten, NULL);
	if (dwBytesWritten != size || isWritten == FALSE)
	{
		throw ("Error write to serial");
	}

}
void CSerialWindo::Read(int size, byte *buf)
{
	DWORD dwBytesRead = 0;
	BOOL isRead = ReadFile(m_serialHandle, buf, size, (DWORD *)&dwBytesRead, NULL);
	if (dwBytesRead != size)
	{
		throw ("Error in serial read");
	}

}
void CSerialWindo::Close()
{
	if (m_serialHandle != 0)
		CloseHandle(m_serialHandle);
	m_serialHandle = 0;
}
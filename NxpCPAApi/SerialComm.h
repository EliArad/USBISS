#pragma once

#include "types.h"
#include <Windows.h>

class SerialComm
{
public:
	SerialComm();
	virtual ~SerialComm();
	virtual void Write(byte *SerBuf, int size) = 0;
	virtual void Read(int size, byte *buf) = 0;
	virtual void Close() = 0;


protected:
	HANDLE m_serialHandle;
};


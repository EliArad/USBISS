#pragma once
#include "SerialComm.h"
#include <string>
using namespace std;

class CSerialWindo : SerialComm
{
public:
	CSerialWindo(string ComPort, int baudrate = 9600);
	virtual ~CSerialWindo();

	virtual void Write(byte *SerBuf, int size);
	virtual void Read(int size, byte *buf);
	virtual void Close();


};


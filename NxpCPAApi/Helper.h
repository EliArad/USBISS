#pragma once 

#include "types.h"

class Helper
{
public:
	static ushort GetShort(byte *buf)
	{
		return (buf[1] << 8) | buf[0];
	}

};
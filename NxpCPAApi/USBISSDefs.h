#pragma once

// I2C DIRECT commands
enum class I2Cdirect
{
	I2CSRP = 0x00,			// Start/Stop Codes - 0x01=start, 0x02=restart, 0x03=stop, 0x04=nack
	I2CSTART,				// send start sequence
	I2CRESTART,				// send restart sequence
	I2CSTOP,				// send stop sequence
	I2CNACK,				// send NACK after next read
	I2CREAD = 0x20,		    // 0x20-0x2f, reads 1-16 bytes
	I2CWRITE = 0x30,		// 0x30-0x3f, writes next 1-16 bytes
};
enum class IssCmds
{
	ISS_VER = 1, 			// returns version num, 1 byte
	ISS_MODE,				// returns ACK, NACK, 1 byte
	GET_SER_NUM,

	I2C_SGL = 0x53,		    // 0x53 Read/Write single byte for non-registered devices
	I2C_AD0,				// 0x54 Read/Write multiple bytes for devices without internal address register
	I2C_AD1,				// 0x55 Read/Write multiple bytes for 1 byte addressed devices 
	I2C_AD2,				// 0x56 Read/Write multiple bytes for 2 byte addressed devices
	I2C_DIRECT,				// 0x57 Direct control of I2C start, stop, read, write.
	ISS_CMD = 0x5A,		    // 0x5A 
	SPI_IO = 0x61,			// 0x61 SPI I/O
	SERIAL_IO,              // 0x62
	SETPINS,				// 0x63 [SETPINS] [pin states]
	GETPINS,				// 0x64 
	GETAD,					// 0x65 [GETAD] [pin to convert]
};


enum class ISS_MODES { IO_MODE = 0x00, IO_CHANGE = 0x10, SERIAL = 0x01 };
enum class ISS_MODES_I2C {
	I2C_S_20KHZ = 0x20, I2C_S_50KHZ = 0x30,
	I2C_S_100KHZ = 0x40, I2C_S_500KHZ = 0x50, I2C_H_100KHZ = 0x60,
	I2C_H_400KHZ = 0x70, I2C_H_1000KHZ = 0x80
};
enum class ISS_MODES_SPI { A2I_L = 0x90, A2I_H = 0x91, I2A_L = 0x92, I2A_H = 0x93 };
enum class IO_TYPES { OUT_LOW = 0x00, OUT_HIGH = 0x01, IN_DIG = 0x02, IN_ANA = 0x03 };
enum class SERIAL_BAUD_RATES { _300 = 9999, _1200 = 2499, _2400 = 1249, _9600 = 311, _19200 = 155, _38400 = 77, _57600 = 51, _115200 = 25, _250000 = 11, _1000000 = 3 };
enum class RESULTS { SUCCESS = 0x00, UNK_CMD = 0x05, INT_ERR1 = 0x06, INT_ERR2 = 0x07 };

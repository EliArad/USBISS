#pragma once
#include <string>
#include <map>


using namespace std;


 

class CErrorCodes
{
public:
	CErrorCodes();
	~CErrorCodes();

    static std::map<int, string> m_errCodes;

	static string GetError(int id);
	static string getAlertReasonBit(u8_t alert);

private:
   

	

};


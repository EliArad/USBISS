// NxpCPAApi.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "NxpPATop.h"
#include <string>
#include <iostream>
#include <conio.h>
#include <stdio.h>

using namespace std;

int _tmain(int argc, _TCHAR* argv[])
{
	try
	{
		CNxpPATop  pa("COM31", true);
		pa.Init();
		pa.ScanTime(10);
		pa.Scan(2400, 2500, 0.5, 100, 100);
		
		getchar();
		pa.StopScan();
	}
	catch (string& caught)
	{
		cout << caught << endl;
	}

	return 0;
}


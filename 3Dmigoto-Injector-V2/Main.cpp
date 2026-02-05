// Injector.cpp : Defines the entry point for the console application.
//


#include <windows.h>
#include <stdio.h>
#include <tlhelp32.h>
#include <set>
#include <string>

#include "D3dxIniUtils.hpp"
#include "InjectorUtils.hpp"
#include "NtInjectorUtils.hpp"
#include "HybridNtInjectorUtils.hpp"

int main()
{
	int rc = EXIT_FAILURE;

	CreateMutexA(0, FALSE, "Local\\3DMigotoLoader");
	if (GetLastError() == ERROR_ALREADY_EXISTS)
		InjectorUtils::WaitExit(EXIT_FAILURE, "ERROR: Another instance of the 3DMigoto Loader is already running. Please close it and try again\n");

	printf("\n------------------------------- 3DMigoto Loader V3-001 ------------------------------\n\n");
	printf("This program is modified by NicoMico based on the 3Dmigoto source code and is exclusively included in the SSMT Release for free distribution. It is completely free! If you paid any amount of money to obtain it, you have definitely been scammed!\n\n");
	printf("\n----------------------------------------------------------------------------------\n\n");

	/*
		In China, resell open source tool is very common issue,
		but most of the scene is resell the compiled binary directly without any modification,
		so add a tip here is necessary.
		
		Most of people who can compile C++ code by themselves will not resell the tool in most case.
		So the tip is important to prevent some annoying people, and also let people know this tool is free.

		Notice some computer can't show Chinese character, so we just use English here.
	*/


	D3dxIniUtils d3dxIniUtils(L"d3dx.ini");
	
	if (d3dxIniUtils.parse_error != L"") {
		InjectorUtils::WaitExit(EXIT_FAILURE, d3dxIniUtils.ToByteString(d3dxIniUtils.parse_error).c_str());
	}


	// =========================================================================================
	// INJECTION METHODS - Choose ONE by commenting/uncommenting
	// =========================================================================================

	// METHOD 1: Classic Win32 API Injection (Includes SetWindowsHookEx support)
	// -----------------------------------------------------------------------------------------
	// InjectorUtils::Run(d3dxIniUtils);


	// METHOD 2: NT API Injection (Create Suspended Process -> NT Inject -> Resume)
	// -----------------------------------------------------------------------------------------
	// NtInjectorUtils::Run(d3dxIniUtils);

	// METHOD 3: Hybrid NT + CBT Hook Injection
	// -----------------------------------------------------------------------------------------
	HybridNtInjectorUtils::Run(d3dxIniUtils);
	
	// =========================================================================================

	return EXIT_SUCCESS;
}


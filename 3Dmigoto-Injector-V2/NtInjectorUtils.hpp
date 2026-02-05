#pragma once

#include <windows.h>
#include <stdio.h>
#include <string>
#include "D3dxIniUtils.hpp"

// Define NT function prototypes and types manually to avoid linking issues
typedef struct _UNICODE_STRING {
    USHORT Length;
    USHORT MaximumLength;
    PWSTR  Buffer;
} UNICODE_STRING, * PUNICODE_STRING;

typedef struct _OBJECT_ATTRIBUTES {
    ULONG           Length;
    HANDLE          RootDirectory;
    PUNICODE_STRING ObjectName;
    ULONG           Attributes;
    PVOID           SecurityDescriptor;
    PVOID           SecurityQualityOfService;
} OBJECT_ATTRIBUTES, * POBJECT_ATTRIBUTES;

typedef struct _CLIENT_ID {
    HANDLE UniqueProcess;
    HANDLE UniqueThread;
} CLIENT_ID, * PCLIENT_ID;

typedef NTSTATUS(NTAPI* pNtCreateThreadEx)(
    PHANDLE ThreadHandle,
    ACCESS_MASK DesiredAccess,
    PVOID ObjectAttributes,
    HANDLE ProcessHandle,
    PVOID StartRoutine,
    PVOID Argument,
    ULONG CreateFlags,
    ULONG_PTR ZeroBits,
    SIZE_T StackSize,
    SIZE_T MaximumStackSize,
    PVOID AttributeList
    );

typedef NTSTATUS(NTAPI* pNtAllocateVirtualMemory)(
    HANDLE ProcessHandle,
    PVOID* BaseAddress,
    ULONG_PTR ZeroBits,
    PSIZE_T RegionSize,
    ULONG AllocationType,
    ULONG Protect
    );

typedef NTSTATUS(NTAPI* pNtWriteVirtualMemory)(
    HANDLE ProcessHandle,
    PVOID BaseAddress,
    PVOID Buffer,
    SIZE_T NumberOfBytesToWrite,
    PSIZE_T NumberOfBytesWritten
    );

typedef NTSTATUS(NTAPI* pNtOpenProcess)(
    PHANDLE ProcessHandle,
    ACCESS_MASK DesiredAccess,
    POBJECT_ATTRIBUTES ObjectAttributes,
    PCLIENT_ID ClientId
    );

class NtInjectorUtils {
public:
    static void Run(D3dxIniUtils& ini) {
        printf("[NT MODE] Starting NT Injection Sequence...\n");

        if (ini.launch != L"") {
            LaunchAndInject(ini);
        }
        else {
            printf("[NT MODE] Waiting for target process not fully implemented in this simplified NT class.\n"
                "Please configure 'launch' in d3dx.ini to use NT Injection for process creation.\n");
            // Fallback or exit?
            // User asked mainly for "create a target process", so LaunchAndInject is the key.
            system("pause");
        }
    }

private:
   static void LaunchAndInject(D3dxIniUtils& ini) {
        STARTUPINFOW si = { sizeof(si) };
        PROCESS_INFORMATION pi = { 0 };
        
        wchar_t run_path[MAX_PATH];
        wchar_t run_args[MAX_PATH];
        wchar_t working_dir[MAX_PATH];

        wcsncpy_s(run_path, MAX_PATH, ini.launch.c_str(), _TRUNCATE);
        wcsncpy_s(run_args, MAX_PATH, ini.launch_args.c_str(), _TRUNCATE);

        // Derive working directory
        wchar_t* file_part = NULL;
        GetFullPathNameW(run_path, MAX_PATH, working_dir, &file_part);
        if (file_part) *file_part = '\0';
        
        printf("[NT MODE] Creating Process (Suspended): %S\n", run_path);

        // We use Win32 CreateProcess for creation as NtCreateUserProcess is overkill/unstable across versions
        // But we keep it suspended to use NT functions for injection.
        if (!CreateProcessW(run_path, run_args, NULL, NULL, FALSE, CREATE_SUSPENDED, NULL, working_dir, &si, &pi)) {
            printf("[NT MODE] Failed to create process. Error: %d\n", GetLastError());
            return;
        }

        printf("[NT MODE] Process Created. PID: %d. Injecting DLLs...\n", pi.dwProcessId);

        // Inject Modules using NT functions
        bool d3d11_ok = NtInjectDll(pi.hProcess, ini.module.c_str());
        bool extra_ok = true;
        if (ini.inject_dll != L"") {
             extra_ok = NtInjectDll(pi.hProcess, ini.inject_dll.c_str());
        }

        if (d3d11_ok && extra_ok) {
            printf("[NT MODE] Injection Successful. Resuming Process...\n");
        }
        else {
            printf("[NT MODE] Creating Process Failed during injection.\n");
            TerminateProcess(pi.hProcess, 0);
            return;
        }

        ResumeThread(pi.hThread);
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        printf("[NT MODE] Done.\n");
    }

    static bool NtInjectDll(HANDLE hProcess, const wchar_t* dllPath) {
        HMODULE hNtdll = GetModuleHandleW(L"ntdll.dll");
        if (!hNtdll) return false;

        auto NtAllocateVirtualMemory = (pNtAllocateVirtualMemory)GetProcAddress(hNtdll, "NtAllocateVirtualMemory");
        auto NtWriteVirtualMemory = (pNtWriteVirtualMemory)GetProcAddress(hNtdll, "NtWriteVirtualMemory");
        auto NtCreateThreadEx = (pNtCreateThreadEx)GetProcAddress(hNtdll, "NtCreateThreadEx");

        if (!NtAllocateVirtualMemory || !NtWriteVirtualMemory || !NtCreateThreadEx) {
            printf("[NT MODE] Failed to resolve NT functions.\n");
            return false;
        }

        PVOID remoteMem = NULL;
        SIZE_T zeroBits = 0;
        SIZE_T pathSize = (wcslen(dllPath) + 1) * sizeof(wchar_t);
        
        // NT Memory Allocation
        NTSTATUS status = NtAllocateVirtualMemory(hProcess, &remoteMem, 0, &pathSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (status != 0) {
            printf("[NT MODE] NtAllocateVirtualMemory failed. Status: 0x%X\n", status);
            return false;
        }

        // NT Memory Write
        SIZE_T bytesWritten = 0;
        status = NtWriteVirtualMemory(hProcess, remoteMem, (PVOID)dllPath, pathSize, &bytesWritten);
        if (status != 0) {
            printf("[NT MODE] NtWriteVirtualMemory failed. Status: 0x%X\n", status);
            return false;
        }

        // Get LoadLibraryW address (kernel32 is loaded at same address in all processes typically)
        PVOID loadLibraryAddr = (PVOID)GetProcAddress(GetModuleHandleW(L"kernel32.dll"), "LoadLibraryW");
        
        // NT Create Thread
        HANDLE hThread = NULL;
        status = NtCreateThreadEx(&hThread, PROCESS_ALL_ACCESS, NULL, hProcess, loadLibraryAddr, remoteMem, 0, 0, 0, 0, NULL);
        if (status != 0 || !hThread) {
            printf("[NT MODE] NtCreateThreadEx failed. Status: 0x%X\n", status);
            return false;
        }

        printf("[NT MODE] Injected: %S\n", dllPath);

        WaitForSingleObject(hThread, INFINITE);
        CloseHandle(hThread);

        return true;
    }
};

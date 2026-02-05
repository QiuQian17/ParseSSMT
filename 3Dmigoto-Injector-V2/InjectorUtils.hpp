#pragma once

#include <windows.h>
#include <stdio.h>
#include <tlhelp32.h>
#include <set>
#include <string>
#include "D3dxIniUtils.hpp"

class InjectorUtils {
public:
    static void WaitKeypress(const std::string& msg) {
        puts(msg.c_str());
        getchar();
    }

    static void WaitExit(int code = 0, const std::string& msg = "\nPress enter to close...\n") {
        WaitKeypress(msg);
        exit(code);
    }

    static wchar_t* DeduceWorkingDirectory(wchar_t* setting, wchar_t dir[MAX_PATH]) {
        DWORD ret;
        wchar_t* file_part = NULL;

        ret = GetFullPathName(setting, MAX_PATH, dir, &file_part);
        if (!ret || ret >= MAX_PATH)
            return NULL;

        ret = GetFileAttributes(dir);
        if (ret == INVALID_FILE_ATTRIBUTES)
            return NULL;

        if (!(ret & FILE_ATTRIBUTE_DIRECTORY) && file_part)
            *file_part = '\0';

        printf("Using working directory: \"%S\"\n", dir);

        return dir;
    }

    static void Run(D3dxIniUtils& ini) {
        wchar_t working_dir[MAX_PATH], * working_dir_p = NULL;
        wchar_t module_full_path[MAX_PATH];
        HHOOK hook;
        bool launch;

        HMODULE module = LoadLibraryA(ini.ToByteString(ini.module).c_str());
        if (!module) {
            printf("Unable to load 3DMigoto \"%s\"\n", ini.ToByteString(ini.module).c_str());
            WaitExit(EXIT_FAILURE);
        }

        GetModuleFileName(module, module_full_path, MAX_PATH);
        if (module)
            printf("Loaded %S\n\n", module_full_path);

        hook = InstallHook(module, "CBTProc");
        if (!hook) {
            WaitExit(EXIT_FAILURE, "Module does not support injection method or Error installing hook\nMake sure this is a recent 3DMigoto d3d11.dll\n");
        }

        if (ini.inject_dll != L"") {
            std::wstring extra_dll_path = ini.inject_dll;
            HMODULE extra_module = LoadLibraryW(extra_dll_path.c_str());
            if (extra_module) {
                wchar_t extra_module_full_path[MAX_PATH];
                GetModuleFileNameW(extra_module, extra_module_full_path, MAX_PATH);
                printf("Loaded Extra DLL for Hook Injection: %S\n", extra_module_full_path);
                InstallHook(extra_module, "CBTProc");
            }
        }

        launch = ini.launch != L"";

        if (launch) {
            wchar_t run_path_wchar_t[MAX_PATH];
            wcsncpy_s(run_path_wchar_t, MAX_PATH, ini.launch.c_str(), _TRUNCATE);
            wchar_t run_args_wchar_t[MAX_PATH];
            wcsncpy_s(run_args_wchar_t, MAX_PATH, ini.launch_args.c_str(), _TRUNCATE);
            CoInitializeEx(NULL, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
            working_dir_p = DeduceWorkingDirectory(run_path_wchar_t, working_dir);
            ShellExecute(NULL, NULL, run_path_wchar_t, run_args_wchar_t, working_dir_p, SW_SHOWNORMAL);
        }
        else {
            printf("3DMigoto ready - Now run the game.\n");
        }

        int delayint = 5;
        try { delayint = std::stoi(ini.delay); }
        catch (...) { delayint = 5; }

        if (delayint != -1) {
            if (delayint <= 0) delayint = 5;
            WaitForTarget(ini.ToByteString(ini.target).c_str(), module_full_path, true, delayint, launch, NULL);
        }
        UnhookWindowsHookEx(hook);
    }

    static HHOOK InstallHook(HMODULE module, const char* procName = "CBTProc") {
        FARPROC fn = GetProcAddress(module, procName);
        if (!fn) {
            return NULL;
        }
        return SetWindowsHookEx(WH_CBT, (HOOKPROC)fn, module, 0);
    }

    static void WaitForTarget(const char* target_a, const wchar_t* module_path, bool wait, int delay, bool launched, const wchar_t* inject_dll = NULL) {
        wchar_t target_w[MAX_PATH];

        if (!MultiByteToWideChar(CP_UTF8, 0, target_a, -1, target_w, MAX_PATH))
            return;

        for (int seconds = 0; wait || delay == -1; seconds++) {
            if (CheckForRunningTarget(target_w, module_path, inject_dll) && delay != -1)
                break;
            Sleep(1000);

            if (launched && seconds == 3) {
                printf("\nStill waiting for the game to start...\n"
                    "If the game does not launch automatically, leave this window open and run it manually.\n"
                    "You can also adjust/remove the [Loader] launch= option in the d3dx.ini as desired.\n\n");
            }
        }

        for (int i = delay; i > 0; i--) {
            printf("Shutting down loader in %i...\r", i);
            Sleep(1000);
            CheckForRunningTarget(target_w, module_path, inject_dll);
        }
        printf("\n");
    }

    static bool CheckForRunningTarget(wchar_t* target, const wchar_t* module, const wchar_t* inject_dll = NULL) {
        HANDLE snapshot;
        PROCESSENTRY32 pe;
        bool rc = false;
        wchar_t* basename = wcsrchr(target, '\\');
        static std::set<DWORD> pids;
        static std::set<DWORD> injected_pids; // Kept for inject_dll logic if needed

        if (basename)
            basename++;
        else
            basename = target;

        snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (snapshot == INVALID_HANDLE_VALUE) {
            printf("Unable to verify if 3DMigoto was successfully loaded: %d\n", GetLastError());
            return false;
        }

        pe.dwSize = sizeof(PROCESSENTRY32);
        if (!Process32First(snapshot, &pe)) {
            printf("Unable to verify if 3DMigoto was successfully loaded: %d\n", GetLastError());
            CloseHandle(snapshot);
            return false;
        }

        do {
            if (_wcsicmp(pe.szExeFile, basename))
                continue;

            rc = VerifyInjection(&pe, module, !pids.count(pe.th32ProcessID)) || rc;
            pids.insert(pe.th32ProcessID);

            // Keep the CreateRemoteThread logic here as fallback or utility, 
            // even if Main.cpp passes NULL to disable it.
            if (rc && inject_dll && inject_dll[0] != L'\0' && !injected_pids.count(pe.th32ProcessID)) {
                InjectDllToTarget(&pe, inject_dll);
                injected_pids.insert(pe.th32ProcessID);
            }

        } while (Process32Next(snapshot, &pe));

        CloseHandle(snapshot);
        return rc;
    }

private:
    static bool VerifyInjection(PROCESSENTRY32* pe, const wchar_t* module, bool log_name) {
        HANDLE snapshot;
        MODULEENTRY32 me;
        const wchar_t* basename = wcsrchr(module, '\\');
        bool rc = false;
        static std::set<DWORD> verified_pids;
        wchar_t exe_path[MAX_PATH], mod_path[MAX_PATH];

        if (basename)
            basename++;
        else
            basename = module;

        do {
            snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pe->th32ProcessID);
        } while (snapshot == INVALID_HANDLE_VALUE && GetLastError() == ERROR_BAD_LENGTH);

        if (snapshot == INVALID_HANDLE_VALUE) {
            DWORD lastError = GetLastError();
            if (lastError == ERROR_ACCESS_DENIED) {
                if (!verified_pids.count(pe->th32ProcessID)) {
                    printf("%d: target process found, but it don't want us to inject, whatever, we don't care. :)\n", pe->th32ProcessID);
                    verified_pids.insert(pe->th32ProcessID);
                }
                return true; 
            }
            printf("%S (%d): Unable to verify if 3DMigoto was successfully loaded: %d\n",
                pe->szExeFile, pe->th32ProcessID, lastError);
            return false;
        }

        me.dwSize = sizeof(MODULEENTRY32);
        if (!Module32First(snapshot, &me)) {
            DWORD lastError = GetLastError();
            if (lastError == ERROR_ACCESS_DENIED) {
                if (!verified_pids.count(pe->th32ProcessID)) {
                    printf("%d: Unable to verify 3DMigoto loading status due to access denied - assuming success :)\n", pe->th32ProcessID);
                    verified_pids.insert(pe->th32ProcessID);
                }
                CloseHandle(snapshot);
                return true;
            }
            printf("%S (%d): Unable to verify if 3DMigoto was successfully loaded: %d\n",
                pe->szExeFile, pe->th32ProcessID, lastError);
            CloseHandle(snapshot);
            return false;
        }

        if (log_name)
            printf("Target process found (%i): %S\n", pe->th32ProcessID, me.szExePath);
        wcscpy_s(exe_path, MAX_PATH, me.szExePath);

        rc = false;
        while (Module32Next(snapshot, &me)) {
            if (_wcsicmp(me.szModule, basename))
                continue;

            if (!_wcsicmp(me.szExePath, module)) {
                if (!verified_pids.count(pe->th32ProcessID)) {
                    printf("%d: 3DMigoto loaded :)\n", pe->th32ProcessID);
                    verified_pids.insert(pe->th32ProcessID);
                }
                rc = true;
            }
            else {
                wcscpy_s(mod_path, MAX_PATH, me.szExePath);
                wcsrchr(exe_path, L'\\')[1] = '\0';
                wcsrchr(mod_path, L'\\')[1] = '\0';
                if (!_wcsicmp(exe_path, mod_path)) {
                    printf("\n\n\n"
                        "WARNING: Found a second copy of 3DMigoto loaded from the game directory:\n"
                        "%S\n"
                        "This may crash - please remove the copy in the game directory and try again\n\n\n",
                        me.szExePath);
                    exit(EXIT_FAILURE);
                }
            }
        }

        CloseHandle(snapshot);
        return rc;
    }

    static bool InjectDllToTarget(PROCESSENTRY32* pe, const wchar_t* dll_path) {
        DWORD dwDesiredAccess = PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ;
        HANDLE hProcess = OpenProcess(dwDesiredAccess, FALSE, pe->th32ProcessID);

        if (!hProcess) {
            DWORD lastError = GetLastError();
            printf("Failed to open process %d for injection. Error Code: %d\n", pe->th32ProcessID, lastError);
            if (lastError == ERROR_ACCESS_DENIED) {
                printf(">> Access Denied! The target process is likely running as Administrator.\n");
                printf(">> Please try running this Injector as Administrator.\n");
            }
            return false;
        }

        LPVOID pDllPath = VirtualAllocEx(hProcess, 0, MAX_PATH * sizeof(wchar_t), MEM_COMMIT, PAGE_READWRITE);
        if (!pDllPath) {
            printf("Failed to allocate memory in target process %d\n", pe->th32ProcessID);
            CloseHandle(hProcess);
            return false;
        }

        if (!WriteProcessMemory(hProcess, pDllPath, (void*)dll_path, (wcslen(dll_path) + 1) * sizeof(wchar_t), 0)) {
            printf("Failed to write DLL path to target process %d\n", pe->th32ProcessID);
            VirtualFreeEx(hProcess, pDllPath, 0, MEM_RELEASE);
            CloseHandle(hProcess);
            return false;
        }

        HANDLE hThread = CreateRemoteThread(hProcess, 0, 0, (LPTHREAD_START_ROUTINE)GetProcAddress(GetModuleHandleA("Kernel32.dll"), "LoadLibraryW"), pDllPath, 0, 0);
        if (!hThread) {
            printf("Failed to create remote thread in target process %d\n", pe->th32ProcessID);
            VirtualFreeEx(hProcess, pDllPath, 0, MEM_RELEASE);
            CloseHandle(hProcess);
            return false;
        }

        WaitForSingleObject(hThread, INFINITE);

        DWORD exitCode = 0;
        GetExitCodeThread(hThread, &exitCode);

        VirtualFreeEx(hProcess, pDllPath, 0, MEM_RELEASE);
        CloseHandle(hThread);
        CloseHandle(hProcess);

        if (exitCode == 0) {
            printf("Remote LoadLibraryW failed in target process %d\n", pe->th32ProcessID);
            return false;
        }

        printf("Successfully injected %S into process %d\n", dll_path, pe->th32ProcessID);
        return true;
    }
};

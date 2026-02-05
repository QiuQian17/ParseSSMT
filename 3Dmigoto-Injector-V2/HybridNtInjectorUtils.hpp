#pragma once

#include <windows.h>
#include <stdio.h>
#include <string>
#include "D3dxIniUtils.hpp"
#include <tlhelp32.h>
#include "InjectorUtils.hpp"
#include <vector>
#include <shellapi.h>

// NT function typedefs
typedef NTSTATUS(NTAPI* pNtCreateThreadEx)(PHANDLE, ACCESS_MASK, PVOID, HANDLE, PVOID, PVOID, ULONG, ULONG_PTR, SIZE_T, SIZE_T, PVOID);
typedef NTSTATUS(NTAPI* pNtAllocateVirtualMemory)(HANDLE, PVOID*, ULONG_PTR, PSIZE_T, ULONG, ULONG);
typedef NTSTATUS(NTAPI* pNtWriteVirtualMemory)(HANDLE, PVOID, PVOID, SIZE_T, PSIZE_T);

auto ResolveNtCreateThreadEx() {
    return (pNtCreateThreadEx)GetProcAddress(GetModuleHandleW(L"ntdll.dll"), "NtCreateThreadEx");
}
auto ResolveNtAllocateVirtualMemory() {
    return (pNtAllocateVirtualMemory)GetProcAddress(GetModuleHandleW(L"ntdll.dll"), "NtAllocateVirtualMemory");
}
auto ResolveNtWriteVirtualMemory() {
    return (pNtWriteVirtualMemory)GetProcAddress(GetModuleHandleW(L"ntdll.dll"), "NtWriteVirtualMemory");
}

class HybridNtInjectorUtils {
public:
    static void Run(D3dxIniUtils& ini) {
        printf("[提示] 冒险开始~\n");
        if (ini.launch == L"") {
            printf("[提示] 还没设置 launch 哦，切换到守候模式~\n");
            FallbackWaitMode(ini);
            return;
        }
        LaunchAndInject(ini, ini.delay);
    }

private:
    static void WaitForExit(const char* msg = "\n按回车键结束冒险...\n") {
        printf("%s", msg);
        getchar();
    }

    static void AutoExitOrWait(const std::wstring& delayStr) {
        int delay = 5;
        try { delay = std::stoi(delayStr); } catch (...) { delay = 5; }
        if (delay == -1) {
            WaitForExit();
            return;
        }
        if (delay <= 0) delay = 5;
        for (int i = delay; i > 0; --i) {
            printf("倒计时 %i 秒后离开，先喝口奶茶...\r", i);
            Sleep(1000);
        }
        printf("\n");
    }

    static void LaunchAndInject(D3dxIniUtils& ini, const std::wstring& delayStr) {
        // 0) Preload d3d11.dll locally and install a GLOBAL CBT hook (threadId = 0), mimicking InjectorUtils order
        HHOOK d3d11_hook = InstallGlobalCbtHook(ini.module.c_str());
        if (!d3d11_hook) {
            printf("[提示] d3d11.dll 小伙伴挂钩失败，先撤退~\n");
            return;
        }

        STARTUPINFOW si = { sizeof(si) };
        PROCESS_INFORMATION pi = { 0 };

        wchar_t run_path[MAX_PATH];
        wchar_t run_args[MAX_PATH];
        wchar_t working_dir[MAX_PATH];
        wcsncpy_s(run_path, MAX_PATH, ini.launch.c_str(), _TRUNCATE);
        wcsncpy_s(run_args, MAX_PATH, ini.launch_args.c_str(), _TRUNCATE);

        //因为鸣潮只能通过ShellExecute启动才不会报错
        if (ini.inject_dll == L"") {
            printf("[提示] 即将召唤目标进程（ShellExecute）：%S\n", run_path);
            CoInitializeEx(NULL, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
            wchar_t* working_dir_p = InjectorUtils::DeduceWorkingDirectory(run_path, working_dir);
            HINSTANCE hInst = ShellExecuteW(NULL, NULL, run_path, run_args, working_dir_p, SW_SHOWNORMAL);
            if ((INT_PTR)hInst <= 32) {
                printf("[提示] 召唤失败，错误码: %Id\n", (INT_PTR)hInst);
                if (d3d11_hook) UnhookWindowsHookEx(d3d11_hook);
                return;
            }

            AutoExitOrWait(delayStr);
            if (d3d11_hook) UnhookWindowsHookEx(d3d11_hook);
            return;
        }

        wchar_t* file_part = NULL;
        GetFullPathNameW(run_path, MAX_PATH, working_dir, &file_part);
        if (file_part) *file_part = L'\0';

        // Build command line with executable name so first argument is not consumed
        std::wstring cmdLine = L"\""; cmdLine += run_path; cmdLine += L"\"";
        if (wcslen(run_args) > 0) { cmdLine += L" "; cmdLine += run_args; }
        std::vector<wchar_t> cmdBuf(cmdLine.begin(), cmdLine.end());
        cmdBuf.push_back(L'\0');

        printf("[提示] 即将召唤目标进程：%S\n", run_path);
        if (!CreateProcessW(run_path, cmdBuf.data(), NULL, NULL, FALSE, CREATE_SUSPENDED, NULL, working_dir, &si, &pi)) {
            printf("[提示] 召唤失败，错误码: %d\n", GetLastError());
            UnhookWindowsHookEx(d3d11_hook);
            return;
        }

        // Inject extra DLL first (NT path)
        bool extra_ok = true;
        if (ini.inject_dll != L"") {
            printf("[提示] 先把额外的魔法道具塞进去：%S\n", ini.inject_dll.c_str());
            extra_ok = NtInjectDll(pi.hProcess, ini.inject_dll.c_str());
        }

        printf("[提示] 主角苏醒，开始行动！\n");
        ResumeThread(pi.hThread);

        // Wait for d3d11.dll to appear (gives CBTProc time to run)
        if (WaitForModuleLoaded(pi.dwProcessId, ini.module.c_str(), 30000)) {
            printf("[提示] d3d11.dll 已就位，小精灵应该已经打招呼啦~\n");
        } else {
            printf("[提示] 等待 d3d11.dll 超时，看起来它去喝奶茶了...\n");
        }

        if (d3d11_hook) UnhookWindowsHookEx(d3d11_hook);

        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);

        if (d3d11_hook && extra_ok) printf("[提示] 冒险完成，全部正常~\n");
        else printf("[提示] 冒险结束，但有小状况，请检查~\n");

        AutoExitOrWait(delayStr);
    }

    static void FallbackWaitMode(D3dxIniUtils& ini) {
        HHOOK d3d11_hook = InstallGlobalCbtHook(ini.module.c_str());
        if (!d3d11_hook) {
            printf("[提示] d3d11.dll 小伙伴挂钩失败，先撤退~\n");
            WaitForExit();
            return;
        }
        wchar_t module_full_path[MAX_PATH];
        HMODULE localMod = LoadLibraryW(ini.module.c_str());
        if (localMod) {
            GetModuleFileNameW(localMod, module_full_path, MAX_PATH);
            printf("[提示] 已在本地装载：%S\n", module_full_path);
        }
        printf("[提示] 没有 launch，进入守候模式，等目标出现再说~\n");
        int delay = ParseDelay(ini.delay);
        InjectorUtils::WaitForTarget(ini.ToByteString(ini.target).c_str(), module_full_path, true,
            delay, false, NULL);
        if (d3d11_hook) UnhookWindowsHookEx(d3d11_hook);
        // 不再二次等待，直接结束
    }

    static int ParseDelay(const std::wstring& delayStr) {
        int delay = 5;
        try { delay = std::stoi(delayStr); } catch (...) { delay = 5; }
        if (delay <= 0 && delay != -1) delay = 5;
        return delay;
    }

    static bool WaitForModuleLoaded(DWORD pid, const wchar_t* modulePath, DWORD timeoutMs) {
        const wchar_t* base = wcsrchr(modulePath, L'\\');
        base = base ? base + 1 : modulePath;
        DWORD waited = 0;
        while (waited < timeoutMs) {
            HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, pid);
            if (snap != INVALID_HANDLE_VALUE) {
                MODULEENTRY32 me; me.dwSize = sizeof(me);
                if (Module32First(snap, &me)) {
                    do {
                        if (_wcsicmp(me.szModule, base) == 0) {
                            CloseHandle(snap);
                            return true;
                        }
                    } while (Module32Next(snap, &me));
                }
                CloseHandle(snap);
            }
            Sleep(500); waited += 500;
        }
        return false;
    }

    static HHOOK InstallGlobalCbtHook(const wchar_t* d3d11Path) {
        HMODULE localMod = LoadLibraryW(d3d11Path);
        if (!localMod) {
            printf("[提示] 装载 d3d11.dll 失败：%d\n", GetLastError());
            return NULL;
        }
        FARPROC cbt = GetProcAddress(localMod, "CBTProc");
        if (!cbt) {
            printf("[提示] d3d11.dll 没有魔法入口，无法打招呼。\n");
            return NULL;
        }
        HHOOK hook = SetWindowsHookExW(WH_CBT, (HOOKPROC)cbt, localMod, 0);
        if (!hook) {
            printf("[提示] 设置魔法挂钩失败：%d\n", GetLastError());
            return NULL;
        }
        printf("[提示] 魔法挂钩已布置，等待触发~\n");
        return hook;
    }

    // Keep InstallCbtHookForD3D11 for reference (unused in new flow)
    static HHOOK InstallCbtHookForD3D11(const PROCESS_INFORMATION& pi, const wchar_t* d3d11Path) {
        HMODULE localMod = LoadLibraryW(d3d11Path);
        if (!localMod) {
            printf("[提示] 装载 d3d11.dll 失败：%d\n", GetLastError());
            return NULL;
        }
        FARPROC cbt = GetProcAddress(localMod, "CBTProc");
        if (!cbt) {
            printf("[提示] d3d11.dll 没有魔法入口，无法打招呼。\n");
            return NULL;
        }
        HHOOK hook = SetWindowsHookExW(WH_CBT, (HOOKPROC)cbt, localMod, pi.dwThreadId);
        if (!hook) {
            printf("[提示] 设置魔法挂钩失败：%d\n", GetLastError());
            return NULL;
        }
        printf("[提示] 魔法挂钩挂载在线程 %d 上。\n", pi.dwThreadId);
        return hook;
    }

    static bool NtInjectDll(HANDLE hProcess, const wchar_t* dllPath) {
        auto NtAlloc = ResolveNtAllocateVirtualMemory();
        auto NtWrite = ResolveNtWriteVirtualMemory();
        auto NtThread = ResolveNtCreateThreadEx();
        if (!NtAlloc || !NtWrite || !NtThread) {
            printf("[提示] 关键魔法函数解析失败，无法注入。\n");
            return false;
        }

        SIZE_T len = (wcslen(dllPath) + 1) * sizeof(wchar_t);
        PVOID remote = nullptr;
        NTSTATUS st = NtAlloc(hProcess, &remote, 0, &len, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (st != 0) { printf("[提示] NtAllocateVirtualMemory fail: 0x%X\n", st); return false; }

        SIZE_T written = 0;
        st = NtWrite(hProcess, remote, (PVOID)dllPath, len, &written);
        if (st != 0) { printf("[提示] NtWriteVirtualMemory fail: 0x%X\n", st); return false; }

        PVOID loadLib = (PVOID)GetProcAddress(GetModuleHandleW(L"kernel32.dll"), "LoadLibraryW");
        HANDLE hThread = NULL;
        st = NtThread(&hThread, PROCESS_ALL_ACCESS, NULL, hProcess, loadLib, remote, 0, 0, 0, 0, NULL);
        if (st != 0 || !hThread) { printf("[提示] 开线程失败，状态: 0x%X\n", st); return false; }

        WaitForSingleObject(hThread, INFINITE);
        CloseHandle(hThread);
        printf("[提示] 已把道具塞进目标：%S\n", dllPath);
        return true;
    }
};

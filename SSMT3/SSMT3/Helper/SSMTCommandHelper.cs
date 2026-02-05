using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpCompress.Common;
using SSMT3;
using SSMT3.InfoClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;

namespace SSMT3
{
    public class SSMTCommandHelper
    {

        public static void InitializeRunInputJson(string arguments)
        {
            //把当前运行的命令保存到RunInput.json
            JObject runInputJson = new JObject();
            if (File.Exists(PathManager.Path_RunInputJson))
            {
                string json = File.ReadAllText(PathManager.Path_RunInputJson); // 读取文件内容
                runInputJson = JObject.Parse(json);

            }
            runInputJson["RunCommand"] = arguments;
            string runInputJsonStr = runInputJson.ToString(Formatting.Indented);
            File.WriteAllText(PathManager.Path_RunInputJson, runInputJsonStr);
        }

        public static void InitializeRunResultJson()
        {
            JObject jsonObject = new JObject();
            jsonObject["result"] = "Unknown Error!";
            File.WriteAllText(PathManager.Path_RunResultJson, jsonObject.ToString());
        }

        public static bool RunPluginExeCommand(string arguments, string targetExe = "", bool ShowRunResultWindow = true, bool ShellExecute = false)
        {

            InitializeRunInputJson(arguments);
            InitializeRunResultJson();
            Process process = new Process();
            if (targetExe == "")
            {

            }
            else
            {
                process.StartInfo.FileName = Path.Combine(PathManager.Path_PluginsFolder, targetExe);
            }
            //运行前必须检查路径
            if (!File.Exists(process.StartInfo.FileName))
            {
                _ = SSMTMessageHelper.Show("Current run path didn't exsits: " + process.StartInfo.FileName, "当前要执行的插件不存在: " + process.StartInfo.FileName + "\n请联系NicoMico赞助获取此插件。");
                return false;
            }

            process.StartInfo.Arguments = arguments;  // 可选，如果该程序接受命令行参数
            //MessageBox.Show("当前运行参数： " + arguments);

            //运行目录必须是调用的文件所在的目录，不然的话就会在当前SSMT.exe下面运行，就会导致很多东西错误，比如逆向的日志无法显示。
            process.StartInfo.WorkingDirectory = PathManager.Path_ResourcesFolder; // <-- 新增

            // 配置进程启动信息
            process.StartInfo.UseShellExecute = ShellExecute;  // 不使用操作系统的shell启动程序
            process.StartInfo.RedirectStandardOutput = false;  // 重定向标准输出
            process.StartInfo.RedirectStandardError = false;   // 重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;  // 不创建新窗口
            // 启动程序
            process.Start();
            process.WaitForExit();

            try
            {
                string runResultJson = File.ReadAllText(PathManager.Path_RunResultJson);
                JObject resultJsonObject = JObject.Parse(runResultJson);
                string runResult = (string)resultJsonObject["result"];
                if (runResult != "success")
                {
                    if (ShowRunResultWindow)
                    {
                        _ = SSMTMessageHelper.Show(
                        "运行结果: " + runResult + ". \n\n很遗憾运行失败了，参考运行结果和运行日志改变策略再试一次吧。\n\n1.请检查您的配置是否正确.\n2.请查看日志获取更多细节信息.\n3.请检查您是否使用的是最新版本，新版本可能已修复此问题\n4.请联系NicoMico寻求帮助或在Github上提交issue: https://github.com/StarBobis/DirectX-BufferModTool.\n\n点击确认为后您打开本次运行日志。",
                        "Run result: " + runResult + ". \n1.Please check your config.\n2.Please check log for more information.\n3.Please ask NicoMico for help, remember to send him the latest log file.\n4.Ask @Developer in ShaderFreedom for help.\n5.Read the source code of DBMT and try analyse the reason for Error with latest log file.");
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show("执行DBMT核心时发生中断，请查看Log日志获取具体内容", "Error when execute DBMT.exe, please see log file for details." + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// V2版本使用RunInfo包裹运行参数
        /// </summary>
        /// <param name="programPaths"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task LaunchSequentiallyAsyncV2(List<RunInfo> RunInfoList)
        {
            if (RunInfoList.Count == 0)
            {
                //因为必定会启动3Dmigoto，所以这里几乎不会被触发，但是嘛万一以后用来干别的呢，加个保险
                return;
            }

                

            foreach (RunInfo runInfo in RunInfoList)
            {
                bool started = false;
                string processName = Path.GetFileNameWithoutExtension(runInfo.RunPath);
                LOG.Info("Try Start: " + processName);

                string StartDirectory = runInfo.RunLocation;
                if (StartDirectory == "")
                {
                    StartDirectory = Path.GetDirectoryName(runInfo.RunPath);
                }
                

                while (!started)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = runInfo.RunPath,
                            WorkingDirectory = StartDirectory,
                            UseShellExecute = runInfo.UseShell,
                            Verb = runInfo.Verb // 触发 UAC 提权
                        };

                        // 尝试启动（会触发 UAC）
                        Process.Start(psi);

                        // 轮询检测程序是否真的启动
                        for (int i = 0; i < 60; i++) // 最长等待约30秒
                        {
                            var running = Process.GetProcessesByName(processName);
                            if (running.Any())
                            {
                                started = true;
                                Debug.WriteLine($"✅ 检测到程序已启动: {runInfo.RunPath}");
                                break;
                            }

                            await Task.Delay(500);
                        }

                        if (!started)
                        {
                            Debug.WriteLine($"⚠️ 启动超时或被取消: {runInfo.RunPath}");
                            started = true; // 跳过继续下一个
                        }

                        await Task.Delay(1000); // 小延迟，避免并发启动
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            // 用户拒绝 UAC
                            Debug.WriteLine($"❌ 用户拒绝启动: {runInfo.RunPath}");
                            started = true; // 跳过
                        }
                        else
                        {
                            Debug.WriteLine($"启动失败: {ex.Message}");
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"启动 {runInfo.RunPath} 失败: {ex}");
                        await Task.Delay(1000);
                    }
                }
            }
        }

        public static async Task LaunchSequentiallyAsync(List<string> programPaths)
        {
            if (programPaths == null || programPaths.Count == 0)
                throw new ArgumentException("程序路径列表不能为空。", nameof(programPaths));

            foreach (var path in programPaths)
            {
                bool started = false;
                string processName = Path.GetFileNameWithoutExtension(path);
                LOG.Info("Try Start: " + processName);
                while (!started)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = path,
                            WorkingDirectory = Path.GetDirectoryName(path),
                            UseShellExecute = true,
                            Verb = "runas" // 触发 UAC 提权
                        };

                        // 尝试启动（会触发 UAC）
                        Process.Start(psi);

                        // 轮询检测程序是否真的启动
                        for (int i = 0; i < 60; i++) // 最长等待约30秒
                        {
                            var running = Process.GetProcessesByName(processName);
                            if (running.Any())
                            {
                                started = true;
                                Debug.WriteLine($"✅ 检测到程序已启动: {path}");
                                break;
                            }

                            await Task.Delay(500);
                        }

                        if (!started)
                        {
                            Debug.WriteLine($"⚠️ 启动超时或被取消: {path}");
                            started = true; // 跳过继续下一个
                        }

                        await Task.Delay(1000); // 小延迟，避免并发启动
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 1223)
                        {
                            // 用户拒绝 UAC
                            Debug.WriteLine($"❌ 用户拒绝启动: {path}");
                            started = true; // 跳过
                        }
                        else
                        {
                            Debug.WriteLine($"启动失败: {ex.Message}");
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"启动 {path} 失败: {ex}");
                        await Task.Delay(1000);
                    }
                }
            }
        }

        public static void RunUPX(string arguments, bool ShowRunResultWindow = true, bool ShellExecute = false)
        {

            Process process = new Process();

            process.StartInfo.FileName = PathManager.Path_UpxExe;
            process.StartInfo.Arguments = arguments;  // 可选，如果该程序接受命令行参数
            //运行目录必须是调用的文件所在的目录，不然的话就会在当前SSMT.exe下面运行，就会导致很多东西错误，比如逆向的日志无法显示。
            process.StartInfo.WorkingDirectory = PathManager.Path_PluginsFolder; // <-- 新增

            // 配置进程启动信息
            process.StartInfo.UseShellExecute = ShellExecute;  // 不使用操作系统的shell启动程序
            process.StartInfo.RedirectStandardOutput = false;  // 重定向标准输出
            process.StartInfo.RedirectStandardError = false;   // 重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;  // 不创建新窗口
            // 启动程序
            process.Start();
            process.WaitForExit();
        }

        


        public static async Task<bool> ProcessRunFile(string FilePath, string WorkingDirectory = "",string arguments = "")
        {
            if (WorkingDirectory == "")
            {
                WorkingDirectory = System.IO.Path.GetDirectoryName(FilePath); // 获取程序所在目录
            }

            try
            {

                if (File.Exists(FilePath))
                {
                    try
                    {

                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            Arguments = arguments,
                            FileName = FilePath,
                            UseShellExecute = true,
                            WorkingDirectory = WorkingDirectory // 设置工作路径为程序所在路径
                        };

                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        await SSMTMessageHelper.Show("打开文件出错: \n" + FilePath + "\n" + ex.Message);
                        return false;
                    }
                }
                else
                {
                    await SSMTMessageHelper.Show("要打开的文件路径不存在: \n" + FilePath);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                await SSMTMessageHelper.Show("Error: " + ex.ToString());
                return false;
            }

        }

        public static async Task<bool> ShellOpenFile(string FilePath,string WorkingDirectory = "",string arguments = "")
        {
            if (WorkingDirectory == "")
            {
                WorkingDirectory = System.IO.Path.GetDirectoryName(FilePath); // 获取程序所在目录
            }

            try
            {

                if (File.Exists(FilePath))
                {
                    try
                    {

                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            Arguments = arguments,
                            FileName = FilePath,
                            UseShellExecute = true, // 允许操作系统决定如何打开文件
                            WorkingDirectory = WorkingDirectory // 设置工作路径为程序所在路径
                        };

                        Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        await SSMTMessageHelper.Show("打开文件出错: \n" + FilePath + "\n" + ex.Message);
                        return false;
                    }
                }
                else
                {
                    await SSMTMessageHelper.Show("要打开的文件路径不存在: \n" + FilePath);
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                await SSMTMessageHelper.Show("Error: " + ex.ToString());
                return false;
            }

        }


        public static void ShellOpenFolder(string FolderPath)
        {
           
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = FolderPath,
                UseShellExecute = true, // 允许操作系统决定如何打开文件夹
                WorkingDirectory = FolderPath // 设置工作路径为要打开的文件夹路径
            };

            Process.Start(startInfo);
           
        }



        public static void OpenWebLink(string Url)
        {
            if (Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                IAsyncOperation<bool> asyncOperation = Launcher.LaunchUriAsync(new Uri(Url));
            }
        }

        public static FileOpenPicker Get_FileOpenPicker(string Suffix)
        {
            FileOpenPicker picker = new FileOpenPicker();

            // 获取当前窗口的 HWND
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, windowHandle);

            picker.ViewMode = PickerViewMode.Thumbnail;

            // 💡 支持多个扩展名，例如 ".png;.mp4;.jpg"
            foreach (var ext in Suffix.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                string cleanExt = ext.Trim();

                // 确保以 "." 开头并去除通配符
                if (!cleanExt.StartsWith("."))
                    cleanExt = "." + cleanExt.TrimStart('*');

                picker.FileTypeFilter.Add(cleanExt);
            }

            return picker;
        }




        public static FileOpenPicker Get_FileOpenPicker(List<string> SuffixList)
        {
            FileOpenPicker picker = new FileOpenPicker();
            // 获取当前窗口的HWND
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, windowHandle);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            foreach (string Suffix in SuffixList)
            {
                picker.FileTypeFilter.Add(Suffix);
            }
            return picker;
        }


        public static FolderPicker Get_FolderPicker()
        {
            FolderPicker picker = new FolderPicker();
            // 获取当前窗口的HWND
            nint windowHandle = WindowNative.GetWindowHandle(App.m_window);
            InitializeWithWindow.Initialize(picker, windowHandle);

            picker.ViewMode = PickerViewMode.Thumbnail;
            //picker.SuggestedStartLocation = PickerLocationId.Desktop;
            return picker;
        }

        public static async Task<string> ChooseFileAndGetPath(string Suffix)
        {
            try
            {
                FileOpenPicker picker = SSMTCommandHelper.Get_FileOpenPicker(Suffix);
                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    return file.Path;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception exception)
            {
                await SSMTMessageHelper.Show("此功能不支持管理员权限运行，请切换到普通用户打开SSMT。\n" + exception.ToString(), "This functio can't run on admin user please use normal user to open SSMT. \n" + exception.ToString());
            }
            return "";
        }

        public static async Task<string> ChooseFileAndGetPath(List<string> SuffixList)
        {
            try
            {
                FileOpenPicker picker = SSMTCommandHelper.Get_FileOpenPicker(SuffixList);
                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    return file.Path;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception exception)
            {
                await SSMTMessageHelper.Show("此功能不支持管理员权限运行，请切换到普通用户打开SSMT。\n" + exception.ToString(), "This functio can't run on admin user please use normal user to open SSMT. \n" + exception.ToString());
            }
            return "";
        }

        public static async Task<string> ChooseFolderAndGetPath()
        {
            try
            {
                FolderPicker folderPicker = SSMTCommandHelper.Get_FolderPicker();
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    return folder.Path;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception exception)
            {
                await SSMTMessageHelper.Show("此功能不支持管理员权限运行，请切换到普通用户打开SSMT。\n" + exception.ToString());
            }
            return "";
            
        }


    }
}

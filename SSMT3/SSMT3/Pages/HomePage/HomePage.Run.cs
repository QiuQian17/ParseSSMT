using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SSMT3;
using SSMT3.InfoClass;
using SSMT3.InfoItemClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class HomePage
    {

        /// <summary>
        /// Nico: 这里涉及到数据库文件读写，我是懒得添加了，后面开源之后看看谁乐意添加谁添加吧。
        /// 因为这里可以设置一些LOD加载距离，防止出现特定视角下Mod和本体来回切换的问题
        /// 但是这个问题其实应该是Mod作者提供不同LOD下的Mod来解决，而不是修改引擎
        /// XXMI Launcher里就有这些东西的设置，咱们也可以加上，但是SSMT不是干这个的，所以懒得加了
        /// </summary>
        private void OverwriteWWMIEngineSettings()
        {
            //
            



        }


        private async void Button_RunLaunchPath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                try
                {
                    // 禁用按钮，防止多次点击误触导致启动多次
                    button.IsEnabled = false;

                    GameConfig gameConfig = new GameConfig();

                    string CurrentGameMigotoLoaderExePath = Path.Combine(gameConfig.MigotoPath, PathManager.Name_3DmigotoLoaderExe);

                    //如果存在旧的，就需要强制删除防止d3dxSkinManager以及XXMI带来的污染问题，光替换是没用的
                    if (File.Exists(CurrentGameMigotoLoaderExePath))
                    {
                        File.Delete(CurrentGameMigotoLoaderExePath);
                    }

                    //每次启动前强制替换LOD.exe 防止被其它工具污染
                    File.Copy(PathManager.Path_Default3DmigotoLoaderExe, CurrentGameMigotoLoaderExePath, true);

                    //当前3Dmigoto d3d11.dll目标路径
                    string MigotoTargetDll = Path.Combine(gameConfig.MigotoPath, "d3d11.dll");

                    //确保d3d11.dll是最新的
                    try
                    {
                        //这个函数只会在初始化的时候调用，所以默认复制Dev版本的d3d11.dll
                        string DllModeFolderName = "ReleaseX64Dev";
                        string MigotoSourceDll = Path.Combine(PathManager.Path_ResourcesFolder, DllModeFolderName + "\\d3d11.dll");
                        File.Copy(MigotoSourceDll, MigotoTargetDll, true);

                    }
                    catch (Exception ex)
                    {

                        _ = SSMTMessageHelper.Show(ex.ToString());
                        //这里不return
                        //因为经常会被占用，此时弹框提醒就行了。
                    }


                    //使用UPX压缩DLL，避开最基础的md5识别，当然目前原神热更新已经修复了这个，所以大部分情况下不管用了
                    //但是不是每个游戏都有反作弊，都能意识到这一点，所以保留此功能，万一有用，呵呵
                    bool useUpx = gameConfig.DllPreProcessSelectedIndex == 1;
                    if (useUpx)
                    {
                        SSMTCommandHelper.RunUPX(MigotoTargetDll, false);
                    }


                    //强制设置analyse_options 使用deferred_ctx_immediate确保IdentityV和YYSLS都能正确Dump出东西
                    string analyse_options = PathManager.analyse_options;

                    if (GlobalConfig.UseSymlinkFeature)
                    {
                        analyse_options = analyse_options + " symlink";
                    }

                    if (CheckBox_AutoSetAnalyseOptions.IsChecked == true)
                    {
                        D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "analyse_options", analyse_options);
                    }

                    string target_path = gameConfig.TargetPath;
                    if (target_path.Trim() == "")
                    {
                        target_path = TextBox_TargetPath.Text;
                        if (target_path.Trim() == "")
                        {
                            _ = SSMTMessageHelper.Show("启动前请先填写进程路径", "Please set your target path before start");
                            return;
                        }
                    }



                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "target", target_path);

                    

                    int dllInitializationDelay = (int)NumberBox_DllInitializationDelay.Value;

                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[system]", "dll_initialization_delay", dllInitializationDelay.ToString());

                    //强制设置hunting为2
                    //老外那边是默认关闭的，但是咱们首先是Mod制作工具，其次才是玩Mod的工具，为了兼顾还是设为2比较好
                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "hunting", "2");

                    //XXMI Launcher不让dump shader，何意味？咱们得强制开启
                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "marking_actions", "clipboard asm hlsl");

                    //设置delay
                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "delay", gameConfig.Delay.ToString());

                    //设置额外注入的dll
                    if (gameConfig.ExtraInjectDllPath.Trim() != "")
                    {
                        gameConfig.RunWithShell = false;
                        D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "inject_dll", gameConfig.ExtraInjectDllPath);
                    }
                    else
                    {
                        
                        D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "inject_dll", "");
                    }



                    //准备运行

                    List<RunInfo> RunFilePathList = new List<RunInfo>();

                    if (gameConfig.AutoRunIgnoreErrorGIPlugin)
                    {
                        string IgnoreGIErrorExePath = Path.Combine(PathManager.Path_PluginsFolder, PathManager.Name_Plugin_GoodWorkGI);
                        if (File.Exists(IgnoreGIErrorExePath))
                        {
                            RunFilePathList.Add(new RunInfo { RunPath = IgnoreGIErrorExePath });
                        }

                    }

                    //只有不勾选纯净游戏模式时，才启用3Dmigoto
                    if (!gameConfig.PureGameMode)
                    {
                        RunFilePathList.Add(new RunInfo { RunPath = CurrentGameMigotoLoaderExePath });
                    }
                    else
                    {
                        //如果是纯净模式，直接启动目标程序即可。
                        RunFilePathList.Add(new RunInfo
                        {
                            RunPath = gameConfig.LaunchPath,
                            RunWithArguments = gameConfig.LaunchArgs,
                            UseShell = true
                        });
                    }

                    if (gameConfig.RunWithShell)
                    {
                        //这里咱们根本不填写launch和launch_args，因为咱们用的是shell启动，所以这里直接就是强制设为空，防止预料之外的行为干扰。
                        D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "launch", "");
                        D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "launch_args", "");

                        if (File.Exists(gameConfig.LaunchPath.Trim()))
                        {
                            LOG.Info(gameConfig.LaunchPath + " 添加到启动列表");
                            RunFilePathList.Add(new RunInfo
                            {
                                RunPath = gameConfig.LaunchPath,
                                RunWithArguments = gameConfig.LaunchArgs,
                                UseShell = true
                            });
                        }

                        if (RunFilePathList.Count == 0)
                        {
                            _ = SSMTMessageHelper.Show("您当前的配置没什么可以启动的，请仔细检查您的配置", "your config can't start anything, please check your config again");
                            return;
                        }

                    }
                    else
                    {
                        //Nico:不用Shell的方式启动的话，就需要把launch和launch_args写进去
                        //这里需要注意，如果鸣潮使用Shell方式启动就会出现透明和白屏问题，所以这里不用Shell的方式启动必须存在

                        D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "launch", gameConfig.LaunchPath);
                        D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "launch_args", gameConfig.LaunchArgs);
                    }

                    //鸣潮需要执行一些预设配置
                    if (gameConfig.GamePreset == GamePreset.WWMI && gameConfig.OverwriteWWMIEngineSetting)
                    {
                        OverwriteWWMIEngineSettings();
                    }


                    await SSMTCommandHelper.LaunchSequentiallyAsyncV2(RunFilePathList);


                    // 点击开始游戏按钮后，务必等待1秒后重新启用，防止误触多次点击
                    await Task.Delay(3000);

                    if (GlobalConfig.PostRunGameAction == 1)
                    {
                        MainWindow.CurrentWindow.MinimizeToTrayAndNotify();
                    }
                    else if (GlobalConfig.PostRunGameAction == 2)
                    {
                        MainWindow.CurrentWindow.Close();
                    }

                }
                catch (Exception ex)
                {
                    // 确保按钮最终被重新启用
                    button.IsEnabled = true;
                    _ = SSMTMessageHelper.Show(ex.ToString());
                }
                finally
                {
                    // 确保按钮最终被重新启用
                    button.IsEnabled = true;
                }
            }



        }



    }
}

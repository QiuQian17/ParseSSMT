using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class HomePage
    {

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ✅ 每次进入页面都会执行，适合刷新 UI
            // 因为开启了缓存模式之后，是无法刷新页面语言的，只能在这里执行来刷新
            TranslatePage();
        }


        public void TranslatePage()
        {
            if (GlobalConfig.Chinese)
            {
                ComboBox_GameName.Header = "配置名称";
                ToolTipService.SetToolTip(ComboBox_GameName, "当前配置的名称，一般以简短的字母排列来表示，例如原神是GIMI，如果你同时玩一个游戏的国际服和国服，就可以通过创建游戏名称然后进行配置的方式在SSMT中实现快速切换");
                Button_CreateNewGame.Content = "以此名称创建新配置";
                Button_DeleteSelectedGame.Content = "删除当前选中配置";

                ComboBox_GamePreset.Header = "游戏预设";
                ToolTipService.SetToolTip(ComboBox_GamePreset, "选择当前游戏名称所属的具体游戏，决定了当前自定义游戏名称所属游戏的模型提取逻辑，Mod生成逻辑，提取使用的数据类型文件夹，3Dmigoto包来源，背景图更新逻辑等等");


                TextBlock_DIYSettings.Text = "自定义游戏设置";

                SettingsCard_MigotoPackage.Header = "3Dmigoto包来源";
                SettingsCard_MigotoPackage.Description = "此选项决定了将从哪个Github地址来下载更新对应的3Dmigoto包";

                SettingsCard_LogicName.Header = "执行逻辑";
                SettingsCard_LogicName.Description = "执行逻辑会影响到当前游戏下载的3Dmigoto包类型和来源，以及会影响到背景图的自动更新，以及提取模型时执行的逻辑，架构设计上来说每个游戏都有专属于它的执行逻辑";

                SettingsCard_GameTypeFolder.Header = "数据类型文件夹";
                SettingsCard_GameTypeFolder.Description = "数据类型文件夹决定了你在提取模型的时候，使用的是哪个数据类型";



                ComboBox_LogicName.Header = "执行逻辑";
                ToolTipService.SetToolTip(ComboBox_LogicName, "执行逻辑会影响到下载的3Dmigoto包来源，自动更新的背景图来源，提取模型逻辑，以及生成Mod逻辑等等，每个游戏都有对应唯一的执行逻辑");
                ComboBox_GameTypeFolder.Header = "数据类型文件夹";
                ToolTipService.SetToolTip(ComboBox_GameTypeFolder, "数据类型影响到提取模型时使用的数据类型，每个游戏都有对应唯一的数据类型文件夹");


                CheckBox_ShowIcon.Content = "显示图标";
                ToolTipService.SetToolTip(CheckBox_ShowIcon, "开启后将在主页的图标列表中显示当前游戏名称对应的图标，关闭后则隐藏图标，显示图标有助于在常用游戏间进行快速切换，不常用的游戏则隐藏避免视觉干扰");

                Button_ChooseGameIcon.Content = "选择图标文件";
                ToolTipService.SetToolTip(Button_ChooseGameIcon, "选择一个图片文件来作为当前游戏名称对应的图标，推荐使用正方形的PNG图片以获得最佳显示效果");

                Button_AutoUpdateBackground.Content = "检查背景图更新";
                ToolTipService.SetToolTip(Button_AutoUpdateBackground, "点击即可自动检查最新背景图并进行更新，仅支持部分游戏的背景图自动更新");

                Button_SelectBackgroundFile.Content = "选择背景文件";
                ToolTipService.SetToolTip(Button_SelectBackgroundFile, "选择一个文件来作为当前游戏名称指定的背景");

                TextBlock_3DmigotoFolderPath.Text = "3Dmigoto设置";

                Button_Choose3DmigotoPath.Content = "选择3Dmigoto文件夹路径";
                Button_Open3DmigotoFolder.Content = "打开当前3Dmigoto文件夹";

                Button_CheckMigotoPackageUpdate.Content = "从Github检查更新并自动下载最新3Dmigoto加载器包";


                TextBox_TargetPath.Header = "进程路径";
                Button_ChooseProcessFile.Content = "选择进程文件";
                Button_OpenProcessFileFolder.Content = "打开进程文件所在文件夹";

                TextBox_LaunchPath.Header = "启动路径";
                Button_ChooseLaunchFile.Content = "选择启动文件";
                Button_OpenLaunchFileFolder.Content = "打开启动文件所在文件夹";

                TextBox_LaunchArgsPath.Header = "启动参数";



                
                CheckBox_AutoSetAnalyseOptions.Content = "启动前自动设置 analyse_options";
                ToolTipService.SetToolTip(CheckBox_AutoSetAnalyseOptions, "勾选后，在启动3Dmigoto之前自动将d3dx.ini的analyse_options设置为: deferred_ctx_immediate dump_rt dump_cb dump_vb dump_ib buf txt dds dump_tex dds");


                CheckBox_ShowWarning.Content = "显示左上角报错";
                ToolTipService.SetToolTip(CheckBox_ShowWarning, "勾选后将在游戏左上角显示3Dmigoto报错，F10刷新生效");


                Button_RunLaunchPath.Content = " 开始游戏";
                ToolTipService.SetToolTip(Button_RunLaunchPath, "先进行一系列配置工作，然后运行3Dmigoto Loader，然后运行启动路径中填写的程序\n注入失败？请检查：设置→隐私和安全性→Windows 安全中心→应用和浏览器控制→基于声誉的保护设置");


                NumberBox_DllInitializationDelay.Header = "d3d11.dll初始化延迟(毫秒)";
                ToolTipService.SetToolTip(NumberBox_DllInitializationDelay, "d3d11.dll初始化时的延迟，单位为毫秒，一般WWMI填200，若仍然闪退则以每次100为单位增加此值直到不闪退，鸣潮在2.4版本更新后至少需要50ms的延迟以确保启动时不会闪退。此外，如果要让Reshade和3Dmigoto一起使用，至少需要150ms的延迟");


                CheckBox_DllPreProcess.Content = "使用UPX默认选项加壳";
                ToolTipService.SetToolTip(CheckBox_DllPreProcess, "勾选后运行前对d3d11.dll执行UPX压缩");

                Button_CleanGICache.Content = "清理原神缓存日志";
                ToolTipService.SetToolTip(Button_CleanGICache, "清理原神反作弊的运行报错日志，以避免由于缓存日志导致标记重点扫描从而更容易出现【与服务器的连接已断开】【15-4001】【10612-4001】的问题");

                Button_RunIgnoreGIError40.Content = "启动网络加固插件";
                ToolTipService.SetToolTip(Button_RunIgnoreGIError40, "开启后，使用SSMT启动游戏使用Mod将可以避免由于Mod导致的网络卡顿而出现的【与服务器的连接已断开】【15-4001】【10612-4001】错误弹窗");

                CheckBox_PureGameMode.Content = "纯净游戏模式";
                ToolTipService.SetToolTip(CheckBox_PureGameMode, "开启后点击【开始游戏】按钮将不再启动3Dmigoto，适合只想纯享游戏的用户");

                TextBlock_BasicSettings.Text = "基础设置";


                CheckBox_RunWithShell.Content = "使用Shell方式运行";
                ToolTipService.SetToolTip(CheckBox_RunWithShell, "勾选后以Shell方式运行启动路径，适度规避父进程检测，仅推荐GIMI开启");


                CheckBox_AutoRunIgnoreErrorGIPlugin.Content = "自动运行网络加固插件";
                ToolTipService.SetToolTip(CheckBox_AutoRunIgnoreErrorGIPlugin, "开启后点击【开始游戏】时会自动运行网络加固插件，免去手动操作");


                NumberBox_Delay.Header = "注入程序自动退出秒数";
                ToolTipService.SetToolTip(NumberBox_Delay, "单位为秒，如果不想让LOD.exe自动退出可以填-1，否则请填写1或以上的值，默认推荐为5，有些特殊需求会需要LOD.exe延迟自动关闭的时间，就需要设置这里的值");

                TextBox_ExtraInjectDll.Header = "额外注入的dll文件路径";
                Button_ChooseExtraInjectDll.Content = "选择额外注入的dll文件";

                CheckBox_OverwriteWWMIEngineSetting.Content = "覆盖鸣潮引擎设置";
                ToolTipService.SetToolTip(CheckBox_OverwriteWWMIEngineSetting, "勾选后，启动游戏时会覆盖鸣潮引擎设置以提高Mod兼容性");

            }
            else
            {
                ToolTipService.SetToolTip(ComboBox_GameName, "Current config name, usually we use simplified name,like GI or GIMI for GIMI");
                ComboBox_GameName.Header = "Config Name";
                Button_CreateNewGame.Content = "Create New Config";
                Button_DeleteSelectedGame.Content = "Delete Selected Config";

                ComboBox_GamePreset.Header = "Game Preset";
                ToolTipService.SetToolTip(ComboBox_GamePreset, "Select which game this GameName belongs to, it decides the logic of 3Dmigoto package source, data type folder, background auto update logic, etc.");

                TextBlock_DIYSettings.Text = "Custom Game Settings";

                SettingsCard_MigotoPackage.Header = "3Dmigoto Package Source";
                SettingsCard_MigotoPackage.Description = "This option decides from which Github address to download and update the corresponding 3Dmigoto package";

                SettingsCard_LogicName.Header = "LogicName";
                SettingsCard_LogicName.Description = "LogicName will affect the 3Dmigoto package type and source for current game, also will affect the background auto update, and model extraction logic, in architecture design, each game has its unique LogicName";

                SettingsCard_GameTypeFolder.Header = "GameType Folder";
                SettingsCard_GameTypeFolder.Description = "GameType Folder decides which data type you are using when you extract model";



                ComboBox_LogicName.Header = "LogicName";
                ComboBox_GameTypeFolder.Header = "GameType Folder";

                CheckBox_ShowIcon.Content = "Show Icon";

                Button_ChooseGameIcon.Content = "Choose Game Icon File";
                ToolTipService.SetToolTip(Button_ChooseGameIcon, "Select an image file to be the icon for current GameName, it's recommended to use square PNG image for best display effect");

                Button_AutoUpdateBackground.Content = "Auto Check And Update Background";
                ToolTipService.SetToolTip(Button_AutoUpdateBackground, "Click to auto check and update background image, only support part of games' background auto update");

                Button_SelectBackgroundFile.Content = "Choose Background File";
                ToolTipService.SetToolTip(Button_SelectBackgroundFile, "Select a file to be the background for current GameName");

                TextBlock_3DmigotoFolderPath.Text = "3Dmigoto Settings";

                Button_Choose3DmigotoPath.Content = "Choose 3Dmigoto Folder Path";
                Button_Open3DmigotoFolder.Content = "Open Current 3Dmigoto Folder";

                Button_CheckMigotoPackageUpdate.Content = "Check Update & Instal Latest 3Dmigoto Package From Github";


                TextBox_TargetPath.Header = "Target Process Path";
                Button_ChooseProcessFile.Content = "Choose Process File";
                Button_OpenProcessFileFolder.Content = "Open Target Process Folder";

                TextBox_LaunchPath.Header = "Launch Process Path";
                Button_ChooseLaunchFile.Content = "Choose Launch File";
                Button_OpenLaunchFileFolder.Content = "Open Launch File Folder";

                TextBox_LaunchArgsPath.Header = "Launch Arguments";


                CheckBox_AutoSetAnalyseOptions.Content = "Auto set analyse_options before launch";
                ToolTipService.SetToolTip(CheckBox_AutoSetAnalyseOptions, "When checked, reset d3dx.ini analyse_options to: deferred_ctx_immediate dump_rt dump_cb dump_vb dump_ib buf txt dds dump_tex dds before launch");



                CheckBox_ShowWarning.Content = "Show warnings on screen";
                ToolTipService.SetToolTip(CheckBox_ShowWarning, "When checked, 3Dmigoto warnings display in-game top-left; press F10 to reload");

                NumberBox_DllInitializationDelay.Header = "d3d11.dll Initialization Delay (ms)";
                ToolTipService.SetToolTip(NumberBox_DllInitializationDelay, "Delay in milliseconds for DLL initialization. We'll go with 200ms for WWMI:\nWuthering Waves requires at least 50ms delay since 2.4 update to not crash on startup.\nAlso, to inject Reshade along with 3dmigoto, 150ms delay is required.");


                CheckBox_DllPreProcess.Content = "Pack d3d11.dll with UPX";
                ToolTipService.SetToolTip(CheckBox_DllPreProcess, "When checked, run UPX on d3d11.dll before launch");

                Button_CleanGICache.Content = "Clean GI Log Cache Files";
                ToolTipService.SetToolTip(Button_CleanGICache, "Clear Error Log Files To Prevent Forever No-Condition Error");

                Button_RunIgnoreGIError40.Content = "Run GoodWorkGI.exe";
                ToolTipService.SetToolTip(Button_RunIgnoreGIError40, "The 4th Generation Mod Network Protect Technique");


                Button_RunLaunchPath.Content = " Start Game";


                CheckBox_PureGameMode.Content = "Pure Game Mode";
                ToolTipService.SetToolTip(CheckBox_PureGameMode, "When enabled, Start Game will skip launching 3Dmigoto, suitable for players who just want pure gameplay");

                TextBlock_BasicSettings.Text = "Basic Settings";


                CheckBox_RunWithShell.Content = "Use shell to launch";
                ToolTipService.SetToolTip(CheckBox_RunWithShell, "When checked, Start Game runs launch path via shell to slightly bypass parent-process checks");

                NumberBox_Delay.Header = "LOD.exe Auto Exit Delay (seconds)";
                ToolTipService.SetToolTip(NumberBox_Delay, "In seconds, set to -1 to prevent LOD.exe from auto exiting; otherwise set to 1 or more, default recommended is 5 seconds");

                TextBox_ExtraInjectDll.Header = "Extra Inject DLL File Path";
                Button_ChooseExtraInjectDll.Content = "Choose Extra Inject DLL File";

                CheckBox_OverwriteWWMIEngineSetting.Content = "Overwrite WWMI Engine Settings";
            }

        }

    }

}
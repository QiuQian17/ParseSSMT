using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class SettingsPage
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
                TextBlock_BasicSettings.Text = "基础设置";

                TextBlock_SSMTCacheFolder.Text = "SSMT缓存文件存放路径设置";
                Button_ChooseSSMTPackageFolder.Content = "选择缓存文件存放路径";


                ToggleSwitch_Theme.OnContent = "曜石黑";
                ToggleSwitch_Theme.OffContent = "晨曦白";

                ToggleSwitch_Chinese.OnContent = "简体中文";
                ToggleSwitch_Chinese.OffContent = "英语";


                TextBlock_About.Text = "关于";

                HyperlinkButton_SubmitIssueAndFeedback.Content = "提交错误报告与使用反馈建议";
                
                TextBlock_Help.Text = "帮助";
                HyperlinkButton_SSMTDocuments.Content = "SSMT使用文档";
                HyperlinkButton_SSMTPluginTheHerta.Content = "SSMT的Blender插件TheHerta3";
                HyperlinkButton_SSMTDiscord.Content = "SSMT Discord交流群";
                //HyperlinkButton_SSMTQQGroup.Content = "SSMT QQ公开群 794243126";

                Run_SponsorSupport.Text = "赞助支持";
                HyperlinkButton_SSMTTechCommunity.Content = "SSMT长期技术支持LTS";
                HyperlinkButton_AFDianNicoMico.Content = "爱发电:NicoMico";

                TextBlock_CheckForUpdates.Text = "检查版本更新";
                Button_AutoUpdate.Content = "自动检查新版本并更新";
                TextBlock_UpdateProgressing.Text = "自动更新下载进度:";

            


                SettingsCard_LuminosityOpacity.Header = "窗口透明度设置";
                SettingsCard_LuminosityOpacity.Description = "窗口透明度的值越小越透明看起来越炫酷，值越大越不透明看起来越厚重";

                SettingsCard_Language.Header = "语言设置";
                SettingsCard_Language.Description = "设置SSMT的界面显示语言，仅支持中文和英文";

                SettingsCard_Theme.Header = "主题颜色设置";
                SettingsCard_Theme.Description = "设置SSMT的界面主题颜色，支持晨曦白和曜石黑两种主题颜色，推荐使用晨曦白";

                


                SettingsCard_SymlinkFeature.Header = "自动设置使用Symlink特性";
                SettingsCard_SymlinkFeature.Description = "开启该选项后，启动游戏时自动设置Symlink特性到d3dx.ini，可以显著减少F8得到的帧分析转储文件总大小，显著提升转储速度，非特殊情况建议保持开启。不过需要注意此时帧分析文件根目录 .\\FrameAnalysis 下的文件基本均为符号链接，指向去重文件目录 .\\FrameAnalysis\\Deduped 下的真正文件。";

                ToggleSwitch_UseSymlinkFeature.OnContent = "开启";
                ToggleSwitch_UseSymlinkFeature.OffContent = "关闭";



                SettingsCard_TextureConversionFormat.Header = "贴图转换格式";
                SettingsCard_TextureConversionFormat.Description = "选择当前逆向后的贴图转换格式";


                SettingsCard_PostReverseAction.Header = "逆向完成后操作";
                SettingsCard_PostReverseAction.Description = "选择逆向完成后的操作";

                ComboBoxItem_OpenFolder.Content = "打开逆向好的文件夹";

                ToolTipService.SetToolTip(ComboBoxItem_OpenFolder,"逆向成功后会打开逆向好的文件夹，逆向失败的话会直接打开运行日志方便排查问题");

                ComboBoxItem_ShowDialog.Content = "显示逆向成功对话框";
                ToolTipService.SetToolTip(ComboBoxItem_ShowDialog,"推荐在大批量逆向Mod时使用此选项，避免每次都打开一个新的文件夹导致任务栏空间占满");

                ComboBoxItem_OpenLatestLogFile.Content = "打开最新的运行日志文件";

                ToolTipService.SetToolTip(ComboBoxItem_OpenLatestLogFile,"不管逆向成功或者失败，都打开运行日志，方便在频繁测试时排查问题使用，一般只有插件开发者会用到，用户不推荐使用此选项");

                SettingsCard_ConvertOriginalTextures.Header = "转换原始贴图";
                SettingsCard_ConvertOriginalTextures.Description = "控制是否转换原始Mod里的贴图文件";
                ToggleSwitch_ConvertOriginalTextures.OnContent = "启用";
                ToggleSwitch_ConvertOriginalTextures.OffContent = "禁用";

                SettingsCard_ConvertTexturesToOutputFolder.Header = "转换贴图到输出文件夹";
                SettingsCard_ConvertTexturesToOutputFolder.Description = "控制是否转换贴图到逆向输出文件夹";
                ToggleSwitch_ConvertTexturesToOutputFolder.OnContent = "启用";
                ToggleSwitch_ConvertTexturesToOutputFolder.OffContent = "禁用";

                TextBlock_ReverseSettings.Text = "Mod逆向设置";

                SettingsCard_PostRunGameAction.Header = "开始游戏后行为";
                SettingsCard_PostRunGameAction.Description = "决定了在主页点击开始游戏运行完毕后默认执行的行为";

                ComboBoxItem_PostRunGameAction_None.Content = "无操作";
                ComboBoxItem_PostRunGameAction_MinimizeToTrayIcon.Content = "隐藏SSMT到右下角通知区域";
                ComboBoxItem_PostRunGameAction_CloseSSMT.Content = "关闭SSMT";

                SettingsCard_PlayerMode.Header = "玩家模式";
                SettingsCard_PlayerMode.Description = "开启玩家模式后，左侧导航栏将只保留主页、Mod管理页面以及设置页面";

                ToggleSwitch_PlayerMode.OnContent = "开启";
                ToggleSwitch_PlayerMode.OffContent = "关闭";

                Expander_GithubToken.Header = "Github Token";
            }
            else
            {
                TextBlock_BasicSettings.Text = "Basic Settings";

                TextBlock_SSMTCacheFolder.Text = "SSMT Cache Folder";
                Button_ChooseSSMTPackageFolder.Content = "Choose Cache Folder";



                ToggleSwitch_Theme.OnContent = "Dark";
                ToggleSwitch_Theme.OffContent = "Light";

                ToggleSwitch_Chinese.OnContent = "Chinese(zh-CN)";
                ToggleSwitch_Chinese.OffContent = "English(en-US)";




                TextBlock_About.Text = "About";

                HyperlinkButton_SubmitIssueAndFeedback.Content = "Submit Issue And Feedback";

                TextBlock_Help.Text = "Help";
                HyperlinkButton_SSMTDocuments.Content = "SSMT Documents";
                HyperlinkButton_SSMTPluginTheHerta.Content = "SSMT's Blender Plugin: TheHerta";
                HyperlinkButton_SSMTDiscord.Content = "SSMT Discord Server";
                //HyperlinkButton_SSMTQQGroup.Content = "SSMT QQGroup 794243126";

                Run_SponsorSupport.Text = "Sponsor Support";
                HyperlinkButton_SSMTTechCommunity.Content = "SSMT Tech Community";
                HyperlinkButton_AFDianNicoMico.Content = "afdian: NicoMico";

                TextBlock_CheckForUpdates.Text = "Check Version Update";
                Button_AutoUpdate.Content = "Auto Update To Latest Version";
                TextBlock_UpdateProgressing.Text = "Auto Update Download Progress:";

         


                SettingsCard_Language.Header = "Language Setting";
                SettingsCard_Language.Description = "Decide what kind of language ssmt show,only support English and Chinese";

                SettingsCard_Theme.Header = "Theme Setting";
                SettingsCard_Theme.Description = "Decide what kind of theme color ssmt use, support Light and Dark theme, Light is recommended";

                SettingsCard_LuminosityOpacity.Header = "Window Opacity Setting";
                SettingsCard_LuminosityOpacity.Description = "The smaller the value, the more transparent the window looks cool; the larger the value, the more opaque the window looks heavy";


                SettingsCard_SymlinkFeature.Header = "Auto Set Use Symlink Feature";
                SettingsCard_SymlinkFeature.Description = "Enabling this option will automatically set the Symlink feature in d3dx.ini when launching the game, which can significantly reduce the total size of frame analysis dump files obtained by F8 and greatly improve dump speed. It is recommended to keep it enabled unless under special circumstances. However, please note that the files in the root directory .\\FrameAnalysis of the frame analysis files are basically symbolic links pointing to the actual files in the deduplicated file directory .\\FrameAnalysis\\Deduped.";

                ToggleSwitch_UseSymlinkFeature.OnContent = "Enable";
                ToggleSwitch_UseSymlinkFeature.OffContent = "Disable";


                SettingsCard_TextureConversionFormat.Header = "Texture Conversion Format";
                SettingsCard_TextureConversionFormat.Description = "Select the texture conversion format";

                SettingsCard_PostReverseAction.Header = "Post Reverse Action";
                SettingsCard_PostReverseAction.Description = "Select the action after reverse operation";

                ComboBoxItem_OpenFolder.Content = "Open Reversed Folder";
                ComboBoxItem_ShowDialog.Content = "Show Success Dialog";
                ComboBoxItem_OpenLatestLogFile.Content = "Open Latest Reverse Log File";

                SettingsCard_ConvertOriginalTextures.Header = "Convert Original Textures";
                SettingsCard_ConvertOriginalTextures.Description = "Control whether to convert original textures in the mod";
                ToggleSwitch_ConvertOriginalTextures.OnContent = "Enable";
                ToggleSwitch_ConvertOriginalTextures.OffContent = "Disable";

                SettingsCard_ConvertTexturesToOutputFolder.Header = "Convert Textures to Output Folder";
                SettingsCard_ConvertTexturesToOutputFolder.Description = "Control whether to convert textures to the reverse output folder";
                ToggleSwitch_ConvertTexturesToOutputFolder.OnContent = "Enable";
                ToggleSwitch_ConvertTexturesToOutputFolder.OffContent = "Disable";

                TextBlock_ReverseSettings.Text = "Mod Reverse Settings";

                SettingsCard_PostRunGameAction.Header = "Post Run Game Action";
                SettingsCard_PostRunGameAction.Description = "Decide the default action after running the game from the home page";


                ComboBoxItem_PostRunGameAction_None.Content = "No Action";
                ComboBoxItem_PostRunGameAction_MinimizeToTrayIcon.Content = "Minimize SSMT to Tray Icon";
                ComboBoxItem_PostRunGameAction_CloseSSMT.Content = "Close SSMT";

                SettingsCard_PlayerMode.Header = "Player Mode";
                SettingsCard_PlayerMode.Description = "When Player Mode is enabled, the left navigation bar will only retain the Home, Mod Management, and Settings pages";

                ToggleSwitch_PlayerMode.OnContent = "Enable";
                ToggleSwitch_PlayerMode.OffContent = "Disable";

                Expander_GithubToken.Header = "Github Token";
            }

        }

    }
}

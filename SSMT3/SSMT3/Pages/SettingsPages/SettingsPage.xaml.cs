using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SSMT3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        bool ReadOver = false;

        bool CheckUpdateIng = false;

        public SettingsPage()
        {
            this.InitializeComponent();
            
            HyperlinkButton_SSMTVersion.Content = PathManager.SSMT_Title;

            try
            {
                ReadSettingsFromConfig();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show("Error: " + ex.ToString());
            }
        }


        public void SaveSettingsToConfig()
        {
            try
            {
                Debug.WriteLine("保存配置");
                GlobalConfig.SSMTCacheFolderPath = TextBox_SSMTPackagePath.Text;


                GlobalConfig.WindowLuminosityOpacity = Slider_LuminosityOpacity.Value;

                GlobalConfig.Chinese = ToggleSwitch_Chinese.IsOn;
                GlobalConfig.Theme = ToggleSwitch_Theme.IsOn;
                GlobalConfig.UseSymlinkFeature = ToggleSwitch_UseSymlinkFeature.IsOn;
                GlobalConfig.PlayerMode = ToggleSwitch_PlayerMode.IsOn;
                GlobalConfig.GithubToken = TextBox_GithubToken.Text.Trim();

                // Save new settings
                if (ComboBox_PostReverseAction.SelectedIndex >= 0)
                {
                    GlobalConfig.PostReverseAction = ComboBox_PostReverseAction.SelectedIndex;
                }

                if (ComboBox_TextureConversionFormat.SelectedItem is ComboBoxItem selectedTextureFormat)
                {
                    GlobalConfig.TextureConversionFormat = selectedTextureFormat.Content.ToString();
                }

                GlobalConfig.ConvertOriginalTextures = ToggleSwitch_ConvertOriginalTextures.IsOn;
                GlobalConfig.ConvertTexturesToOutputFolder = ToggleSwitch_ConvertTexturesToOutputFolder.IsOn;

                if (ComboBox_PostRunGameAction.SelectedIndex >= 0)
                {
                    GlobalConfig.PostRunGameAction = ComboBox_PostRunGameAction.SelectedIndex;
                }

                GlobalConfig.SaveConfig();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
            
        }

        public void ReadSettingsFromConfig()
        {
            ReadOver = false;
            //防止程序启动时没正确读取，这里冗余读取一次，后面看情况可以去掉
            GlobalConfig.ReadConfig();

            TextBox_SSMTPackagePath.Text = GlobalConfig.SSMTCacheFolderPath;



            Slider_LuminosityOpacity.Value = GlobalConfig.WindowLuminosityOpacity;

            ToggleSwitch_Theme.IsOn = GlobalConfig.Theme;
            ToggleSwitch_Chinese.IsOn = GlobalConfig.Chinese;
            
            ToggleSwitch_UseSymlinkFeature.IsOn = GlobalConfig.UseSymlinkFeature;
            ToggleSwitch_PlayerMode.IsOn = GlobalConfig.PlayerMode;

            if (GlobalConfig.PostReverseAction >= 0 && GlobalConfig.PostReverseAction < ComboBox_PostReverseAction.Items.Count)
            {
                ComboBox_PostReverseAction.SelectedIndex = GlobalConfig.PostReverseAction;
            }

            if (GlobalConfig.PostRunGameAction >= 0 && GlobalConfig.PostRunGameAction < ComboBox_PostRunGameAction.Items.Count)
            {
                ComboBox_PostRunGameAction.SelectedIndex = GlobalConfig.PostRunGameAction;
            }

            foreach (ComboBoxItem item in ComboBox_TextureConversionFormat.Items)
            {
                if (item.Content.ToString() == GlobalConfig.TextureConversionFormat)
                {
                    ComboBox_TextureConversionFormat.SelectedItem = item;
                    break;
                }
            }

            ToggleSwitch_ConvertOriginalTextures.IsOn = GlobalConfig.ConvertOriginalTextures;
            ToggleSwitch_ConvertTexturesToOutputFolder.IsOn = GlobalConfig.ConvertTexturesToOutputFolder;

            TextBox_GithubToken.Text = GlobalConfig.GithubToken;

            ReadOver = true;
        }


        private async void Button_ChooseSSMTPackageFolder_Click(object sender, RoutedEventArgs e)
        {
            string FolderPath = await SSMTCommandHelper.ChooseFolderAndGetPath();

            if (FolderPath == ""
)
            {
                return;
            }

            if (Directory.Exists(FolderPath))
            {

                TextBox_SSMTPackagePath.Text = FolderPath;
                GlobalConfig.SSMTCacheFolderPath = FolderPath;
                GlobalConfig.SaveConfig();
            }

            SSMTResourceUtils.InitializeWorkFolder(true);
            _ = SSMTMessageHelper.Show("缓存文件夹路径设置成功!", "Success");
        }

        private void TextBox_SSMTPackageFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TextBox_SSMTPackagePath.Text))
            {
                GlobalConfig.SSMTCacheFolderPath = TextBox_SSMTPackagePath.Text;
                GlobalConfig.SaveConfig();

                SSMTResourceUtils.InitializeWorkFolder(true);
            }

        }

        /// <summary>
        /// 任何设置项被改变后，都应该立刻调用这个方法，否则无法同步状态。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshSettings(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }






        private async void Button_AutoUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUpdateIng)
            {
                _ = SSMTMessageHelper.Show("请稍等，正在检查是否有新版本中，速度取决于你的电脑联通Github的速度，无需重复检测。");
                return;
            }

            try
            {
                CheckUpdateIng = true;
                var source = new GithubSource("https://github.com/StarBobis/SSMT/", GlobalConfig.GithubToken, false);

                var mgr = new UpdateManager(source);
                // check for new version
                ProgressRing_UpdateRing.Visibility = Visibility.Visible;
                var newVersion = await mgr.CheckForUpdatesAsync();
                ProgressRing_UpdateRing.Visibility = Visibility.Collapsed;
                CheckUpdateIng = false;

                if (newVersion == null)
                {


                    await SSMTMessageHelper.Show("您当前的版本已经是最新版本","Your version is already latest version.");
                    return; // no update available
                }
                else
                {
                    bool UpdateConfirm = await SSMTMessageHelper.ShowConfirm("检测到可用的新版本! 是否开始自动更新?","Available new version detected, do you want to update?");

                    if (UpdateConfirm)
                    {
                        // 显示进度条
                        TextBlock_UpdateProgressing.Visibility = Visibility.Visible;
                        ProgressBar_AutoUpdate.Visibility = Visibility.Visible;

                        // 创建进度回调（使用Action<int>）
                        Action<int> progress = p =>
                        {
                            // 使用Dispatcher确保UI线程更新
                            ProgressBar_AutoUpdate.DispatcherQueue.TryEnqueue(() =>
                            {
                                ProgressBar_AutoUpdate.Value = p; // 直接使用整数值
                            });
                        };

                        // download new version
                        await mgr.DownloadUpdatesAsync(newVersion, progress);

                        // install new version and restart app
                        mgr.ApplyUpdatesAndRestart(newVersion);
                    }
                    else
                    {
                        // 用户取消更新，隐藏进度条
                        TextBlock_UpdateProgressing.Visibility = Visibility.Collapsed;
                        ProgressBar_AutoUpdate.Visibility = Visibility.Collapsed;
                    }

                }
                

            }
            catch (Exception ex)
            {
                ProgressRing_UpdateRing.Visibility = Visibility.Collapsed;
                CheckUpdateIng = false;
                // 发生异常，隐藏进度条
                TextBlock_UpdateProgressing.Visibility = Visibility.Collapsed;
                ProgressBar_AutoUpdate.Visibility = Visibility.Collapsed;

                string message = null;

                // 检查是否是 Octokit.ApiException（GitHub API 错误）
                if (ex.GetType().Name == "ApiException" && ex.Message.Contains("401"))
                {
                    message = "更新失败：GitHub 拒绝访问（401 Unauthorized）。\n可能是 GitHub API 限流或未授权访问导致，请稍后重试或检查网络代理。";
                }
                // 检查是否为普通 HttpRequestException
                else if (ex is HttpRequestException httpEx && httpEx.Message.Contains("401"))
                {
                    message = "更新失败：网络请求返回 401 未授权。\n请检查您的网络环境或代理设置。";
                }
                // 检查是否为 Octokit 的 rate limit（403）
                else if (ex.Message.Contains("403") && ex.Message.Contains("rate"))
                {
                    message = "更新失败：GitHub 访问频率受限（403 Rate Limit）。\n请稍后再试。";
                }
                // 其他常见错误（DNS、网络中断）
                else if (ex is HttpRequestException || ex.Message.Contains("NameResolutionFailure"))
                {
                    message = "无法连接到 GitHub，请检查网络连接或系统代理。";
                }

                if (message == null)
                {
                    // 默认兜底
                    message = "自动更新失败。\n详细错误信息如下，请联系开发者添加报错处理：\n" + ex.Message;
                }

                await SSMTMessageHelper.Show(message);
            }

        }



        private void ToggleSwitch_Theme_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                if (ToggleSwitch_Theme.IsOn)
                {
                    if (this.XamlRoot?.Content is FrameworkElement root)
                    {
                        root.RequestedTheme = ElementTheme.Dark;
                        MainWindow.CurrentWindow._controller.TintColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);
                    }
                }
                else
                {
                    if (this.XamlRoot?.Content is FrameworkElement root)
                    {
                        root.RequestedTheme = ElementTheme.Light;
                        MainWindow.CurrentWindow._controller.TintColor = Windows.UI.Color.FromArgb(255, 245, 245, 245);
                    }

                }
                SaveSettingsToConfig();
            }
        }

        private void ToggleSwitch_Chinese_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();

                //这里必须触发重新访问，因为仅仅调用翻译的话，对于部分组件是有BUG的
                //例如ComboBox的选项就不会被翻译，不信可以自己解开试试
                //WinUI3基本上全是这种大大小小的坑
                Frame.Navigate(typeof(SettingsPage));

                //TranslatePage();
            }
        }

      

        private void Slider_LuminosityOpacity_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ReadOver)
            {
                MainWindow.CurrentWindow._controller.LuminosityOpacity = (float)Slider_LuminosityOpacity.Value;
            }
        }




     

        private void Slider_LuminosityOpacity_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettingsToConfig();

        }

   

        

    





      

        private void ToggleSwitch_UseSymlinkFeature_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }



        private void ComboBox_TextureConversionFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }

        private void ComboBox_PostReverseAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }

        private void ToggleSwitch_ConvertOriginalTextures_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }

        private void ToggleSwitch_ConvertTexturesToOutputFolder_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }

        private void ComboBox_PostRunGameAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }

        private void ToggleSwitch_PlayerMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }

            MainWindow.CurrentWindow.SetPlayerMode();
        }

        private void TextBox_GithubToken_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using SSMT3.Core.Utils;
using SSMT3.InfoItemClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUI3Helper;

namespace SSMT3
{
    public partial class MainWindow
    {
        //缓存上一次的页面，防止重复加载
        private string _lastBackgroundHtml;

        private void BackgroundWebView_CoreWebView2Initialized(object sender, CoreWebView2InitializedEventArgs e)
        {
            if (e.Exception is null)
            {
                // 将虚拟主机名映射到 SSMT 缓存文件夹的根目录
                // 这样可以确保所有背景视频（无论在哪个子文件夹）都可以通过相对路径访问
                BackgroundWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "assets.ssmt.local",
                    GlobalConfig.SSMTCacheFolderPath,
                    CoreWebView2HostResourceAccessKind.Allow);
            }
        }


        private void ResetBackground()
        {

            // 隐藏视频
            BackgroundWebView.Visibility = Visibility.Collapsed;

            // 清空静态图
            MainWindowImageBrush.Visibility = Visibility.Collapsed;
            MainWindowImageBrush.Source = null;
        }

        public async void ShowBackgroundVideo(string path, string TargetGameName)
        {
            if (GlobalConfig.CurrentGameName != TargetGameName)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                LOG.Info("背景视频文件不存在: " + path);
                return;
            }


            // 清空静态图
            MainWindowImageBrush.Visibility = Visibility.Collapsed;
            MainWindowImageBrush.Source = null;

            if (BackgroundWebView.Visibility == Visibility.Collapsed)
            {
                BackgroundWebView.Visibility = Visibility.Visible;
            }

            // 初始化 WebView2
            await BackgroundWebView.EnsureCoreWebView2Async();


            try
            {
                var core = BackgroundWebView.CoreWebView2;
                core.IsMuted = true;

                // 针对当前视频目录进行虚拟主机映射（视频可能不在缓存目录下）
                string videoDir = Path.GetDirectoryName(path)!;
                string fileName = Path.GetFileName(path);

                // 固定主机名，每次更新到当前目录（同名会覆盖旧映射）
                core.SetVirtualHostNameToFolderMapping(
                    "assets.ssmt.local",
                    videoDir,
                    CoreWebView2HostResourceAccessKind.Allow);

                // 使用文件名构造可访问的 URL
                string videoUrl = $"https://assets.ssmt.local/{Uri.EscapeDataString(fileName)}?gamename={GlobalConfig.CurrentGameName}";
                //string videoUrl = $"https://assets.ssmt.local/{Uri.EscapeDataString(fileName)}?t={DateTime.Now.Ticks}";

                LOG.Info("VideoUrl: " + videoUrl);

                // 选择 mime type
                string type = path.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) ? "video/webm" : "video/mp4";

                // 注入 HTML 播放视频
                var htmlContent = $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
html,body{{margin:0;padding:0;background:transparent;overflow:hidden;}}
video{{position:absolute;left:0;top:0;width:100%;height:100%;object-fit:cover;}}
</style>
</head>
<body>
<video autoplay loop muted playsinline>
  <source src='{videoUrl}' type='{type}'>
</video>
</body>
</html>";


                if (!string.Equals(_lastBackgroundHtml, htmlContent, StringComparison.Ordinal))
                {
                    _lastBackgroundHtml = htmlContent;
                    BackgroundWebView.NavigateToString(htmlContent);
                    VisualHelper.CreateFadeAnimation(BackgroundWebView);

                }
            }
            catch (Exception ex)
            {
                LOG.Info("加载背景视频失败: " + ex);
                BackgroundWebView.Visibility = Visibility.Collapsed;
            }
        }


        public void ShowBackgroundPicture(string path, string TargetGameName)
        {
            if (GlobalConfig.CurrentGameName != TargetGameName)
            {
                return;
            }

            // 隐藏视频
            BackgroundWebView.Visibility = Visibility.Collapsed;

            if (MainWindowImageBrush.Visibility == Visibility.Collapsed)
            {
                MainWindowImageBrush.Visibility = Visibility.Visible;
            }

            //VisualHelper.CreateScaleAnimation(imageVisual);
            //VisualHelper.CreateFadeAnimation(imageVisual);

            // 强制刷新图片链接（你之前已有相同逻辑）
            MainWindowImageBrush.Source =
                new BitmapImage(new Uri(path + "?t=" + DateTime.Now.Ticks));
        }


        /// <summary>
        /// 用于更新游戏前，删除已存在的背景图
        /// </summary>
        /// <returns></returns>
        public void DeleteAllBackgroundFile()
        {
            //来一个支持的后缀名列表，然后依次判断
            List<BackgroundSuffixItem> SuffixList = new List<BackgroundSuffixItem>();
            //这里顺序可有讲究了，在此特别说明
            //首先就是有MP4的情况下优先加载MP4，因为.webm会转换为.mp4格式来作为背景图
            SuffixList.Add(new BackgroundSuffixItem { Suffix = ".mp4", IsVideo = true });
            SuffixList.Add(new BackgroundSuffixItem { Suffix = ".webm", IsVideo = true });
            SuffixList.Add(new BackgroundSuffixItem { Suffix = ".webp", IsPicture = true });
            SuffixList.Add(new BackgroundSuffixItem { Suffix = ".png", IsPicture = true });

            //这里轮着试一遍所有的背景图类型，如果有的话就设置上了
            //如果没有的话就保持刚开始初始化完那种没有的状态了
            string TargetGameFolderPath = Path.Combine(PathManager.Path_GamesFolder, GlobalConfig.CurrentGameName);

            foreach (BackgroundSuffixItem SuffixItem in SuffixList)
            {
                string BackgroundFilePath = Path.Combine(TargetGameFolderPath, "Background" + SuffixItem.Suffix);

                if (File.Exists(BackgroundFilePath))
                {
                    File.Delete(BackgroundFilePath);
                }
            }

        }

        public async Task InitializeBackground(string TargetGame)
        {


            //来一个支持的后缀名列表，然后依次判断
            List<BackgroundSuffixItem> SuffixList = new List<BackgroundSuffixItem>();
            //这里顺序可有讲究了，在此特别说明
            //首先就是有MP4的情况下优先加载MP4，因为.webm会转换为.mp4格式来作为背景图
            SuffixList.Add(new BackgroundSuffixItem { Suffix = ".mp4", IsVideo = true });
            SuffixList.Add(new BackgroundSuffixItem { Suffix = ".webm", IsVideo = true });
            SuffixList.Add(new BackgroundSuffixItem { Suffix = ".webp", IsPicture = true });
            SuffixList.Add(new BackgroundSuffixItem { Suffix = ".png", IsPicture = true });

            //这里轮着试一遍所有的背景图类型，如果有的话就设置上了
            //如果没有的话就保持刚开始初始化完那种没有的状态了
            string TargetGameFolderPath = Path.Combine(PathManager.Path_GamesFolder, TargetGame + "\\");

            bool BackgroundExists = false;
            foreach (BackgroundSuffixItem SuffixItem in SuffixList)
            {
                string BackgroundFilePath = Path.Combine(TargetGameFolderPath, "Background" + SuffixItem.Suffix);

                if (!File.Exists(BackgroundFilePath))
                {
                    continue;
                }

                if (SuffixItem.IsVideo)
                {
                    ShowBackgroundVideo(BackgroundFilePath, TargetGame);
                    BackgroundExists = true;
                    break;
                }
                else if (SuffixItem.IsPicture)
                {
                    ShowBackgroundPicture(BackgroundFilePath, TargetGame);
                    BackgroundExists = true;
                    break;
                }

            }


            //米的四个游戏保底更新背景图，主要是为了用户第一次拿到手SSMT的时候就能有背景图
            if (!BackgroundExists)
            {
                //只有米的四个游戏会根据游戏名称默认触发保底背景图更新
                try
                {


                    if (TargetGame == LogicName.GIMI ||
                        TargetGame == LogicName.SRMI ||
                        TargetGame == LogicName.HIMI ||
                        TargetGame == LogicName.ZZMI
                        )
                    {
                        string PossibleWebpPicture = Path.Combine(TargetGameFolderPath, "Background.webp");
                        string PossiblePngBackgroundPath = Path.Combine(TargetGameFolderPath, "Background.png");


                        if (!File.Exists(PossibleWebpPicture))
                        {
                            if (!File.Exists(PossiblePngBackgroundPath))
                            {
                                await AutoUpdateBackgroundPicture(TargetGame, TargetGame);
                            }
                        }
                    }
                    else if (TargetGame == LogicName.WWMI ||
                             TargetGame == LogicName.WuWa
                        )
                    {
                        string PossibleMp4Picture = Path.Combine(TargetGameFolderPath, "Background.mp4");

                        if (!File.Exists(PossibleMp4Picture))
                        {
                            await AutoUpdateBackgroundPicture(TargetGame);
                        }
                    }
                    else
                    {
                        MainWindow.CurrentWindow.ResetBackground();
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

        }

        public async Task AutoUpdateBackgroundPicture(string TargetGame, string SpecificLogicName = "")
        {
            ResetBackground();
            MainWindow.CurrentWindow.DeleteAllBackgroundFile();

            string GameId = HoyoBackgroundUtils.GetGameId(SpecificLogicName, GlobalConfig.Chinese);

            if (GameId == "")
            {
                _ = SSMTMessageHelper.Show("当前选择的执行逻辑: " + SpecificLogicName + " 暂不支持自动更新背景图，请手动设置。");
                return;
            }

            string BaseUrl = HoyoBackgroundUtils.GetBackgroundUrl(GameId, GlobalConfig.Chinese);

            bool UseWebmBackground = false;

            try
            {
                string NewWebmBackgroundPath = await HoyoBackgroundUtils.DownloadLatestWebmBackground(BaseUrl, TargetGame);

                if (File.Exists(NewWebmBackgroundPath))
                {
                    UseWebmBackground = true;
                }



                ShowBackgroundVideo(NewWebmBackgroundPath, TargetGame);
                LOG.Info("设置好背景图视频了");
            }
            catch (Exception ex)
            {
                LOG.Info(ex.ToString());
            }

            if (UseWebmBackground)
            {
                LOG.Info("用上视频背景图了，后面内容不管了");
                return;
            }

            string NewWebpBackgroundPath = await HoyoBackgroundUtils.DownloadLatestWebpBackground(BaseUrl);
            if (File.Exists(NewWebpBackgroundPath))
            {
                ShowBackgroundPicture(NewWebpBackgroundPath, TargetGame);
            }

        }

        // 重载一份用于鸣潮的自动更新背景视频逻辑
        public async Task AutoUpdateBackgroundPicture(string TargetGame)
        {
            ResetBackground();
            MainWindow.CurrentWindow.DeleteAllBackgroundFile();

            string videoUrl = await KuroBackgroundUtils.GetBackgroundUrlAsync();

            // 如果没有获取到视频的Url地址, 那么直接返回, 让背景为默认的黑色背景
            if (string.IsNullOrEmpty(videoUrl))
            {
                LOG.Error($"游戏: {TargetGame} 的背景加载失败");
                return;
            }

            try
            {
                string NewMp4BackgroundPath = await KuroBackgroundUtils.DownloadBackgroundAsync(videoUrl, TargetGame);

                if (File.Exists(NewMp4BackgroundPath) && !string.IsNullOrEmpty(NewMp4BackgroundPath))
                {
                    ShowBackgroundVideo(NewMp4BackgroundPath, TargetGame);
                    LOG.Info($"游戏: {TargetGame} 的背景加载成功");
                }
                else
                {
                    LOG.Error($"游戏: {TargetGame} 的背景文件不存在");
                }
            }
            catch (Exception ex)
            {
                LOG.Error($"游戏: {TargetGame} 的背景加载失败" + Environment.NewLine + ex.ToString());
            }
        }
    }
}

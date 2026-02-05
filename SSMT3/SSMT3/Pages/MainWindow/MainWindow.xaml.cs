using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using SSMT3;
using SSMT3.Core.Win32;
using SSMT3.InfoItemClass;
using SSMT3.Pages.AutoReversePage;
using SSMT3.Pages.ManuallyReversePage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinRT;
using WinUI3Helper;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public TrayIcon _trayIcon;

        /// <summary>
        /// 视觉效果组件
        /// </summary>
        private Visual imageVisual;

        /// <summary>
        /// 背景透明效果控制器
        /// </summary>
        public DesktopAcrylicController _controller;

        public static MainWindow CurrentWindow;

        public Image MainWindowImageBrushW;


        public MainWindow()
        {
            InitializeComponent();


            // 构造函数中（InitializeComponent 之后）添加
            _trayIcon = new TrayIcon(this, "Assets\\XiaoMai.ico", "SSMT3 正在后台运行");

            // 初始化Composition组件
            // 获取Image控件的Visual对象
            imageVisual = ElementCompositionPreview.GetElementVisual(MainWindowImageBrush);
            BackgroundWebView.CoreWebView2Initialized += BackgroundWebView_CoreWebView2Initialized;

            //全局配置文件夹不存在就创建一个
            if (!Directory.Exists(PathManager.Path_SSMT3GlobalConfigsFolder))
            {
                Directory.CreateDirectory(PathManager.Path_SSMT3GlobalConfigsFolder);
            }

            CurrentWindow = this;


            // 1. 把窗口变成可以挂系统背景的目标
            var target = this.As<ICompositionSupportsSystemBackdrop>();

            // 2. 创建 Acrylic-Thin 控制器
            _controller = new DesktopAcrylicController()
            {

                Kind = DesktopAcrylicKind.Thin,   // ← 这就是“Thin透明”的关键
                LuminosityOpacity = 0.65f,  //遮挡光线程度
                TintOpacity = 0.1f //moh
            };

            // 3. 挂到窗口并激活
            _controller.AddSystemBackdropTarget(target);
            _controller.SetSystemBackdropConfiguration(new SystemBackdropConfiguration { IsInputActive = true });

            try
            {
                GlobalConfig.ReadConfig();

                if (GlobalConfig.Theme)
                {
                    WindowHelper.SetTheme(this, ElementTheme.Dark);
                    _controller.TintColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);
                }
                else
                {
                    WindowHelper.SetTheme(this, ElementTheme.Light);
                    _controller.TintColor = Windows.UI.Color.FromArgb(255, 245, 245, 245);
                }
            }
            catch (Exception ex)
            {
                LOG.Info("Parse Error");
                ex.ToString();
            }

            _controller.LuminosityOpacity = (float)GlobalConfig.WindowLuminosityOpacity;

            GlobalConfig.SaveConfig();

            this.ExtendsContentIntoTitleBar = true;

            //设置标题和宽高
            this.Title = PathManager.SSMT_Title;
            //设置图标
            this.AppWindow.SetIcon("Assets/XiaoMai.ico");


            //启动的时候就检查SSMT缓存文件夹是否设置正确，如果不正确就去设置，直到正确了才允许跳转其它页面
            if (GlobalConfig.SSMTCacheFolderPath == "" || !Directory.Exists(GlobalConfig.SSMTCacheFolderPath))
            {
                //如果不存在，那没办法了，只能创建一个了

                string DefaultCacheLocation = System.IO.Path.Combine(PathManager.Path_AppDataLocal, "SSMTDefaultCacheFolder\\");
                if (!Directory.Exists(DefaultCacheLocation))
                {
                    Directory.CreateDirectory(DefaultCacheLocation);
                }

                GlobalConfig.SSMTCacheFolderPath = DefaultCacheLocation;
                GlobalConfig.SaveConfig();

            }

            SSMTResourceUtils.InitializeWorkFolder(false);


            //默认选中主页界面
            contentFrame.Navigate(typeof(HomePage));




            TranslatePage();

            ResetWindow();

            SetPlayerMode();
        }

        public void SetPlayerMode()
        {
            if (GlobalConfig.PlayerMode)
            {
                NavigationViewItem_TexturePage.Visibility = Visibility.Collapsed;
                NavigationViewItem_WorkPage.Visibility = Visibility.Collapsed;
                NavigationViewItem_TextureToolBoxPage.Visibility = Visibility.Collapsed;
                NavigationViewItem_GameTypePage.Visibility = Visibility.Collapsed;
                NavigationViewItem_ManuallyReversePage.Visibility = Visibility.Collapsed;
                NavigationViewItem_AutoReversePage.Visibility = Visibility.Collapsed;
                NavigationViewItem_ProtectPage.Visibility = Visibility.Collapsed;
                

            }
            else
            {
                NavigationViewItem_TexturePage.Visibility = Visibility.Visible;
                NavigationViewItem_WorkPage.Visibility = Visibility.Visible;
                NavigationViewItem_TextureToolBoxPage.Visibility = Visibility.Visible;
                NavigationViewItem_GameTypePage.Visibility = Visibility.Visible;
                NavigationViewItem_ManuallyReversePage.Visibility = Visibility.Visible;
                NavigationViewItem_AutoReversePage.Visibility = Visibility.Visible;
                NavigationViewItem_ProtectPage.Visibility = Visibility.Visible;

            }

        }


        private void ResetWindow()
        {
            double logicalWidth = GlobalConfig.WindowWidth;
            double logicalHeight = GlobalConfig.WindowHeight;

            WindowHelper.SetSmartSizeAndMoveToCenter(AppWindow, (int)(logicalWidth), (int)(logicalHeight));
        }











        private void nvSample_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // PxncAcd: 添加重复点击设置页则回退到前一页的功能
            if (args.IsSettingsInvoked)
            {
                // 如果当前页面就是设置页，则返回上一页

                if (contentFrame.CurrentSourcePageType == typeof(SettingsPage))
                {
                    if (contentFrame.CanGoBack)
                    {
                        contentFrame.GoBack();
                    }
                    return;
                }

                // 否则导航到设置页
                contentFrame.Navigate(typeof(SettingsPage));

                if (BackgroundWebView.Visibility == Visibility.Collapsed)
                {
                    _ = InitializeBackground(GlobalConfig.CurrentGameName);

                }

                Rectangle_NavigationShadow.Visibility = Visibility.Collapsed;
                SettingsPageBackgroundOverlay.Visibility = Visibility.Visible;

                //ResetBackground();
            }
            else if (args.InvokedItemContainer is NavigationViewItem item)
            {
                Rectangle_NavigationShadow.Visibility = Visibility.Visible;
                SettingsPageBackgroundOverlay.Visibility = Visibility.Collapsed;
                var pageTag = item.Tag.ToString();
                Type pageType = null;



                switch (pageTag)
                {
                    case "HomePage":
                        pageType = typeof(HomePage);
                        break;

                   
                    case "WorkPage":
                        pageType = typeof(WorkPage);
                        break;
                    case "TexturePage":
                        pageType = typeof(TexturePage);
                        break;
                    case "TextureToolBoxPage":
                        pageType = typeof(TextureToolBoxPage);
                        break;
                    case "GameTypePage":
                        pageType = typeof(GameTypePage);
                        break;
                    case "ModManagePage":
                        pageType = typeof(ModManagePage);
                        break;
                    case "DocumentPage":
                        pageType = typeof(DocumentPage);
                        break;
                    case "ProtectPage":
                        pageType = typeof(ProtectPage);
                        break;
                    case "AutoReversePage":
                        pageType = typeof(AutoReversePage);
                        break;
                    case "ManuallyReversePage":
                        pageType = typeof(ManuallyReversePage);
                        break;
                }



                if (pageType != null)
                {

                    if (pageType != typeof(HomePage))
                    {
                        ResetBackground();
                    }
                   

                    if (contentFrame.Content?.GetType() != pageType)
                    {
                        //如果当前点击的页面不是当前页面，就跳转到目标页面
                        contentFrame.Navigate(pageType);
                    }

                }


            }

        }



        private void Window_Closed(object sender, WindowEventArgs args)
        {

            //退出程序时，保存窗口大小
            //用户反馈蓝屏的时候，全局配置文件会损坏导致SSMT无法启动，启动后闪退。
            //所以不管是保存还是读取配置都应该有TryCatch，
            //咱们已经有了，但是这玩意高低也算个小坑，特此记录。
            GlobalConfig.WindowWidth = App.m_window.AppWindow.Size.Width;
            GlobalConfig.WindowHeight = App.m_window.AppWindow.Size.Height;
            GlobalConfig.SaveConfig();

            //不释放资源就会出现那个0x0000005的内存访问异常
            //但是没有任何文档对此有所说明
            //可恶的WinUI3
            try
            {
                _controller?.RemoveAllSystemBackdropTargets();
                _controller?.Dispose();
                _controller = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backdrop cleanup failed: {ex}");
            }

            _trayIcon.Dispose();
            _trayIcon = null;

        }




        private void nvSample_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (contentFrame.CanGoBack)
            {
                contentFrame.GoBack();
            }
        }

        private void contentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //此函数的作用是，在Frame每次Navigate调用后，自动设置选中项为当前跳转到的页面。
            //这样就不需要手动设置了

            // 确保返回按钮状态正确更新
            nvSample.IsBackEnabled = contentFrame.CanGoBack;

            // 假设页面类名和 Tag 有对应关系，如 "HomePage" -> "HomePage"
            // 为了让这种方法能够很方便的生效，以后都要符合这种命名约定
            string tag = contentFrame.SourcePageType.Name;

            nvSample.SelectedItem = nvSample.MenuItems.OfType<NavigationViewItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == tag) ?? null;

            // 处理设置页面的背景显示逻辑
            if (contentFrame.SourcePageType == typeof(SettingsPage))
            {
                SettingsPageBackgroundOverlay.Visibility = Visibility.Visible;
                Rectangle_NavigationShadow.Visibility = Visibility.Collapsed;
            }
            else
            {
                SettingsPageBackgroundOverlay.Visibility = Visibility.Collapsed;
                Rectangle_NavigationShadow.Visibility = Visibility.Visible;
            }
        }

      
    }
}

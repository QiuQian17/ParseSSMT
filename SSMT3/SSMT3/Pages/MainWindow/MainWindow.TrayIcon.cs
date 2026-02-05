using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class MainWindow
    {

        public void MinimizeToTrayAndNotify()
        {
            // 1. 隐藏窗口（保留后台运行）
            this.AppWindow.Hide();   // 若想仅最小化，可改用 this.AppWindow.Minimize();

            // 2. 发送 Windows 通知提醒用户
            var notification = new AppNotificationBuilder()
                .AddText("SSMT3已最小化到托盘")
                .AddText("程序仍在后台持续运行")
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }


        public void RestoreWindow()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                this.AppWindow.Show();   // 如果之前 Hide() 了，先 Show
                this.Activate();         // 让窗口前置
            });
        }

    }
}

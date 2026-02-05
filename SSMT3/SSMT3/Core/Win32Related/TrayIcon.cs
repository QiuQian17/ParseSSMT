using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3.Core.Win32
{
    public class TrayIcon :IDisposable
    {

        private const uint WM_APP = 0x8000;
        private const uint WM_TRAY = WM_APP + 1;
        private const int WM_COMMAND = 0x0111;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        private const uint NIF_MESSAGE = 0x00000001;
        private const uint NIF_ICON = 0x00000002;
        private const uint NIF_TIP = 0x00000004;
        private const uint NIM_ADD = 0x00000000;
        private const uint NIM_DELETE = 0x00000002;

        private const uint MF_STRING = 0x00000000;
        private const uint TPM_RIGHTBUTTON = 0x0002;

        private const int IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;
        private const uint LR_DEFAULTSIZE = 0x00000040;
        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);
        private static readonly IntPtr IDI_APPLICATION = new IntPtr(32512);

        private readonly string _className;
        private readonly WndProc _wndProcDelegate;
        private readonly IntPtr _messageWindow;
        private readonly IntPtr _iconHandle;
        private readonly uint _iconId = 1;
        private readonly Window _mainWindow;

        public TrayIcon(Window mainWindow, string iconRelativePath = "Assets\\XiaoMai.ico", string tooltip = "SSMT3")
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _className = "SSMT3_Tray_" + Guid.NewGuid().ToString("N");
            _wndProcDelegate = WindowProc;

            var wndClass = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<WNDCLASSEX>(),
                lpfnWndProc = _wndProcDelegate,
                lpszClassName = _className
            };
            RegisterClassEx(ref wndClass);

            _messageWindow = CreateWindowEx(
                0, _className, string.Empty, 0,
                0, 0, 0, 0,
                HWND_MESSAGE, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            string iconPath = iconRelativePath;
            if (!Path.IsPathRooted(iconPath))
            {
                iconPath = Path.Combine(AppContext.BaseDirectory, iconRelativePath);
            }

            _iconHandle = File.Exists(iconPath)
                ? LoadImage(IntPtr.Zero, iconPath, IMAGE_ICON, 0, 0, LR_LOADFROMFILE | LR_DEFAULTSIZE)
                : LoadIcon(IntPtr.Zero, IDI_APPLICATION);

            var nid = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _messageWindow,
                uID = _iconId,
                uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                uCallbackMessage = WM_TRAY,
                hIcon = _iconHandle,
                szTip = tooltip
            };
            Shell_NotifyIcon(NIM_ADD, ref nid);
        }

        public void Dispose()
        {
            var nid = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _messageWindow,
                uID = _iconId
            };
            Shell_NotifyIcon(NIM_DELETE, ref nid);

            if (_iconHandle != IntPtr.Zero && _iconHandle != LoadIcon(IntPtr.Zero, IDI_APPLICATION))
            {
                DestroyIcon(_iconHandle);
            }

            if (_messageWindow != IntPtr.Zero)
            {
                DestroyWindow(_messageWindow);
            }

            UnregisterClass(_className, IntPtr.Zero);
        }

        private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_TRAY)
            {
                int l = lParam.ToInt32();
                if (l == WM_LBUTTONDBLCLK)
                {
                    _mainWindow.DispatcherQueue.TryEnqueue(_mainWindow.Activate);
                }
                else if (l == WM_RBUTTONUP)
                {
                    uint pos = GetMessagePos();
                    int x = (short)(pos & 0xffff);
                    int y = (short)((pos >> 16) & 0xffff);
                    ShowContextMenu(x, y);
                }
            }
            else if (msg == WM_COMMAND)
            {
                int command = wParam.ToInt32() & 0xffff;
                switch (command)
                {
                    case 1000:

                        //先Show再Activate，避免窗口没有被正确激活的问题
                        _mainWindow.AppWindow.Show();
                        _mainWindow.DispatcherQueue.TryEnqueue(_mainWindow.Activate);
                        break;
                    case 1001:
                        _mainWindow.DispatcherQueue.TryEnqueue(_mainWindow.Close);
                        break;
                }
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void ShowContextMenu(int x, int y)
        {
            IntPtr hMenu = CreatePopupMenu();
            AppendMenu(hMenu, MF_STRING, 1000, "打开 SSMT3");
            AppendMenu(hMenu, MF_STRING, 1001, "退出");

            SetForegroundWindow(_messageWindow);
            TrackPopupMenu(hMenu, TPM_RIGHTBUTTON, x, y, 0, _messageWindow, IntPtr.Zero);
            DestroyMenu(hMenu);
        }

        #region Native

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] _reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASSEX
        {
            public int cbSize;
            public int style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetMessagePos();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll")]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, int uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

        #endregion
    }
}

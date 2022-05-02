#pragma warning disable CS8604
#pragma warning disable CS8605
#pragma warning disable CS8619
#pragma warning disable CS0649

using BotwInstaller.Lib;
using BotwInstaller.Lib.Remote;
using BotwInstaller.Wizard.ViewModels;
using BotwInstaller.Wizard.ViewThemes.App;
using BotwScripts.Lib.Common.Computer;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BotwInstaller.Wizard.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        private readonly IWindowManager WindowManager;

        #region Fix Window Size in fullscreen.

        internal static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam, int minWidth, int minHeight)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = minWidth;
                mmi.ptMinTrackSize.y = minHeight;
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new();
            public RECT rcWork = new();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object? obj)
            {
                if (obj is not Rect) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        #endregion

        public ShellView()
        {
            #region Template Setters

            InitializeComponent();
            DataContext = new ShellViewModel(WindowManager);

            if (File.Exists(ShellViewTheme.ThemeFile))
            {
                ShellViewTheme.ThemeStr = "Light";
                Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsLight), Grid.MarginProperty, new Thickness(0), 100);
                ShellViewTheme.Change(true);
            }
            else
            {
                ShellViewTheme.ThemeStr = "Dark";
                Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsDark), Grid.MarginProperty, new Thickness(0), 100);
                ShellViewTheme.Change();
            }

            // Load button close/minimize events
            btnExit.Click += (s, e) => { Hide(); Environment.Exit(1); };
            btnMin.Click += (s, e) => WindowState = WindowState.Minimized;
            btnReSize.Click += (s, e) =>
            {

                if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
                else
                    WindowState = WindowState.Normal;
            };

            SourceInitialized += async (s, e) =>
            {
                System.Windows.Forms.Application.ThreadException += new ThreadExceptionEventHandler(((ShellViewModel)DataContext).Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(((ShellViewModel)DataContext).CurrentDomain_UnhandledException);

                using HttpClient client = new();
                GameInfo.ModPresetData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>[]>>>(await client.GetStringAsync(HttpLinks.ModPresets));
            };

            // Assign state changed events
            shellView.StateChanged += (s, e) =>
            {

                if (WindowState == WindowState.Normal)
                {
                    rectCascade.Opacity = 0;
                    rectMaximize.Opacity = 1;
                }
                else
                {
                    rectCascade.Opacity = 1;
                    rectMaximize.Opacity = 0;
                }
            };

            // Change app theme event
            footerChangeAppTheme.Click += async (s, e) =>
            {
                if (ShellViewTheme.ThemeStr == "Dark")
                {
                    ShellViewTheme.ThemeStr = "Light";
                    ShellViewTheme.Change(true);
                    Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsDark), Grid.MarginProperty, new Thickness(0, 0, 0, 34), 250);
                    await Task.Run(() => Thread.Sleep(200));
                    Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsLight), Grid.MarginProperty, new Thickness(0), 200);
                }
                else
                {
                    ShellViewTheme.ThemeStr = "Dark";
                    ShellViewTheme.Change();
                    Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsLight), Grid.MarginProperty, new Thickness(0, 34, 0, 0), 250);
                    await Task.Run(() => Thread.Sleep(200));
                    Animation.ThicknessAnim(footerChangeAppTheme_GridParent, nameof(footerChangeAppTheme_IsDark), Grid.MarginProperty, new Thickness(0), 200);
                }
            };

            footerRequestHelp.Click += async (s, e) => await HiddenProcess.Start("explorer.exe", "https://github.com/ArchLeaders/BotwInstaller-RE#readme");

            #endregion

            // . . .
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            Process.Start("explorer.exe", link.NavigateUri.ToString());
        }
    }
}

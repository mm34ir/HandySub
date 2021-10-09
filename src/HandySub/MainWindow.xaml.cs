using HandySub.Common;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace HandySub
{
    public sealed partial class MainWindow : Window
    {
        internal static MainWindow Instance {  get; set; }
        private AppWindow m_appWindow;
        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;
            m_appWindow = GetAppWindowForCurrentWindow();
            if (m_appWindow != null)
            {
                m_appWindow.Title = "HandySub";
            }
        }
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ((sender as Grid).XamlRoot.Content as Grid).RequestedTheme = Helper.Settings.ApplicationTheme;
        }
    }
}

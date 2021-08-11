using HandySub.Models;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using System;
using WinRT;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;
using HandySub.Common;

namespace HandySub
{
    public partial class App : Application
    {
        public static StartupArgumentModel StartUpArguments = new StartupArgumentModel
        {
            Name = string.Empty,
            Path = string.Empty
        };
        public App()
        {
            this.InitializeComponent();
            //AppCenter.Start(Consts.AppSecret, typeof(Analytics), typeof(Crashes));
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            var windowNative = m_window.As<IWindowNative>();
            m_windowHandle = windowNative.WindowHandle;
            m_window.Title = "HandySub";
            m_window.Activate();
        }

        private Window m_window;
        private IntPtr m_windowHandle;
        public IntPtr WindowHandle { get { return m_windowHandle; } }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
    }
}

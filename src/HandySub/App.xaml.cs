using HandySub.Models;
using Microsoft.UI.Xaml;

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
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}

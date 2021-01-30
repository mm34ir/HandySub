using System;
using System.Collections.Generic;
using System.Windows.Media;
using HandyControl.Tools;
using HandySub.Language;
using HandySub.Model;
using ModernWpf.Controls;

namespace HandySub
{
    internal class AppConfig
    {
        public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json";

        public string UILang { get; set; } = "en-US";

        public LanguageModel SubtitleLanguage { get; set; } = new()
            {LanguageCode = "farsi_persian", DisplayName = Lang.SLFarsi, LocalizeCode = "SLFarsi"};

        public string StoreLocation { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
        public string ServerUrl { get; set; } = "https://sub.deltaleech.com";
        public bool IsContextMenuFile { get; set; } = true;
        public bool IsShowNotification { get; set; } = true;
        public bool IsIDMEngine { get; set; } = false;
        public bool IsKeepAlive { get; set; } = false;
        public bool IsBackVisible { get; set; } = false;
        public bool IsFirstRun { get; set; } = true;
        public List<string> History = new List<string>();
        public NavigationViewPaneDisplayMode PaneDisplayMode { get; set; } = NavigationViewPaneDisplayMode.Left;

        public ApplicationTheme Theme { get; set; } = ApplicationTheme.Light;
        public Brush Accent { get; set; } = ResourceHelper.GetResource<Brush>("PrimaryBrush");
    }
}
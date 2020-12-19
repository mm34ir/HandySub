using System;
using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Language;
using HandySub.Model;
using ModernWpf.Controls;

namespace HandySub
{
    internal class AppConfig : GlobalDataHelper<AppConfig>
    {
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
        public NavigationViewPaneDisplayMode PaneDisplayMode { get; set; } = NavigationViewPaneDisplayMode.Left;

        public SkinType Skin { get; set; } = SkinType.Default;
    }
}
using HandyControl.Controls;
using HandyControl.Data;
using SubtitleDownloader.Language;
using SubtitleDownloader.Model;
using System;

namespace SubtitleDownloader
{
    internal class AppConfig : GlobalDataHelper<AppConfig>
    {
        public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json";

        public string UILang { get; set; } = "fa-IR";
        public LanguageModel SubtitleLanguage { get; set; } = new LanguageModel { LanguageCode = "farsi_persian", DisplayName = Lang.SLFarsi };
        public string StoreLocation { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
        public string ServerUrl { get; set; } = "https://sub.deltaleech.com";
        public bool IsAutoDownloadSubtitle { get; set; } = false;
        public bool IsContextMenuFile { get; set; } = true;
        public bool IsContextMenuFolder { get; set; } = true;
        public bool IsShowNotification { get; set; } = true;
        public bool IsShowNotifyIcon { get; set; } = false;
        public bool IsFirstRun { get; set; } = true;
        public bool IsIDMEngine { get; set; } = false;

        public SkinType Skin { get; set; } = SkinType.Default;
    }
}
using HandyControl.Data;
using System;

namespace SubtitleDownloader
{
    internal class AppConfig
    {
        public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json";

        public string UILang { get; set; } = "en";
        public string SubtitleLang { get; set; } = "farsi_persian";
        public string StoreLocation { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
        public string ServerUrl { get; set; } = "https://subf2m.co";
        public bool IsAutoDownloadSubtitle { get; set; } = false;
        public bool IsAutoSelectOpenedTab { get; set; } = true;
        public bool IsContextMenuFile { get; set; } = true;
        public bool IsContextMenuFolder { get; set; } = true;
        public bool IsDraggableTab { get; set; } = false;

        public SkinType Skin { get; set; } = SkinType.Dark;
    }
}
using HandyControl.Themes;
using HandySub.Models;
using ModernWpf.Controls;
using Nucs.JsonSettings;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace HandySub.Assets
{
    public class ISettings : JsonSettings
    {
        public override string FileName { get; set; } = Consts.ConfigPath;

        #region Property

        public virtual int Version { get; set; } = 13991222;
        
        public virtual string InterfaceLanguage { get; set; } = "en-US";
        public virtual string SubtitleLanguage { get; set; } = "All";
        public virtual string StoreLocation { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
        public virtual ServerModel SubsceneServer { get; set; } = new ServerModel { Index = 1, Key = "Delta Leech", Url = "https://sub.deltaleech.com"};
        public virtual ServerModel ShellServer { get; set; } = new ServerModel { Index = 0};
        public virtual bool IsFirstRun { get; set; } = true;
        public virtual bool IsBackEnabled { get; set; } = true;
        public virtual bool IsIDMEnabled { get; set; } = false;
        public virtual bool IsAddToContextMenu { get; set; } = true;
        public virtual bool IsOpenHandySubWithContext { get; set; } = true;
        public virtual bool IsShowNotification { get; set; } = true;
        public virtual int MaxHistoryNumber { get; set; } = 19;
        public virtual List<string> History { get; set; } = new List<string>();
        public virtual NavigationViewPaneDisplayMode PaneDisplayMode { get; set; } = NavigationViewPaneDisplayMode.Left;
        public virtual ApplicationTheme Theme { get; set; } = ApplicationTheme.Light;
        public virtual Brush Accent { get; set; }

        #endregion Property

        public ISettings()
        {
            Version = 13991223;
        }

        public ISettings(string fileName) : base(fileName)
        {
        }
    }
}

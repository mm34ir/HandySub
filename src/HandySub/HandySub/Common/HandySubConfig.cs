using Microsoft.UI.Xaml;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Modulation;
using System;
using HandySub.Models;
using System.Collections.ObjectModel;

namespace HandySub.Common
{
    public class HandySubConfig : JsonSettings, IVersionable
    {
        [EnforcedVersion("4.0.0.1")]
        public virtual Version Version { get; set; } = new Version(4, 0, 0, 0);
        public override string FileName { get; set; } = Constants.AppConfigPath;

        public virtual string SubtitleLanguage { get; set; } = Constants.ALL_Language;
        public virtual string DefaultDownloadLocation { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
        public virtual string FileNameRegex { get; set; } = Constants.FileNameRegex;
        public virtual string SubtitleQuality { get; set; } = Constants.ALL_Qualities;
        public virtual string LastCheckedUpdate { get; set; } = "Never";

        public virtual ServerModel SubsceneServer { get; set; } = new ServerModel { Index = 0, Key = "Subf2m", Url = "https://subf2m.co" };

        public virtual ServerModel ShellServer { get; set; } = new ServerModel { Index = 0 };
        public virtual bool IsFirstRun { get; set; } = true;
        public virtual bool IsIDMEnabled { get; set; } = false;
        public virtual bool IsAddToContextMenuEnabled { get; set; } = true;
        public virtual bool IsOpenHandySubWithContextMenuEnabled { get; set; } = true;
        public virtual bool IsShowNotificationEnabled { get; set; } = true;
        public virtual bool IsAutoDeCompressEnabled { get; set; } = false;
        public virtual bool IsDoubleClickEnabled { get; set; } = true;
        public virtual bool IsDoubleClickDownloadEnabled { get; set; } = false;
        public virtual bool IsDefaultRegexEnabled { get; set; } = true;
        public virtual bool IsSoundEnabled { get; set; } = false;
        public virtual bool IsSpatialSoundEnabled { get; set; } = false;
        public virtual bool IsHistoryEnabled { get; set; } = true;
        public virtual ObservableCollection<string> SearchHistory { get; set; } = new ObservableCollection<string>();
        public virtual ElementTheme ApplicationTheme { get; set; } = ElementTheme.Default;
    }
}

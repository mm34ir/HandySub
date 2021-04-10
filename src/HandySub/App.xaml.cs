using HandyControl.Themes;
using HandyControl.Tools;
using HandySub.Assets;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using static HandySub.Assets.Helper;
namespace HandySub
{
    public partial class App : Application
    {
        public static StartupArgumentModel StartUpArguments = new StartupArgumentModel 
        { 
           Name = string.Empty,
           Path = string.Empty
        };

        string[] BAD_WORDS = {
            "hdtv", "exm", "RMT", "DD5", "YTS", "TURKISH", "VIDEOFLIX", "Gisaengchung", "KOREAN", "8CH",
            "BluRay", "Hdcam", "-", "XviD", "AC3", "EVO", "WEBRip", "FGT", "MP3", "CMRG", "Pahe", "webdl",
            "10bit", "720p", "1080p", "480p", "WEB-DL", "H264", "H265", "x264", "x265", "800MB", "900MB",
            "HEVC", "PSA", "RARBG", "6CH", "2CH", "CAMRip", "Rip", "AVS", "RMX", "RMTeam", "mSD", ".",
            "SVA", "MkvCage", "MeGusta", "TBS", "AMZN", "DDP5.1", "DDP5", "SHITBOX", "NITRO", "WEB", "DL",
            "1080", "720", "480", "MrMovie", "BWBP", "NTG", "HMAX", "Atmos", "MZABI", "2018", "2019", "2020",
            "2021", "2022", "MRCS", "/", "GalaxyRG", "HDR", "YTS.LT", "1400MB", "H.264", "H.265", "YTS.MX",
            "DV", "PSiG", "ION10", "NTb", "SYNCOPY", "PHOENIX", "MinX", "300MB", "150MB", "AFG", "Cakes",
            "2010", "2011", "2012", "2013", "2014", "2015", "2016", "2017"
        };

        public App()
        {
            ApplicationHelper.StartProfileOptimization(Consts.CachePath);

            if (!Settings.Version.Equals(RegistryHelper.GetValue<int>(Consts.VersionKey, Consts.AppName)))
            {
                if (File.Exists(Consts.ConfigPath))
                {
                    File.Delete(Consts.ConfigPath);
                }
                RegistryHelper.AddOrUpdateKey(Consts.VersionKey, Consts.AppName, Settings.Version);

                Settings = JsonSettings.Load<ISettings>().EnableAutosave();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigHelper.Instance.SetLang(Settings.InterfaceLanguage);
            
            UpdateTheme(Settings.Theme);
            UpdateAccent(Settings.Accent);

            if (e.Args.Length > 0)
            {
                var NameFromContextMenu = RemoveBadWords(Path.GetFileNameWithoutExtension(e.Args[0]));
                StartUpArguments = new StartupArgumentModel
                {
                    Name = NameFromContextMenu,
                    Path = e.Args[0].Replace(Path.GetFileName(e.Args[0]), "")
                };
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            AppCenter.Start(Consts.AppSecret, typeof(Analytics), typeof(Crashes));
        }

        public string RemoveBadWords(string stringToClean)
        {
            var cleaned = string.Join(" ", stringToClean.Split(new string[] { " ", ".", "-" }, StringSplitOptions.None).Where(w => !BAD_WORDS.Contains(w, StringComparer.OrdinalIgnoreCase)));

            cleaned = Regex.Replace(cleaned, @"S[0-9].{1}E[0-9].{1}", "", RegexOptions.IgnoreCase); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"(\[[^\]]*\])|(\([^\)]*\))", ""); // remove between () and []
            cleaned = Regex.Replace(cleaned, "[ ]{2,}", " "); // remove space [More than 2 space] and replace with one space

            return cleaned.Trim();
        }

        internal void UpdateTheme(ApplicationTheme theme)
        {
            if (ThemeManager.Current.ApplicationTheme != theme)
            {
                ThemeManager.Current.ApplicationTheme = theme;
                ModernWpf.ThemeManager.Current.ApplicationTheme = theme == ApplicationTheme.Light ? (ModernWpf.ApplicationTheme?)ModernWpf.ApplicationTheme.Light : (ModernWpf.ApplicationTheme?)ModernWpf.ApplicationTheme.Dark;
            }
        }

        internal void UpdateAccent(Brush accent)
        {
            if (ThemeManager.Current.AccentColor != accent)
            {
                ThemeManager.Current.AccentColor = accent;
                ModernWpf.ThemeManager.Current.AccentColor = accent == null ? null : ColorHelper.GetColorFromBrush(accent);
            }
        }
    }
}

using HandyControl.Controls;
using HandyControl.Themes;
using HandyControl.Tools;
using HandySub.Assets;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Collections.Generic;
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
        public static string[] WindowsContextMenuArgument = { string.Empty, string.Empty };

        private readonly List<string> wordsToRemove =
            ". hdtv exm RMT DD5 YTS TURKISH VIDEOFLIX Gisaengchung KOREAN 8CH BluRay Hdcam HDCAM - XviD AC3 EVO WEBRip FGT MP3 CMRG Pahe 10bit 720p 1080p 480p WEB-DL H264 H265 x264 x265 800MB 900MB HEVC PSA RARBG 6CH 2CH CAMRip Rip AVS RMX HDTV RMTeam mSD SVA MkvCage MeGusta TBS AMZN DDP5.1 DDP5 SHITBOX NITRO WEB DL 1080 720 480 MrMovie BWBP NTG "
                .Split(' ').ToList();
        public App()
        {
            ApplicationHelper.StartProfileOptimization(Consts.CachePath);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigHelper.Instance.SetLang(Settings.InterfaceLanguage);
            if (!Settings.Version.Equals(RegistryHelper.GetValue<int>(Consts.VersionKey, Consts.AppName)))
            {
                if (File.Exists(Consts.ConfigPath))
                {
                    File.Delete(Consts.ConfigPath);
                }
                RegistryHelper.AddOrUpdateKey(Consts.VersionKey, Consts.AppName, Settings.Version);
            }

            UpdateTheme(Settings.Theme);
            UpdateAccent(Settings.Accent);

            if (e.Args.Length > 0)
            {
                var NameFromContextMenu = RemoveJunkString(Path.GetFileNameWithoutExtension(e.Args[0]));

                WindowsContextMenuArgument[0] = NameFromContextMenu;
                WindowsContextMenuArgument[1] = e.Args[0].Replace(Path.GetFileName(e.Args[0]), "");
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            AppCenter.Start(Consts.AppSecret, typeof(Analytics), typeof(Crashes));
        }

        public string RemoveJunkString(string stringToClean)
        {
            var cleaned = Regex.Replace(stringToClean, "\\b" + string.Join("\\b|\\b", wordsToRemove) + "\\b", " ");
            cleaned = Regex.Replace(cleaned, @"S[0-9].{1}E[0-9].{1}", ""); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"s[0-9].{1}e[0-9].{1}", ""); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"(\[[^\]]*\])|(\([^\)]*\))", ""); // remove between () and []
            cleaned = Regex.Replace(cleaned, "[ ]{2,}",
                " "); // remove space [More than 2 space] and replace with one space

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

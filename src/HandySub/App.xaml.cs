using HandyControl.Themes;
using HandyControl.Tools;
using HandySub.Assets;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using System.IO;
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

        string BAD_WORDS_Pattern = @"(?:hd(?:tv|cam|r)|e(?:xm|vo)|RMT|DDP?5(?:\.1)?|YTS|Turkish|VideoFlix|Gisaengchung|Korean|8CH|BluRay|-|XVid|A(?:c3|VS)|web(?:-?(?:rip|dl))?|fgt|mp3|cmrg|pahe|10bit|(?:720|480|1080)[pi]?|H\.?26[45]|x26[45]|\d{3,}MB|H(?:MAX|EVC)|PS(?:A|iG)|RARBG|[26]CH|(?:CAM)?Rip|RM(?:X|Team)|msd|sva|mkvcage|megusta|tbs|amz|shitbox|nitro|Mr(?:Movie|CS)|BWBP|NT[bG]|Atmos|MZABI|20(?:1\d|2[01])|\/|GalaxyRG|YTS(?:\.(?:LT|MX))?|DV|ION10|SYNCOPY|Phoenix|Minx|AFG|Cakes|@Gemovies|M3|\.)";

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

                Settings = JsonSettings.Load<Config>().EnableAutosave();
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
            Regex wordFilter = new Regex(BAD_WORDS_Pattern, RegexOptions.IgnoreCase);
            var cleaned = Regex.Replace(stringToClean, @"S[0-9].{1}E[0-9].{1}", "", RegexOptions.IgnoreCase); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"(\[[^\]]*\])|(\([^\)]*\))", ""); // remove between () and []

            cleaned = wordFilter.Replace(cleaned, " ").Trim();
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

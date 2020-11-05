using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using HandyControl.Tools;
using HandySub.Language;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace HandySub
{
    public partial class App : Application
    {
        public static string[] WindowsContextMenuArgument = { string.Empty, string.Empty };

        private readonly List<string> wordsToRemove = ". hdtv exm RMT DD5 YTS TURKISH VIDEOFLIX Gisaengchung KOREAN 8CH BluRay Hdcam HDCAM - XviD AC3 EVO WEBRip FGT MP3 CMRG Pahe 10bit 720p 1080p 480p WEB-DL H264 H265 x264 x265 800MB 900MB HEVC PSA RARBG 6CH 2CH CAMRip Rip AVS RMX HDTV RMTeam mSD SVA MkvCage MeGusta TBS AMZN DDP5.1 DDP5 SHITBOX NITRO WEB DL 1080 720 480 MrMovie BWBP NTG "
           .Split(' ').ToList();

        public App()
        {
            GlobalDataHelper<AppConfig>.Init($"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json");
            LocalizationManager.Instance.LocalizationProvider = new ResxProvider();
            LocalizationManager.Instance.CurrentCulture = new System.Globalization.CultureInfo(GlobalDataHelper<AppConfig>.Config.UILang);


            //init Appcenter Crash Reporter
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                   typeof(Analytics), typeof(Crashes));
        }

        public string RemoveJunkString(string stringToClean)
        {
            string cleaned = Regex.Replace(stringToClean, "\\b" + string.Join("\\b|\\b", wordsToRemove) + "\\b", " ");
            cleaned = Regex.Replace(cleaned, @"S[0-9].{1}E[0-9].{1}", ""); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"s[0-9].{1}e[0-9].{1}", ""); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"(\[[^\]]*\])|(\([^\)]*\))", ""); // remove between () and []
            cleaned = Regex.Replace(cleaned, "[ ]{2,}", " "); // remove space [More than 2 space] and replace with one space

            return cleaned.Trim();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var boot = new Bootstrapper();
            boot.Run();
            ConfigHelper.Instance.SetLang(GlobalDataHelper<AppConfig>.Config.UILang);

            if (e.Args.Length > 0)
            {
                string NameFromContextMenu = RemoveJunkString(Path.GetFileNameWithoutExtension(e.Args[0]));

                WindowsContextMenuArgument[0] = NameFromContextMenu;
                WindowsContextMenuArgument[1] = e.Args[0].Replace(Path.GetFileName(e.Args[0]), "");
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public void UpdateSkin(SkinType skin)
        {
            SharedResourceDictionary.SharedDictionaries.Clear();
            ResourceHelper.GetTheme("hcTheme", Resources).Skin = skin;

            var tm = ThemeManager.Current;
            if (tm.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                tm.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                tm.ApplicationTheme = ApplicationTheme.Dark;
            }
            Current.MainWindow?.OnApplyTemplate();
        }
    }
}

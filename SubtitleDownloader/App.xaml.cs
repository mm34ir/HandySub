using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace SubtitleDownloader
{
    public partial class App
    {
        public static string[] WindowsContextMenuArgument = { string.Empty, string.Empty };

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            GlobalData.Init();

            //init Appcenter Crash Reporter
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                               typeof(Analytics), typeof(Crashes));

            //set Lang
            ConfigHelper.Instance.SetLang(GlobalData.Config.UILang);

            //set Skin
            if (GlobalData.Config.Skin != SkinType.Default)
            {
                UpdateSkin(GlobalData.Config.Skin);
            }

            if (e.Args.Length > 0)
            {
                //this words must be remove
                var replacements = new[]{
                   new{Find="Hdcam",Replace=" "},
                   new{Find="HDCAM",Replace=" "},
                   new{Find=".",Replace=" "},
                   new{Find="-",Replace=" "},
                   new{Find="XviD",Replace=" "},
                   new{Find="AC3",Replace=" "},
                   new{Find="EVO",Replace=" "},
                   new{Find="WEBRip",Replace=" "},
                   new{Find="FGT",Replace=" "},
                   new{Find="MP3",Replace=" "},
                   new{Find="CMRG",Replace=" "},
                   new{Find="Pahe",Replace=" "},
                   new{Find="10bit",Replace=" "},
                   new{Find="720p",Replace=" "},
                   new{Find="1080p",Replace=" "},
                   new{Find="480p",Replace=" "},
                   new{Find="WEB-DL",Replace=" "},
                   new{Find="H264",Replace=" "},
                   new{Find="H265",Replace=" "},
                   new{Find="x264",Replace=" "},
                   new{Find="x265",Replace=" "},
                   new{Find="800MB",Replace=" "},
                   new{Find="900MB",Replace=" "},
                   new{Find="HEVC",Replace=" "},
                   new{Find="PSA",Replace=" "},
                   new{Find="RARBG",Replace=" "},
                   new{Find="6CH",Replace=" "},
                   new{Find="2CH",Replace=" "},
                   new{Find="CAMRip",Replace=" "},
                   new{Find="Rip",Replace=" "},
                   new{Find="AVS",Replace=" "},
                   new{Find="RMX",Replace=" "},
                   new{Find="HDTV",Replace=" "},
                   new{Find="RMTeam",Replace=" "},
                   new{Find="mSD",Replace=" "},
                   new{Find="SVA",Replace=" "},
                   new{Find="MkvCage",Replace=" "},
                   new{Find="MeGusta",Replace=" "},
                   new{Find="TBS",Replace=" "},
                   new{Find="AMZN",Replace=" "},
                   new{Find="DDP5.1",Replace=" "},
                   new{Find="SHITBOX",Replace=" "},
                   new{Find="NITRO",Replace=" "},
                   new{Find="WEB DL",Replace=" "},
                   new{Find="1080",Replace=" "},
                   new{Find="720",Replace=" "},
                   new{Find="480",Replace=" "},
                   new{Find="MrMovie",Replace=" "}
                            };

                var NameFromContextMenu = replacements.Aggregate(Path.GetFileNameWithoutExtension(e.Args[0]), (current, set) => current.Replace(set.Find, set.Replace));
                NameFromContextMenu = Regex.Replace(NameFromContextMenu, @"(\[[^\]]*\])|(\([^\)]*\))", ""); // remove between () and []
                NameFromContextMenu = Regex.Replace(NameFromContextMenu, "S[0-9].{1}E[0-9].{1}", ""); // remove SXXEXX ==> X is 0-9
                NameFromContextMenu = Regex.Replace(NameFromContextMenu, "[ ]{2,}", " "); // remove space [More than 2 space] and replace with one space

                //get ContextMenu Argument
                if (e.Args.Length == 1)
                {
                    WindowsContextMenuArgument[0] = NameFromContextMenu;
                }
                else if (e.Args.Length == 2)
                {
                    WindowsContextMenuArgument[1] = NameFromContextMenu;
                }
            }


            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        internal void UpdateSkin(SkinType skin)
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/Skin{skin.ToString()}.xaml")
            });
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
        }
    }
}

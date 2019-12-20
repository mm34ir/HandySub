using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Net;
using System.Windows;

namespace SubtitleDownloader
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            GlobalData.Init();

            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                               typeof(Analytics), typeof(Crashes));

            ConfigHelper.Instance.SetLang(GlobalData.Config.UILang);

            if (GlobalData.Config.Skin != SkinType.Default)
            {
                UpdateSkin(GlobalData.Config.Skin);
            }

            if (e.Args.Length > 0)
            {
                GlobalData.Config.ContextMenuTemp = Path.GetFileNameWithoutExtension(e.Args[0]);
                GlobalData.Save();
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            GlobalData.Config.ContextMenuTemp = string.Empty;
            GlobalData.Save();
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

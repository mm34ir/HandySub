using System;
using System.Windows;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;

namespace SubtitleDownloader
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var skin = InIHelper.ReadValue(SettingsKey.Skin);
            var lang = InIHelper.ReadValue(SettingsKey.Language);
            if (skin.Contains("Dark"))
                UpdateSkin(SkinType.Dark);
            else if (skin.Contains("Violet"))
                UpdateSkin(SkinType.Violet);
            else
                UpdateSkin(SkinType.Default);
                if(System.IO.File.Exists("config.ini"))
                    ConfigHelper.Instance.SetLang(lang);
                else
                    ConfigHelper.Instance.SetLang("en");

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

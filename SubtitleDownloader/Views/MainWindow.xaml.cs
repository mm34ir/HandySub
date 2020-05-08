using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools;
using SubtitleDownloader.Language;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
namespace SubtitleDownloader.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Change Skin and Language
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e)
        {
            PopupConfig.IsOpen = true;
        }

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is SkinType tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(GlobalDataHelper<AppConfig>.Config.Skin))
                {
                    return;
                }

                GlobalDataHelper<AppConfig>.Config.Skin = tag;
                GlobalDataHelper<AppConfig>.Save();
                ((App)Application.Current).UpdateSkin(tag);
            }
        }

        private void ButtonLangs_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is string tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(GlobalDataHelper<AppConfig>.Config.UILang))
                {
                    return;
                }

                Growl.Ask(Lang.ResourceManager.GetString("ChangeLanguage"), b =>
                {
                    if (!b)
                    {
                        return true;
                    }

                    GlobalDataHelper<AppConfig>.Config.UILang = tag;
                    GlobalDataHelper<AppConfig>.Save();
                    LocalizationManager.Instance.CurrentCulture = new System.Globalization.CultureInfo(tag);
                    ConfigHelper.Instance.SetLang(GlobalDataHelper<AppConfig>.Config.UILang);
                    return true;
                });
            }
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (GlobalDataHelper<AppConfig>.Config.IsShowNotifyIcon)
            {
                if (GlobalDataHelper<AppConfig>.Config.IsFirstRun)
                {
                    MessageBoxResult result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
                    {
                        Message = Lang.ResourceManager.GetString("RunInBackgroundMainWindow"),
                        Caption = Lang.ResourceManager.GetString("Title"),
                        Button = MessageBoxButton.YesNo,
                        IconBrushKey = ResourceToken.AccentBrush,
                        IconKey = ResourceToken.InfoGeometry
                    });
                    if (result == MessageBoxResult.Yes)
                    {
                        Hide();
                        e.Cancel = true;
                        GlobalDataHelper<AppConfig>.Config.IsFirstRun = false;
                        GlobalDataHelper<AppConfig>.Save();
                    }
                    else
                    {
                        base.OnClosing(e);
                    }
                }
                else
                {
                    Hide();
                    e.Cancel = true;
                }
            }
            else
            {
                base.OnClosing(e);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

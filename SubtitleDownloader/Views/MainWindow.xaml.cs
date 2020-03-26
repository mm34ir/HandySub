using HandyControl.Controls;
using HandyControl.Data;
using SubtitleDownloader.DynamicLanguage;
using SubtitleDownloader.Language;
using SubtitleDownloader.ViewModels;
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
                if (tag.Equals(GlobalData.Config.Skin))
                {
                    return;
                }

                GlobalData.Config.Skin = tag;
                GlobalData.Save();
                ((App)Application.Current).UpdateSkin(tag);
            }
        }

        private void ButtonLangs_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is string tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(GlobalData.Config.UILang))
                {
                    return;
                }

                Growl.Ask(Lang.ResourceManager.GetString("ChangeLanguage"), b =>
                {
                    if (!b)
                    {
                        return true;
                    }

                    GlobalData.Config.UILang = tag;
                    GlobalData.Save();
                    TranslationSource.Instance.Language = tag;
                    ((MainWindowViewModel)(DataContext)).SetFlowDirection();
                    return true;
                });
            }
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (GlobalData.Config.IsShowNotifyIcon)
            {
                if (GlobalData.Config.IsFirstRun)
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
                        GlobalData.Config.IsFirstRun = false;
                        GlobalData.Save();
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

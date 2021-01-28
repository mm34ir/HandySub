using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using HandyControl.Tools;
using HandySub.Language;
using HandySub.ViewModels;

namespace HandySub.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Change Theme and Language

        private void MenuSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem button && button.Tag is ApplicationTheme tag)
            {
                if (tag.Equals(GlobalDataHelper<AppConfig>.Config.Theme)) return;

                GlobalDataHelper<AppConfig>.Config.Theme = tag;
                GlobalDataHelper<AppConfig>.Save();
                ((App) Application.Current).UpdateSkin(tag);
            }
        }

        #endregion

        private void MenuLanguage_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem button && button.Tag is string tag)
            {
                if (tag.Equals(GlobalDataHelper<AppConfig>.Config.UILang)) return;

                Growl.AskGlobal(Lang.ResourceManager.GetString("ChangeLanguage"), b =>
                {
                    if (!b) return true;

                    GlobalDataHelper<AppConfig>.Config.UILang = tag;
                    GlobalDataHelper<AppConfig>.Save();
                    LocalizationManager.Instance.CurrentCulture = new CultureInfo(tag);
                    ConfigHelper.Instance.SetLang(GlobalDataHelper<AppConfig>.Config.UILang);
                    if (SettingsViewModel.Instance != null) SettingsViewModel.Instance.LoadSubtitleLanguage();

                    if (SubsceneViewModel.Instance != null) SubsceneViewModel.Instance.LoadLanguage();

                    return true;
                });
            }
        }
    }
}
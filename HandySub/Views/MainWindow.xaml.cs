using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                ((App) Application.Current).UpdateTheme(tag);
            }
            else if (e.OriginalSource is MenuItem btn && (string) btn.Tag is "Accent")
            {
                var picker = SingleOpenHelper.CreateControl<ColorPicker>();
                var window = new PopupWindow
                {
                    PopupElement = picker,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    MinWidth = 0,
                    MinHeight = 0,
                    Title = Lang.Accent
                };
                var brush = GlobalDataHelper<AppConfig>.Config.Accent;
                if (brush.GetType() == typeof(LinearGradientBrush))
                {
                    var lbrush = (LinearGradientBrush) brush;
                    picker.SelectedBrush = new SolidColorBrush(lbrush.GradientStops[1].Color);
                }
                else
                {
                    Color color = ((SolidColorBrush) brush).Color;
                    picker.SelectedBrush = new SolidColorBrush(color);
                }

                picker.SelectedColorChanged += delegate
                {
                    ((App) Application.Current).UpdateAccentColor(picker.SelectedBrush);
                    GlobalDataHelper<AppConfig>.Config.Accent = picker.SelectedBrush;
                    GlobalDataHelper<AppConfig>.Save();
                    window.Close();
                };
                picker.Canceled += delegate { window.Close(); };
                window.Show();
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
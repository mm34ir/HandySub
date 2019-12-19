using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public HorizontalAlignment ToggleAlignment { get; set; }
        public Settings()
        {
            InitializeComponent();
            DataContext = this;
            loadSettings();
            setAlignment();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    txtBrowse.Text = dialog.SelectedPath + @"\";
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmb = sender as HandyControl.Controls.ComboBox;
            ComboBoxItem typeItem = (ComboBoxItem)cmb.SelectedItem;
            InIHelper.AddValue(SettingsKey.SubtitleLanguage, typeItem.Content.ToString());
        }

        private void autoDownload_Checked(object sender, RoutedEventArgs e)
        {
            if (autoDownload.IsChecked.Value)
            {
                InIHelper.AddValue(SettingsKey.AutoDownload, "true");
            }
            else
            {
                InIHelper.AddValue(SettingsKey.AutoDownload, "false");
            }
        }

        private void selectTab_Checked(object sender, RoutedEventArgs e)
        {
            if (selectTab.IsChecked.Value)
            {
                InIHelper.AddValue(SettingsKey.SelectTab, "true");
            }
            else
            {
                InIHelper.AddValue(SettingsKey.SelectTab, "false");
            }
        }

        private void loadSettings()
        {
            var getLocation = InIHelper.ReadValue(SettingsKey.Location);
            if (string.IsNullOrEmpty(getLocation))
            {
                getLocation = string.Empty;
            }
            txtBrowse.Text = getLocation;

            var getSelectTab = InIHelper.ReadValue(SettingsKey.SelectTab);
            if (string.IsNullOrEmpty(getSelectTab))
            {
                getSelectTab = "false";
            }
            selectTab.IsChecked = Convert.ToBoolean(getSelectTab);

            var getAutoDownload = InIHelper.ReadValue(SettingsKey.AutoDownload);
            if (string.IsNullOrEmpty(getAutoDownload))
            {
                getAutoDownload = "false";
            }
            autoDownload.IsChecked = Convert.ToBoolean(getAutoDownload);
        }
        private void setAlignment()
        {
            var getLanguage = InIHelper.ReadValue(SettingsKey.Language);
            if (!string.IsNullOrEmpty(getLanguage) || getLanguage.Equals("fa"))
            {
                ToggleAlignment = HorizontalAlignment.Left;
            }
            else
            {
                ToggleAlignment = HorizontalAlignment.Right;

            }

        }
    }
}

using HandyControl.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

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
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
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
            HandyControl.Controls.ComboBox cmb = sender as HandyControl.Controls.ComboBox;
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
            string getLocation = InIHelper.ReadValue(SettingsKey.Location);
            if (string.IsNullOrEmpty(getLocation))
            {
                getLocation = string.Empty;
            }
            txtBrowse.Text = getLocation;

            string getSelectTab = InIHelper.ReadValue(SettingsKey.SelectTab);
            if (string.IsNullOrEmpty(getSelectTab))
            {
                getSelectTab = "false";
            }
            selectTab.IsChecked = Convert.ToBoolean(getSelectTab);

            string getAutoDownload = InIHelper.ReadValue(SettingsKey.AutoDownload);
            if (string.IsNullOrEmpty(getAutoDownload))
            {
                getAutoDownload = "false";
            }
            autoDownload.IsChecked = Convert.ToBoolean(getAutoDownload);
        }
        private void setAlignment()
        {
            string getLanguage = InIHelper.ReadValue(SettingsKey.Language);
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

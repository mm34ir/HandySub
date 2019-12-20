using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
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
                    GlobalData.Config.StoreLocation = txtBrowse.Text;
                    GlobalData.Save();
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandyControl.Controls.ComboBox cmb = sender as HandyControl.Controls.ComboBox;
            ComboBoxItem typeItem = (ComboBoxItem)cmb.SelectedItem;
            GlobalData.Config.SubtitleLang = typeItem.Content.ToString();
            GlobalData.Save();
        }

        private void ServerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandyControl.Controls.ComboBox cmb = sender as HandyControl.Controls.ComboBox;
            ComboBoxItem typeItem = (ComboBoxItem)cmb.SelectedItem;
            GlobalData.Config.ServerUrl = typeItem.Content.ToString();
            GlobalData.Save();
        }

        private void autoDownload_Checked(object sender, RoutedEventArgs e)
        {
            GlobalData.Config.IsAutoDownloadSubtitle = autoDownload.IsChecked.Value;
            GlobalData.Save();
        }

        private void selectTab_Checked(object sender, RoutedEventArgs e)
        {
            GlobalData.Config.IsAutoSelectOpenedTab = selectTab.IsChecked.Value;
            GlobalData.Save();
        }

        private void loadSettings()
        {
            txtBrowse.Text = GlobalData.Config.StoreLocation;

            selectTab.IsChecked = GlobalData.Config.IsAutoSelectOpenedTab;

            autoDownload.IsChecked = GlobalData.Config.IsAutoDownloadSubtitle;

            contextMenuFile.IsChecked = GlobalData.Config.IsContextMenuFile;

            contextMenuFolder.IsChecked = GlobalData.Config.IsContextMenuFolder;

        }
        private void setAlignment()
        {
            if (GlobalData.Config.UILang.Equals("en"))
            {
                autoDownload.HorizontalAlignment = HorizontalAlignment.Left;
                selectTab.HorizontalAlignment = HorizontalAlignment.Left;
                contextMenuFile.HorizontalAlignment = HorizontalAlignment.Left;
                contextMenuFolder.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                autoDownload.HorizontalAlignment = HorizontalAlignment.Left;
                selectTab.HorizontalAlignment = HorizontalAlignment.Left;
                contextMenuFile.HorizontalAlignment = HorizontalAlignment.Left;
                contextMenuFolder.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        private void contextMenuFile_Checked(object sender, RoutedEventArgs e)
        {
            GlobalData.Config.IsContextMenuFile = contextMenuFile.IsChecked.Value;
            GlobalData.Save();
            RegisterContextMenu(false, !contextMenuFile.IsChecked.Value);
        }

        private void contextMenuFolder_Checked(object sender, RoutedEventArgs e)
        {
            GlobalData.Config.IsContextMenuFolder = contextMenuFolder.IsChecked.Value;
            GlobalData.Save();
            RegisterContextMenu(true, !contextMenuFolder.IsChecked.Value);
        }

        private void RegisterContextMenu(bool IsFolder, bool IsDelete = false)
        {
            if (IsDelete)
            {
                if (IsFolder)
                {
                    RegistryKey regFolderKeyOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\directory\shell\", true);
                    regFolderKeyOpen.DeleteSubKeyTree("Get Subtitle");
                }
                else
                {
                    RegistryKey regFileKeyOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\*\shell\", true);
                    regFileKeyOpen.DeleteSubKeyTree("Get Subtitle");
                }
            }
            else
            {
                if (IsFolder)
                {
                    RegistryKey regFileOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle\command\", true);
                    if (regFileOpen == null)
                    {
                        RegistryKey regFolderKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\directory\shell\Get Subtitle\command\");
                        regFolderKey.SetValue("", $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" \"%1\"");
                    }
                }
                else
                {
                    RegistryKey regFolderOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\directory\shell\Get Subtitle\command\", true);
                    if (regFolderOpen == null)
                    {
                        RegistryKey regFileKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle\command\");
                        regFileKey.SetValue("", $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" \"%1\"");
                    }
                }
            }
        }
    }
}

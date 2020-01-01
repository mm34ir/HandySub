using HandyControl.Controls;
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
            InitSettings();
            setAlignment();
        }

        /// <summary>
        /// Get settings value from config file
        /// </summary>
        private void InitSettings()
        {
            txtBrowse.Text = GlobalData.Config.StoreLocation;

            autoDownload.IsChecked = GlobalData.Config.IsAutoDownloadSubtitle;
            selectTab.IsChecked = GlobalData.Config.IsAutoSelectOpenedTab;
            tabIsDraggable.IsChecked = GlobalData.Config.IsDraggableTab;
            contextMenuFile.IsChecked = GlobalData.Config.IsContextMenuFile;
            contextMenuFolder.IsChecked = GlobalData.Config.IsContextMenuFolder;

            TitleElement.SetTitle(cmbSubLang, string.Format(Properties.Langs.Lang.SubtitleLanguage, GlobalData.Config.SubtitleLang));
            TitleElement.SetTitle(cmbSubServer, string.Format(Properties.Langs.Lang.Server, GlobalData.Config.ServerUrl));

        }

        /// <summary>
        /// Set Alignment for ToggleButton Base on Language
        /// </summary>
        private void setAlignment()
        {
            if (GlobalData.Config.UILang.Equals("en"))
            {
                autoDownload.HorizontalAlignment = HorizontalAlignment.Left;
                selectTab.HorizontalAlignment = HorizontalAlignment.Left;
                tabIsDraggable.HorizontalAlignment = HorizontalAlignment.Left;
                contextMenuFile.HorizontalAlignment = HorizontalAlignment.Left;
                contextMenuFolder.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                autoDownload.HorizontalAlignment = HorizontalAlignment.Left;
                selectTab.HorizontalAlignment = HorizontalAlignment.Left;
                tabIsDraggable.HorizontalAlignment = HorizontalAlignment.Left;
                contextMenuFile.HorizontalAlignment = HorizontalAlignment.Left;
                contextMenuFolder.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        /// <summary>
        /// Save Download Path to Config File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Save Subtitle Language to Config File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)cmbSubLang.SelectedItem;
            if (!typeItem.Content.ToString().Equals(GlobalData.Config.SubtitleLang))
            {
                GlobalData.Config.SubtitleLang = typeItem.Content.ToString();
                GlobalData.Save();
                TitleElement.SetTitle(cmbSubLang, string.Format(Properties.Langs.Lang.SubtitleLanguage, GlobalData.Config.SubtitleLang));
            }
        }

        /// <summary>
        /// Save Subtitle Server to Config File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem typeItem = (ComboBoxItem)cmbSubServer.SelectedItem;
            if (!typeItem.Content.ToString().Equals(GlobalData.Config.ServerUrl))
            {
                GlobalData.Config.ServerUrl = typeItem.Content.ToString();
                GlobalData.Save();
                TitleElement.SetTitle(cmbSubServer, string.Format(Properties.Langs.Lang.SubtitleLanguage, GlobalData.Config.ServerUrl));
            }
        }

        private void autoDownload_Checked(object sender, RoutedEventArgs e)
        {
            if (autoDownload.IsChecked.Value != GlobalData.Config.IsAutoDownloadSubtitle)
            {
                GlobalData.Config.IsAutoDownloadSubtitle = autoDownload.IsChecked.Value;
                GlobalData.Save();
            }
        }

        private void selectTab_Checked(object sender, RoutedEventArgs e)
        {
            if (selectTab.IsChecked.Value != GlobalData.Config.IsAutoSelectOpenedTab)
            {
                GlobalData.Config.IsAutoSelectOpenedTab = selectTab.IsChecked.Value;
                GlobalData.Save();
            }
        }

        private void contextMenuFile_Checked(object sender, RoutedEventArgs e)
        {
            if (contextMenuFile.IsChecked.Value != GlobalData.Config.IsContextMenuFile)
            {
                GlobalData.Config.IsContextMenuFile = contextMenuFile.IsChecked.Value;
                GlobalData.Save();
                RegisterContextMenu(false, !contextMenuFile.IsChecked.Value);
            }
        }

        private void contextMenuFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (contextMenuFolder.IsChecked.Value != GlobalData.Config.IsContextMenuFolder)
            {
                GlobalData.Config.IsContextMenuFolder = contextMenuFolder.IsChecked.Value;
                GlobalData.Save();
                RegisterContextMenu(true, !contextMenuFolder.IsChecked.Value);
            }
        }

        private void IsDraggable_Checked(object sender, RoutedEventArgs e)
        {
            if (tabIsDraggable.IsChecked.Value != GlobalData.Config.IsDraggableTab)
            {
                GlobalData.Config.IsDraggableTab = MainWindow.mainWindow.IsDraggableTab = tabIsDraggable.IsChecked.Value;
                GlobalData.Save();
            }
        }

        /// <summary>
        /// Register Application to Windows ContextMenu
        /// </summary>
        /// <param name="IsFolder">Register to Folder ContextMenu or Files ContextMenu</param>
        /// <param name="IsDelete">UnRegister from ContextMenu</param>
        private void RegisterContextMenu(bool IsFolder, bool IsDelete = false)
        {
            try
            {
                if (IsDelete)
                {
                    if (IsFolder)
                    {
                        RegistryKey regFolderKeyOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\directory\shell\", true);
                        regFolderKeyOpen.DeleteSubKeyTree("Get Subtitle");
                        regFolderKeyOpen.DeleteSubKeyTree("Get Subtitle with World Subtitle");
                    }
                    else
                    {
                        RegistryKey regFileKeyOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\*\shell\", true);
                        regFileKeyOpen.DeleteSubKeyTree("Get Subtitle");
                        regFileKeyOpen.DeleteSubKeyTree("Get Subtitle with World Subtitle");
                    }
                }
                else
                {
                    if (IsFolder)
                    {
                        RegistryKey regFolderOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\directory\shell\Get Subtitle\command\", true);
                        if (regFolderOpen == null)
                        {
                            //Subscene
                            RegistryKey regFolderKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\directory\shell\Get Subtitle\command\");
                            regFolderKey.SetValue("", $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" \"%1\"");

                            //World Subtitle
                            RegistryKey regFolderWKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\directory\shell\Get Subtitle with World Subtitle\command\");
                            regFolderWKey.SetValue("", $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" \"%1\" \"#world#\"");
                        }
                    }
                    else
                    {
                        RegistryKey regFileOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle\command\", true);
                        if (regFileOpen == null)
                        {
                            //Subscene
                            RegistryKey regFileKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle\command\");
                            regFileKey.SetValue("", $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" \"%1\"");

                            //World Subtitle
                            RegistryKey regFileWKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle with World Subtitle\command\");
                            regFileWKey.SetValue("", $"\"{System.Reflection.Assembly.GetExecutingAssembly().Location}\" \"%1\" \"#world#\"");
                        }
                    }
                }
            }
            catch (System.ArgumentException) { }
        }
    }
}

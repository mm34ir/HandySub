using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.WinUI.UI;
using HandySub.Common;
using System.Reflection;
using SettingsUI.Helpers;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.System;
using HandySub.Models;
using Newtonsoft.Json;
using SettingsUI.Extensions;

namespace HandySub.Pages
{
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        #region INotifyProeprtyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                NotifyPropertyChanged(nameof(SelectedIndex));
            }
        }

        private ObservableCollection<string> _history = new ObservableCollection<string>();

        public ObservableCollection<string> History
        {
            get => _history;
            set
            {
                _history = value;
                NotifyPropertyChanged(nameof(History));
            }
        }
        #endregion

        private string CurrentVersion;

        private string ChangeLog = string.Empty;

        public AdvancedCollectionView HistoryACV;
        public SettingsPage()
        {
            this.InitializeComponent();
            LoadSettings();
            HistoryACV = new AdvancedCollectionView(History, true);
            HistoryACV.SortDescriptions.Add(new SortDescription(SortDirection.Ascending));
        }

        public void LoadSettings()
        {
            folderLink.Content = Helper.Settings.DefaultDownloadLocation;
            CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() + "-beta.2";

            SelectedIndex = GeneralHelper.GetThemeIndex(Helper.Settings.ApplicationTheme);
            txtLastChecked.Text = Helper.Settings.LastCheckedUpdate;

            tgDoubleClick.IsOn = Helper.Settings.IsDoubleClickEnabled;
            tgDoubleClickDownload.IsOn = Helper.Settings.IsDoubleClickDownloadEnabled;
            tgAddContextMenu.IsOn = Helper.Settings.IsAddToContextMenuEnabled;
            tgShowNotify.IsOn = Helper.Settings.IsShowNotificationEnabled;
            tgDownloadIDM.IsOn = Helper.Settings.IsIDMEnabled;
            tgUnzip.IsOn = Helper.Settings.IsAutoDeCompressEnabled;
            tgRegex.IsOn = Helper.Settings.IsDefaultRegexEnabled;
            tgSound.IsOn = Helper.Settings.IsSoundEnabled;
            tgHistory.IsOn = Helper.Settings.IsHistoryEnabled;
            txtRegex.Text = Helper.Settings.FileNameRegex;
            spatialSoundBox.IsChecked = Helper.Settings.IsSpatialSoundEnabled;
            History = Helper.Settings.SearchHistory;

            cmbSubscene.SelectedIndex = Helper.Settings.SubsceneServer.Index;
            cmbSubtitle.SelectedIndex = Helper.Settings.ShellServer.Index;
            cmbLanguage.SelectedItem = Helper.Settings.SubtitleLanguage;
            cmbQuality.SelectedItem = Helper.Settings.SubtitleQuality;
        }

        #region OpenFolder
        public async Task<string> OpenAndSelectFolder()
        {
            var main = MainWindow.Instance;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(main);

            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                return folder.Path;
            }
            return null;
        }

        private async void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var folder = await OpenAndSelectFolder();
            if (!string.IsNullOrEmpty(folder))
            {
                folderLink.Content = folder;
                Helper.Settings.DefaultDownloadLocation = folder;
            }
        }

        private async void folderLink_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(folderLink.Content.ToString()));
        }
        #endregion

        #region Theme
        public void OnThemeChanged(object sender, SelectionChangedEventArgs args)
        {
            if ((sender as ComboBox).XamlRoot == null)
                return;

            var root = (sender as ComboBox).XamlRoot.Content;
            if (_selectedIndex.Equals(0))
            {
                (root as Grid).RequestedTheme = ElementTheme.Light;
            }
            else if (_selectedIndex.Equals(1))
            {
                (root as Grid).RequestedTheme = ElementTheme.Dark;
            }
            else if (_selectedIndex.Equals(2))
            {
                (root as Grid).RequestedTheme = ElementTheme.Default;
            }
            Helper.Settings.ApplicationTheme = GeneralHelper.GetElementThemeEnum(_selectedIndex);
        }

        private async void OpenAccentColor_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:personalization-colors"));
        }

        #endregion

        #region ToggleSwitch
        private void tgDoubleClick_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.Settings.IsDoubleClickEnabled = tgDoubleClick.IsOn;
        }
        private void tgDoubleClickDownload_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.Settings.IsDoubleClickDownloadEnabled = tgDoubleClickDownload.IsOn;
        }

        private void tgDownloadIDM_Toggled(object sender, RoutedEventArgs e)
        {
            if (!Helper.IsIDMExist().IsExist)
            {
                infoIDM.IsOpen = true;
                tgDownloadIDM.IsOn = false;
            }
            Helper.Settings.IsIDMEnabled = tgDownloadIDM.IsOn;
        }

        private void tgShowNotify_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.Settings.IsShowNotificationEnabled = tgShowNotify.IsOn;
        }

        private void tgUnzip_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.Settings.IsAutoDeCompressEnabled = tgUnzip.IsOn;
        }

        private void tgAddContextMenu_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.Settings.IsAddToContextMenuEnabled = tgAddContextMenu.IsOn;
        }

        private void tgRegex_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.Settings.IsDefaultRegexEnabled = tgRegex.IsOn;
            if (tgRegex.IsOn)
            {
                if (txtRegex == null)
                {
                    return;
                }
                if (Constants.FileNameRegex != Helper.Settings.FileNameRegex)
                {
                    Helper.Settings.FileNameRegex = Constants.FileNameRegex;
                    txtRegex.Text = Constants.FileNameRegex;
                }
            }
        }

        private void tgHistory_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.Settings.IsHistoryEnabled = tgHistory.IsOn;
        }
        #endregion

        #region Sound
        private void tgSound_Toggled(object sender, RoutedEventArgs e)
        {
            if (tgSound.IsOn == true)
            {
                spatialSoundBox.IsEnabled = true;
                ElementSoundPlayer.State = ElementSoundPlayerState.On;
                Helper.Settings.IsSoundEnabled = true;
            }
            else
            {
                spatialSoundBox.IsEnabled = false;
                spatialSoundBox.IsChecked = false;

                ElementSoundPlayer.State = ElementSoundPlayerState.Off;
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
                Helper.Settings.IsSoundEnabled = false;
            }
        }
        private void spatialSoundBox_Checked(object sender, RoutedEventArgs e)
        {
            if (tgSound.IsOn == true)
            {
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On;
                Helper.Settings.IsSpatialSoundEnabled = true;
            }
        }
        private void spatialSoundBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tgSound.IsOn == true)
            {
                ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
                Helper.Settings.IsSpatialSoundEnabled = false;
            }
        }
        #endregion

        #region ComboBox
        private void cmbSubtitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ServerModel)cmbSubtitle.SelectedItem;
            if (mode != Helper.Settings.ShellServer)
            {
                Helper.Settings.ShellServer = mode;
            }
        }

        private void cmbSubscene_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ServerModel)cmbSubscene.SelectedItem;
            if (mode != Helper.Settings.SubsceneServer)
            {
                Helper.Settings.SubsceneServer = mode;
            }
        }

        private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cmbLanguage.SelectedItem as string;
            if (item != Helper.Settings.SubtitleLanguage)
            {
                Helper.Settings.SubtitleLanguage = item;
            }
        }

        private void cmbQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cmbQuality.SelectedItem as string;
            if (item != Helper.Settings.SubtitleQuality)
            {
                Helper.Settings.SubtitleQuality = item;
            }
        }
        #endregion

        #region History
        private void btnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            History.Clear();
            Helper.Settings.SearchHistory = new ObservableCollection<string>();
            infoClear.IsOpen = true;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            History.Remove((string)historyList.SelectedItem);
            Helper.Settings.SearchHistory = History;
        }

        #endregion

        #region Backup
        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            var folder = await OpenAndSelectFolder();
            if (!string.IsNullOrEmpty(folder))
            {
                var json = JsonConvert.SerializeObject(Helper.Settings, Formatting.Indented);
                await File.WriteAllTextAsync(@$"{folder}\HandySub Settings-{DateTime.Now:yyyy-MM-dd HH-mm-ss}.json", json);
                File.Copy(Constants.FavoritePath, @$"{folder}\HandySub Favorite-{DateTime.Now:yyyy-MM-dd HH-mm-ss}.json", true);
            }
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Import Settings and Favorite List",
                Content = "Please select the folder for the exported files. Note that this will overwrite all your current settings and favorite list Also note that you should make a backup before doing this, as your settings may be changed and not available in the new version, in this case all your settings will be lost.",
                PrimaryButtonText = "Ok",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot
            };
            var result = await dialog.ShowAsyncQueue();
            if (result == ContentDialogResult.Primary)
            {
                var folder = await OpenAndSelectFolder();
                if (!string.IsNullOrEmpty(folder))
                {
                    var files = Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories);
                    foreach (var item in files)
                    {
                        if (Path.GetFileNameWithoutExtension(item).Contains("HandySub Settings"))
                        {
                            var json = await File.ReadAllTextAsync(item);
                            var handySubConfig = JsonConvert.DeserializeObject<HandySubConfig>(json);
                            Helper.SetImportedSettings(handySubConfig);
                            LoadSettings();
                        }
                        else if (Path.GetFileNameWithoutExtension(item).Contains("HandySub Favorite"))
                        {
                            File.Copy(item, Constants.FavoritePath, true);
                        }
                    }
                }
            }
        }
        #endregion

        #region Check for Update
        private async void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (GeneralHelper.IsNetworkAvailable())
            {
                txtLastChecked.Text = DateTime.Now.ToShortDateString();
                Helper.Settings.LastCheckedUpdate = DateTime.Now.ToShortDateString();

                downloadPanel.Children.Clear();
                updateErrorInfo.IsOpen = false;
                updateDownloadInfo.IsOpen = false;
                updateInfo.IsOpen = false;
                prgUpdate.IsActive = true;
                txtUpdate.Visibility = Visibility.Visible;
                try
                {
                    var update = await UpdateHelper.CheckUpdateAsync("ghost1372", "handysub");
                    if (update.IsExistNewVersion)
                    {
                        txtReleaseNote.Visibility = Visibility.Visible;
                        ChangeLog = update.Changelog;
                        updateDownloadInfo.Message = $"We found a new Version {update.TagName} Created at {update.CreatedAt} and Published at {update.PublishedAt}";
                        foreach (var item in update.Assets)
                        {
                            var btn = new Button
                            {
                                Content = $"Download {Path.GetFileName(item.Url).Replace("HandySub.Package._", "")}",
                                MinWidth = 300,
                                Margin = new Thickness(10)
                            };

                            btn.Click += async (s, e) =>
                            {
                                await Launcher.LaunchUriAsync(new Uri(item.Url));
                            };

                            downloadPanel.Children.Add(btn);
                        }

                        updateDownloadInfo.IsOpen = true;
                    }
                    else
                    {
                        updateInfo.IsOpen = true;
                    }
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }

                prgUpdate.IsActive = false;
                txtUpdate.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowError(Constants.InternetIsNotAvailable, Constants.InternetIsNotAvailableTitle);
            }
        }

        public void ShowError(string message, string title = null)
        {
            updateErrorInfo.Title = title;
            updateErrorInfo.Message = message;
            updateErrorInfo.IsOpen = true;
        }

        private async void txtReleaseNote_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Release Notes",
                //Content = new ScrollViewer { Content = new MarkdownTextBlock { Text = ChangeLog }, Margin = new Thickness(10) },
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = Content.XamlRoot
            };

            await dialog.ShowAsyncQueue();
        }

        #endregion

        private void txtRegex_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtRegex.Text))
            {
                Helper.Settings.FileNameRegex = txtRegex.Text;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            History = Helper.Settings.SearchHistory;
        }
    }
}

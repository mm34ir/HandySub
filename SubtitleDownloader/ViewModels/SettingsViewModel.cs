using HandyControl.Controls;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using SubtitleDownloader.Data;
using SubtitleDownloader.Language;
using SubtitleDownloader.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;

namespace SubtitleDownloader.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        internal static SettingsViewModel Instance;
        #region Property

        private string _DefaultSubLang;
        public string DefaultSubLang
        {
            get => LocalizationManager.Instance.Localize(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LocalizeCode).ToString();
            set => SetProperty(ref _DefaultSubLang, value);
        }

        private string _DefaultSubServer;
        public string DefaultSubServer
        {
            get => GlobalDataHelper<AppConfig>.Config.ServerUrl;
            set => SetProperty(ref _DefaultSubServer, value);
        }

        private ObservableCollection<LanguageModel> _languageItems = new ObservableCollection<LanguageModel>();
        public ObservableCollection<LanguageModel> LanguageItems
        {
            get => _languageItems;
            set => SetProperty(ref _languageItems, value);
        }

        private string _CurrentStoreLocation;
        public string CurrentStoreLocation
        {
            get => _CurrentStoreLocation;
            set => SetProperty(ref _CurrentStoreLocation, value);
        }

        private string _currentServer;
        public string CurrentServer
        {
            get => _currentServer;
            set => SetProperty(ref _currentServer, value);
        }

        private string _currentLanguage;
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set => SetProperty(ref _currentLanguage, value);
        }

        private bool _getIsCheckedAutoDownload;
        public bool GetIsCheckedAutoDownload { get => _getIsCheckedAutoDownload; set => SetProperty(ref _getIsCheckedAutoDownload, value); }

        private bool _getIsCheckedShowNotification;
        public bool GetIsCheckedShowNotification { get => _getIsCheckedShowNotification; set => SetProperty(ref _getIsCheckedShowNotification, value); }


        private bool _getIsCheckedFileContextMenu;
        public bool GetIsCheckedFileContextMenu { get => _getIsCheckedFileContextMenu; set => SetProperty(ref _getIsCheckedFileContextMenu, value); }

        private bool _getIsCheckedFolderContextMenu;
        public bool GetIsCheckedFolderContextMenu { get => _getIsCheckedFolderContextMenu; set => SetProperty(ref _getIsCheckedFolderContextMenu, value); }

        private bool _getIsCheckedShowNotifyIcon;
        public bool GetIsCheckedShowNotifyIcon { get => _getIsCheckedShowNotifyIcon; set => SetProperty(ref _getIsCheckedShowNotifyIcon, value); }

        private bool _getIsCheckedIDM;
        public bool GetIsCheckedIDM { get => _getIsCheckedIDM; set => SetProperty(ref _getIsCheckedIDM, value); }
        #endregion
        #region Command
        public DelegateCommand SelectFolderCommand { get; private set; }
        public DelegateCommand AddPluginCommand { get; private set; }

        public DelegateCommand<object> AutoDownloadCommand { get; private set; }
        public DelegateCommand<object> ShowNotificationCommand { get; private set; }
        public DelegateCommand<object> AddToFileContextMenuCommand { get; private set; }
        public DelegateCommand<object> AddToFolderContextMenuCommand { get; private set; }
        public DelegateCommand<object> IsShowNotifyIconCommand { get; private set; }
        public DelegateCommand<object> IDMCommand { get; private set; }

        public DelegateCommand<SelectionChangedEventArgs> SubtitleLanguageCommand { get; private set; }
        public DelegateCommand<SelectionChangedEventArgs> ServerChangedCommand { get; private set; }
        #endregion

        public ICollectionView ItemsView => CollectionViewSource.GetDefaultView(LanguageItems);


        public SettingsViewModel()
        {
            Instance = this;
            SelectFolderCommand = new DelegateCommand(SelectFolder);
            AddPluginCommand = new DelegateCommand(AddNewPlugin);
            AutoDownloadCommand = new DelegateCommand<object>(AutoDownload);
            ShowNotificationCommand = new DelegateCommand<object>(ShowNotification);
            AddToFileContextMenuCommand = new DelegateCommand<object>(AddToFileContextMenu);
            AddToFolderContextMenuCommand = new DelegateCommand<object>(AddToFolderContextMenu);
            IsShowNotifyIconCommand = new DelegateCommand<object>(IsShowNotifyIcon);
            IDMCommand = new DelegateCommand<object>(IDM);

            SubtitleLanguageCommand = new DelegateCommand<SelectionChangedEventArgs>(SubtitleLanguageChanged);
            ServerChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(ServerChanged);
            ItemsView.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));

            InitSettings();
        }


        private void AddNewPlugin()
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Module files (*.dll)|*.dll"
            };
            if (openFile.ShowDialog().Value)
            {
                File.Copy(openFile.FileName, $@".\modules\{Path.GetFileName(openFile.FileName)}", true);
                HandyControl.Controls.MessageBox.Success(LocalizationManager.Instance.Localize("AddedPlugin").ToString());
            }
        }

        private void InitSettings()
        {
            CurrentStoreLocation = GlobalDataHelper<AppConfig>.Config.StoreLocation;

            GetIsCheckedAutoDownload = GlobalDataHelper<AppConfig>.Config.IsAutoDownloadSubtitle;
            GetIsCheckedFileContextMenu = GlobalDataHelper<AppConfig>.Config.IsContextMenuFile;
            GetIsCheckedFolderContextMenu = GlobalDataHelper<AppConfig>.Config.IsContextMenuFolder;
            GetIsCheckedShowNotification = GlobalDataHelper<AppConfig>.Config.IsShowNotification;
            GetIsCheckedShowNotifyIcon = GlobalDataHelper<AppConfig>.Config.IsShowNotifyIcon;
            GetIsCheckedIDM = GlobalDataHelper<AppConfig>.Config.IsIDMEngine;

            LoadSubtitleLanguage();
        }

        public void LoadSubtitleLanguage()
        {
            LanguageItems.Clear();
            DefaultSubLang = CurrentLanguage = LocalizationManager.Instance.Localize(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LocalizeCode).ToString();
            CurrentServer = string.Format(Lang.ResourceManager.GetString("SubServer"), GlobalDataHelper<AppConfig>.Config.ServerUrl);
            LanguageItems.AddRange(SupportedLanguages.LoadSubtitleLanguage());
        }

        private void ServerChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            if (e.AddedItems[0] is ComboBoxItem item)
            {
                if (!item.Content.ToString().Equals(GlobalDataHelper<AppConfig>.Config.ServerUrl))
                {
                    GlobalDataHelper<AppConfig>.Config.ServerUrl = item.Content.ToString();
                    GlobalDataHelper<AppConfig>.Save();
                    CurrentServer = string.Format(Lang.ResourceManager.GetString("SubServer"), GlobalDataHelper<AppConfig>.Config.ServerUrl);
                }
            }
        }

        private void SubtitleLanguageChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            if (e.AddedItems[0] is LanguageModel item)
            {
                if (!item.Equals(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage))
                {
                    GlobalDataHelper<AppConfig>.Config.SubtitleLanguage = item;
                    GlobalDataHelper<AppConfig>.Save();
                    CurrentLanguage = GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.DisplayName;
                }
            }
        }

        #region ToggleButton
        private void IDM(object isChecked)
        {
            if ((bool)isChecked != GlobalDataHelper<AppConfig>.Config.IsIDMEngine)
            {
                GlobalDataHelper<AppConfig>.Config.IsIDMEngine = (bool)isChecked;
                GlobalDataHelper<AppConfig>.Save();
            }
        }

        private void ShowNotification(object isChecked)
        {
            if ((bool)isChecked != GlobalDataHelper<AppConfig>.Config.IsShowNotification)
            {
                GlobalDataHelper<AppConfig>.Config.IsShowNotification = (bool)isChecked;
                GlobalDataHelper<AppConfig>.Save();
            }
        }

        private void IsShowNotifyIcon(object isChecked)
        {
            if ((bool)isChecked != GlobalDataHelper<AppConfig>.Config.IsShowNotifyIcon)
            {
                GlobalDataHelper<AppConfig>.Config.IsShowNotifyIcon = (bool)isChecked;
                GlobalDataHelper<AppConfig>.Save();
            }
        }

        private void AddToFolderContextMenu(object isChecked)
        {
            if ((bool)isChecked != GlobalDataHelper<AppConfig>.Config.IsContextMenuFolder)
            {
                GlobalDataHelper<AppConfig>.Config.IsContextMenuFolder = (bool)isChecked;
                GlobalDataHelper<AppConfig>.Save();
                RegisterContextMenu(true, !(bool)isChecked);
            }
        }

        private void AddToFileContextMenu(object isChecked)
        {
            if ((bool)isChecked != GlobalDataHelper<AppConfig>.Config.IsContextMenuFile)
            {
                GlobalDataHelper<AppConfig>.Config.IsContextMenuFile = (bool)isChecked;
                GlobalDataHelper<AppConfig>.Save();
                RegisterContextMenu(false, !(bool)isChecked);
            }
        }

        private void AutoDownload(object isChecked)
        {
            if ((bool)isChecked != GlobalDataHelper<AppConfig>.Config.IsAutoDownloadSubtitle)
            {
                GlobalDataHelper<AppConfig>.Config.IsAutoDownloadSubtitle = (bool)isChecked;
                GlobalDataHelper<AppConfig>.Save();
            }
        }
        #endregion

        private void SelectFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    CurrentStoreLocation = dialog.SelectedPath + @"\";
                    GlobalDataHelper<AppConfig>.Config.StoreLocation = CurrentStoreLocation;
                    GlobalDataHelper<AppConfig>.Save();
                }
            }
        }

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
                        RegistryKey regFolderOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\directory\shell\Get Subtitle\command\", true);
                        if (regFolderOpen == null)
                        {
                            //Subscene
                            RegistryKey regFolderKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\directory\shell\Get Subtitle\command\");
                            regFolderKey.SetValue("", $"\"{System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName}\" \"%1\"");
                        }
                    }
                    else
                    {
                        RegistryKey regFileOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle\command\", true);
                        if (regFileOpen == null)
                        {
                            //Subscene
                            RegistryKey regFileKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle\command\");
                            regFileKey.SetValue("", $"\"{System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName}\" \"%1\"");
                        }
                    }
                }
            }
            catch (ArgumentException) { }
            catch (NullReferenceException) { }
        }
    }
}

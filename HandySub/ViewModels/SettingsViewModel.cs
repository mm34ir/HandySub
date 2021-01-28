using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using HandyControl.Controls;
using HandySub.Data;
using HandySub.Language;
using HandySub.Model;
using Microsoft.Win32;
using ModernWpf.Controls;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace HandySub.ViewModels
{
    public class SettingsViewModel : BindableBase, IRegionMemberLifetime
    {
        internal static SettingsViewModel Instance;

        public SettingsViewModel()
        {
            MainWindowViewModel.Instance.IsBackEnabled = false;

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CheckUpdateCommand = new DelegateCommand(CheckforUpdate);
            Instance = this;
            SelectFolderCommand = new DelegateCommand(SelectFolder);
            ShowNotificationCommand = new DelegateCommand<object>(ShowNotification);
            AddToFileContextMenuCommand = new DelegateCommand<object>(AddToFileContextMenu);
            KeepAliveUICommand = new DelegateCommand<object>(KeepAliveUI);
            IDMCommand = new DelegateCommand<object>(IDM);

            SubtitleLanguageCommand = new DelegateCommand<SelectionChangedEventArgs>(SubtitleLanguageChanged);
            ServerChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(ServerChanged);
            PaneDisplayModeChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(PaneDisplayModeChanged);
            ItemsView.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));

            InitSettings();
        }

        public ICollectionView ItemsView => CollectionViewSource.GetDefaultView(LanguageItems);
        public bool KeepAlive => GlobalDataHelper<AppConfig>.Config.IsKeepAlive;

        private void InitSettings()
        {
            CurrentStoreLocation = GlobalDataHelper<AppConfig>.Config.StoreLocation;

            GetIsCheckedFileContextMenu = GlobalDataHelper<AppConfig>.Config.IsContextMenuFile;
            GetIsCheckedShowNotification = GlobalDataHelper<AppConfig>.Config.IsShowNotification;
            GetIsCheckedIDM = GlobalDataHelper<AppConfig>.Config.IsIDMEngine;
            GetIsCheckedKeepAlive = GlobalDataHelper<AppConfig>.Config.IsKeepAlive;

            PaneIndex = (int) GlobalDataHelper<AppConfig>.Config.PaneDisplayMode;
            LoadSubtitleLanguage();
        }

        public void LoadSubtitleLanguage()
        {
            LanguageItems.Clear();
            DefaultSubLang = CurrentLanguage = LocalizationManager.Instance
                .Localize(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LocalizeCode).ToString();
            CurrentServer = string.Format(Lang.ResourceManager.GetString("SubServer"),
                GlobalDataHelper<AppConfig>.Config.ServerUrl);
            LanguageItems.AddRange(SupportedLanguages.LoadSubtitleLanguage());
        }

        private void CheckforUpdate()
        {
            try
            {
                var ver = UpdateHelper.Instance.CheckUpdate("HandyOrg", "HandySub");
                var link = ver.Asset[0].browser_download_url;

                if (ver.IsExistNewVersion)
                    Growl.AskGlobal(string.Format(Lang.ResourceManager.GetString("NewVersionFound"), link), b =>
                    {
                        if (!b) return true;

                        Process.Start(link);
                        return true;
                    });
                else
                    Growl.InfoGlobal(Lang.ResourceManager.GetString("LatestVersion"));
            }
            catch (Exception)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("NoNewVersion"));
            }
        }

        private void PaneDisplayModeChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            if (e.AddedItems[0] is NavigationViewPaneDisplayMode item)
                if (!item.Equals(GlobalDataHelper<AppConfig>.Config.PaneDisplayMode))
                {
                    GlobalDataHelper<AppConfig>.Config.PaneDisplayMode = item;
                    GlobalDataHelper<AppConfig>.Save();
                    GlobalDataHelper<AppConfig>.Init($"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json");
                    MainWindowViewModel.Instance.PaneDisplayMode = item;
                }
        }

        private void ServerChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            if (e.AddedItems[0] is ComboBoxItem item)
                if (!item.Content.ToString().Equals(GlobalDataHelper<AppConfig>.Config.ServerUrl))
                {
                    GlobalDataHelper<AppConfig>.Config.ServerUrl = item.Content.ToString();
                    GlobalDataHelper<AppConfig>.Save();
                    CurrentServer = string.Format(Lang.ResourceManager.GetString("SubServer"),
                        GlobalDataHelper<AppConfig>.Config.ServerUrl);
                    GlobalDataHelper<AppConfig>.Init($"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json");
                }
        }

        private void SubtitleLanguageChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            if (e.AddedItems[0] is LanguageModel item)
                if (!item.Equals(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage))
                {
                    GlobalDataHelper<AppConfig>.Config.SubtitleLanguage = item;
                    GlobalDataHelper<AppConfig>.Save();
                    CurrentLanguage = GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.DisplayName;
                }
        }

        private void SelectFolder()
        {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                CurrentStoreLocation = dialog.SelectedPath + @"\";
                GlobalDataHelper<AppConfig>.Config.StoreLocation = CurrentStoreLocation;
                GlobalDataHelper<AppConfig>.Save();
            }
        }

        private void RegisterContextMenu(bool IsDelete = false)
        {
            try
            {
                if (IsDelete)
                {
                    var regFolderKeyOpen =
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\directory\shell\", true);
                    var regFileKeyOpen = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\*\shell\", true);

                    regFolderKeyOpen?.DeleteSubKeyTree("Get Subtitle");
                    regFileKeyOpen?.DeleteSubKeyTree("Get Subtitle");
                }
                else
                {
                    var regFileOpen =
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle\command\", true);
                    if (regFileOpen == null)
                    {
                        var regFileKey =
                            Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Classes\*\shell\Get Subtitle\command\");
                        regFileKey.SetValue("",
                            $"\"{Process.GetCurrentProcess().MainModule.FileName}\" \"%1\"");
                    }
                }
            }
            catch (ArgumentException)
            {
            }
            catch (NullReferenceException)
            {
            }
        }

        #region Property

        private string _version;

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        private string _DefaultSubLang;

        public string DefaultSubLang
        {
            get => LocalizationManager.Instance
                .Localize(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LocalizeCode).ToString();
            set => SetProperty(ref _DefaultSubLang, value);
        }

        private string _DefaultSubServer;

        public string DefaultSubServer
        {
            get => GlobalDataHelper<AppConfig>.Config.ServerUrl;
            set => SetProperty(ref _DefaultSubServer, value);
        }

        private ObservableCollection<LanguageModel> _languageItems = new();

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

        private int _PaneIndex;

        public int PaneIndex
        {
            get => _PaneIndex;
            set => SetProperty(ref _PaneIndex, value);
        }

        private bool _getIsCheckedShowNotification;

        public bool GetIsCheckedShowNotification
        {
            get => _getIsCheckedShowNotification;
            set => SetProperty(ref _getIsCheckedShowNotification, value);
        }


        private bool _getIsCheckedFileContextMenu;

        public bool GetIsCheckedFileContextMenu
        {
            get => _getIsCheckedFileContextMenu;
            set => SetProperty(ref _getIsCheckedFileContextMenu, value);
        }

        private bool _getIsCheckedIDM;

        public bool GetIsCheckedIDM
        {
            get => _getIsCheckedIDM;
            set => SetProperty(ref _getIsCheckedIDM, value);
        }

        private bool _getIsCheckedKeepAlive;

        public bool GetIsCheckedKeepAlive
        {
            get => _getIsCheckedKeepAlive;
            set => SetProperty(ref _getIsCheckedKeepAlive, value);
        }

        #endregion

        #region Command

        public DelegateCommand CheckUpdateCommand { get; }
        public DelegateCommand SelectFolderCommand { get; }
        public DelegateCommand<object> ShowNotificationCommand { get; }
        public DelegateCommand<object> AddToFileContextMenuCommand { get; }
        public DelegateCommand<object> KeepAliveUICommand { get; }
        public DelegateCommand<object> IDMCommand { get; }

        public DelegateCommand<SelectionChangedEventArgs> SubtitleLanguageCommand { get; }
        public DelegateCommand<SelectionChangedEventArgs> ServerChangedCommand { get; }
        public DelegateCommand<SelectionChangedEventArgs> PaneDisplayModeChangedCommand { get; }

        #endregion

        #region ToggleButton

        private void KeepAliveUI(object isChecked)
        {
            if ((bool) isChecked != GlobalDataHelper<AppConfig>.Config.IsKeepAlive)
            {
                GlobalDataHelper<AppConfig>.Config.IsKeepAlive = (bool) isChecked;
                GlobalDataHelper<AppConfig>.Config.IsBackVisible = (bool) isChecked;
                GlobalDataHelper<AppConfig>.Save();
                GlobalDataHelper<AppConfig>.Init($"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json");
                MainWindowViewModel.Instance.IsBackVisible = !(bool) isChecked;
            }
        }

        private void IDM(object isChecked)
        {
            if ((bool) isChecked != GlobalDataHelper<AppConfig>.Config.IsIDMEngine)
            {
                GlobalDataHelper<AppConfig>.Config.IsIDMEngine = (bool) isChecked;
                GlobalDataHelper<AppConfig>.Save();
            }
        }

        private void ShowNotification(object isChecked)
        {
            if ((bool) isChecked != GlobalDataHelper<AppConfig>.Config.IsShowNotification)
            {
                GlobalDataHelper<AppConfig>.Config.IsShowNotification = (bool) isChecked;
                GlobalDataHelper<AppConfig>.Save();
            }
        }

        private void AddToFileContextMenu(object isChecked)
        {
            if ((bool) isChecked != GlobalDataHelper<AppConfig>.Config.IsContextMenuFile)
            {
                GlobalDataHelper<AppConfig>.Config.IsContextMenuFile = (bool) isChecked;
                GlobalDataHelper<AppConfig>.Save();
                RegisterContextMenu(!(bool) isChecked);
            }
        }

        #endregion
    }
}
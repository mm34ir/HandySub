using HandyControl.Controls;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using HandySub.Assets;
using HandySub.Assets.Strings;
using HandySub.Models;
using ModernWpf.Controls;
using nucs.JsonSettings;
using nucs.JsonSettings.Autosave;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class General : UserControl
    {
        ISettings Settings = JsonSettings.Load<ISettings>().EnableAutosave();
        string Version = string.Empty;
        public General()
        {
            InitializeComponent();
            GenerateServers();
            GenerateShellServers();
            LoadInitialSettings();
        }

        private void GenerateServers()
        {
            List<ServerModel> servers = new List<ServerModel>
            {
                new ServerModel{ Index = 0, Key = "Subf2m", Url = "https://subf2m.co" },
                new ServerModel{ Index = 1, Key = "Delta Leech", Url = "https://sub.deltaleech.com" },
                new ServerModel{ Index = 2, Key = "Subscene", Url = "https://subscene.com" }
            };

            cmbSubsceneServer.ItemsSource = servers;
        }

        private void GenerateShellServers()
        {
            List<ServerModel> servers = new List<ServerModel>
            {
                new ServerModel{ Index = 0, Key = Lang.ResourceManager.GetString("Subscene")},
                new ServerModel{ Index = 1, Key = Lang.ResourceManager.GetString("ESubtitle")},
                new ServerModel{ Index = 2, Key = Lang.ResourceManager.GetString("WorldSubtitle")},
                new ServerModel{ Index = 3, Key = Lang.ResourceManager.GetString("ISubtitles")}
            };

            cmbShellServer.ItemsSource = servers;
        }


        private void LoadInitialSettings()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            cmbPaneDisplay.SelectedItem = Settings.PaneDisplayMode;
            cmbSubsceneServer.SelectedIndex = Settings.SubsceneServer.Index;
            cmbShellServer.SelectedIndex = Settings.ShellServer.Index;
            MainWindow.Instance.navView.PaneDisplayMode = Settings.PaneDisplayMode;

            tgIDM.IsChecked = Settings.IsIDMEnabled;
            tgBack.IsChecked = Settings.IsBackEnabled;
            tgContext.IsChecked = Settings.IsAddToContextMenu;
            currentVersion.Text = Lang.ResourceManager.GetString("CurrentVersion").Format(Version);

            CanExecuteClearHistory();

            num.Value = Settings.MaxHistoryNumber;
            txtLocation.Text = Lang.ResourceManager.GetString("CurrentLocation").Format(Settings.StoreLocation);
        }

        private void cmbPaneDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (NavigationViewPaneDisplayMode)cmbPaneDisplay.SelectedItem;
            if (mode != Settings.PaneDisplayMode)
            {
                Settings.PaneDisplayMode = mode;
                MainWindow.Instance.navView.PaneDisplayMode = mode;
            }
        }

        private void tgIDM_Checked(object sender, RoutedEventArgs e)
        {
            var state = tgIDM.IsChecked.Value;
            if (state != Settings.IsIDMEnabled)
            {
                Settings.IsIDMEnabled = state;
            }
        }

        private void ResetAccent_Click(object sender, RoutedEventArgs e)
        {
            Settings.Accent = null;
            ((App)Application.Current).UpdateAccent(null);
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnCheck.IsEnabled = false;
                var ver = await UpdateHelper.Instance.CheckUpdateAsync("HandyOrg", "HandySub");

                if (ver.IsExistNewVersion)
                {
                    Growl.AskGlobal(Lang.ResourceManager.GetString("VersionFound"), b =>
                    {
                        if (!b)
                        {
                            return true;
                        }

                        Helper.StartProcess(ver.Asset[0].browser_download_url);
                        return true;
                    });
                }
                else
                {
                    Growl.InfoGlobal(Lang.ResourceManager.GetString("LatestVersion"));
                }

                btnCheck.IsEnabled = true;

            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal(ex.Message);
            }
        }

        private void tgContext_Checked(object sender, RoutedEventArgs e)
        {
            var state = tgContext.IsChecked.Value;
            if (state != Settings.IsAddToContextMenu)
            {
                Settings.IsAddToContextMenu = state;
                if (state)
                {
                    cmbShellServer.Visibility = Visibility.Visible;
                    ApplicationHelper.RegisterToWindowsFileShellContextMenu(Consts.ContextMenuName, Consts.ShellCommand);
                }
                else
                {
                    cmbShellServer.Visibility = Visibility.Collapsed;
                    ApplicationHelper.UnRegisterFromWindowsFileShellContextMenu(Consts.ContextMenuName);
                }
            }
        }

        private void cmbSubsceneServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ServerModel)cmbSubsceneServer.SelectedItem;
            if (mode != Settings.SubsceneServer)
            {
                Settings.SubsceneServer = mode;
            }
        }

        private bool CanExecuteClearHistory()
        {
            var _setting = JsonSettings.Load<ISettings>().EnableAutosave();

            if (_setting.History.Count > 0)
            {
                btnClear.IsEnabled = true;
                return true;
            }
            else 
            {
                btnClear.IsEnabled = false;
                return false;
            }
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            Helper.OnClearHistory();
            CanExecuteClearHistory();
        }

        private void num_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            var val = num.Value;
            if (val != Settings.MaxHistoryNumber)
            {
                Settings.MaxHistoryNumber = (int)val;
            }
        }

        private void tgBack_Checked(object sender, RoutedEventArgs e)
        {
            var state = tgBack.IsChecked.Value;
            if (state != Settings.IsBackEnabled)
            {
                Settings.IsBackEnabled = state;
                MainWindow.Instance.navView.IsBackButtonVisible = state ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;
            }
        }

        private void SubtitleStoreLocation_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtLocation.Text = Lang.ResourceManager.GetString("CurrentLocation").Format(dialog.SelectedPath);
                    Settings.StoreLocation = dialog.SelectedPath;
                }
            }
        }

        private void cmbShellServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ServerModel)cmbShellServer.SelectedItem;
            if (mode != Settings.ShellServer)
            {
                Settings.ShellServer = mode;
            }
        }
    }
}

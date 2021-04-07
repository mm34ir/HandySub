using HandyControl.Controls;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using HandySub.Assets;
using HandySub.Models;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using static HandySub.Assets.Helper;
namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class General : UserControl
    {
        string Version = string.Empty;
        public General()
        {
            InitializeComponent();
            GenerateServers();
            GenerateShellServers();
            LoadInitialSettings();

            LocalizationManager.Instance.CultureChanged += Instance_CultureChanged;
        }

        private void Instance_CultureChanged(object sender, HandyControl.Data.FunctionEventArgs<System.Globalization.CultureInfo> e)
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            currentVersion.Text = LocalizationManager.LocalizeString("CurrentVersion").Format(Version);
            txtLocation.Text = LocalizationManager.LocalizeString("CurrentLocation").Format(Settings.StoreLocation);
        }

        private void LoadInitialSettings()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            num.Value = Settings.MaxHistoryNumber;

            cmbPaneDisplay.SelectedItem = Settings.PaneDisplayMode;
            cmbSubsceneServer.SelectedIndex = Settings.SubsceneServer.Index;
            cmbShellServer.SelectedIndex = Settings.ShellServer.Index;
            MainWindow.Instance.navView.PaneDisplayMode = Settings.PaneDisplayMode;

            tgIDM.IsChecked = Settings.IsIDMEnabled;
            tgNotify.IsChecked = Settings.IsShowNotification;
            tgBack.IsChecked = Settings.IsBackEnabled;
            tgContext.IsChecked = Settings.IsAddToContextMenu;

            txtLocation.Text = LocalizationManager.LocalizeString("CurrentLocation").Format(Settings.StoreLocation);
            currentVersion.Text = LocalizationManager.LocalizeString("CurrentVersion").Format(Version);

            CanExecuteClearHistory();
        }

        #region Generate Servers
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
                new ServerModel{ Index = 0, Key = LocalizationManager.LocalizeString("Subscene")},
                new ServerModel{ Index = 1, Key = LocalizationManager.LocalizeString("ESubtitle")},
                new ServerModel{ Index = 2, Key = LocalizationManager.LocalizeString("WorldSubtitle")},
                new ServerModel{ Index = 3, Key = LocalizationManager.LocalizeString("ISubtitles")}
            };

            cmbShellServer.ItemsSource = servers;
        }

        #endregion

        #region ComboBox SelectionChanged
        private void cmbPaneDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (NavigationViewPaneDisplayMode)cmbPaneDisplay.SelectedItem;
            if (mode != Settings.PaneDisplayMode)
            {
                Settings.PaneDisplayMode = mode;
                MainWindow.Instance.navView.PaneDisplayMode = mode;
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

        private void cmbSubsceneServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ServerModel)cmbSubsceneServer.SelectedItem;
            if (mode != Settings.SubsceneServer)
            {
                Settings.SubsceneServer = mode;
            }
        }
        #endregion

        #region ToggleButton Checked/UnChecked
        private void tgIDM_Checked(object sender, RoutedEventArgs e)
        {
            var state = tgIDM.IsChecked.Value;
            if (state != Settings.IsIDMEnabled)
            {
                Settings.IsIDMEnabled = state;
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

        private void tgBack_Checked(object sender, RoutedEventArgs e)
        {
            var state = tgBack.IsChecked.Value;
            if (state != Settings.IsBackEnabled)
            {
                Settings.IsBackEnabled = state;
                MainWindow.Instance.navView.IsBackButtonVisible = state ? NavigationViewBackButtonVisible.Visible : NavigationViewBackButtonVisible.Collapsed;
            }
        }

        private void tgNotify_Checked(object sender, RoutedEventArgs e)
        {
            var state = tgNotify.IsChecked.Value;
            if (state != Settings.IsShowNotification)
            {
                Settings.IsShowNotification = state;
            }
        }
        #endregion

        #region History
        private bool CanExecuteClearHistory()
        {
            if (Settings.History.Count > 0)
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
            OnClearHistory();
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

        #endregion

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
                var ver = await UpdateHelper.CheckUpdateAsync("HandyOrg", "HandySub");

                if (ver.IsExistNewVersion)
                {
                    Growl.AskGlobal(LocalizationManager.LocalizeString("VersionFound"), b =>
                    {
                        if (!b)
                        {
                            return true;
                        }

                        StartProcess(ver.Assets[0].Url);
                        return true;
                    });
                }
                else
                {
                    Growl.InfoGlobal(LocalizationManager.LocalizeString("LatestVersion"));
                }

                btnCheck.IsEnabled = true;

            }
            catch (Exception ex)
            {
                Growl.ErrorGlobal(ex.Message);
            }
        }

        private void SubtitleStoreLocation_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtLocation.Text = LocalizationManager.LocalizeString("CurrentLocation").Format(dialog.SelectedPath);
                    Settings.StoreLocation = dialog.SelectedPath;
                }
            }
        }
    }
}

using Downloader;
using HandySub.Common;
using HandySub.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SettingsUI.Helpers;
using System;
using System.ComponentModel;
using System.IO;

namespace HandySub.UserControls
{
    public sealed partial class ESubtitleUserControl : UserControl
    {
        #region DependencyProperty
        public static readonly DependencyProperty LinkProperty =
        DependencyProperty.Register("Link", typeof(string), typeof(ESubtitleUserControl),
           new PropertyMetadata(string.Empty));

        public string Link
        {
            get { return (string)GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(ESubtitleUserControl),
            new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        #endregion

        string location = string.Empty;
        public ESubtitleUserControl()
        {
            this.InitializeComponent();
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            ESubtitleDownloadPage.Instance.CloseStatus();

            if (GeneralHelper.IsNetworkAvailable())
            {
                if (!Helper.Settings.IsIDMEnabled)
                {
                    btnDownload.IsEnabled = false;
                    ProgressStatus.IsIndeterminate = true;
                    ProgressStatus.Visibility = Visibility.Visible;
                    ProgressStatus.Value = 0;
                }

                try
                {
                    if (!string.IsNullOrEmpty(Link))
                    {
                        // if luanched from ContextMenu set location next to the movie file
                        if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
                        {
                            location = App.StartUpArguments.Path;
                        }
                        else 
                        {
                            // get location from config
                            location = Helper.Settings.DefaultDownloadLocation;

                            // get location from FolderPicker
                            if (Helper.Settings.IsAskLocationEnabled)
                            {
                                var path = await Helper.OpenAndSelectFolder();
                                if (!string.IsNullOrEmpty(path))
                                {
                                    location = path;
                                }
                            }
                        }

                        if (!Helper.Settings.IsIDMEnabled)
                        {
                            var downloader = new DownloadService();
                            downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                            downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                            await downloader.DownloadFileTaskAsync(Link, new DirectoryInfo(location));
                        }
                        else
                        {
                            ProgressStatus.Visibility = Visibility.Collapsed;
                            Helper.OpenLinkWithIDM(Link);
                        }
                    }
                }
                catch (NullReferenceException ex)
                {
                    ESubtitleDownloadPage.Instance.ShowStatus(null, ex.Message, InfoBarSeverity.Error);
                    btnDownload.IsEnabled = true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    ESubtitleDownloadPage.Instance.ShowStatus(null, ex.Message, InfoBarSeverity.Error);
                    btnDownload.IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (Exception ex)
                {
                    ESubtitleDownloadPage.Instance.ShowStatus(null, ex.Message, InfoBarSeverity.Error);
                    btnDownload.IsEnabled = true;
                }
                finally
                {
                    ProgressStatus.Visibility = Visibility.Collapsed;
                    btnDownload.IsEnabled = true;
                }
            }
            else
            {
                ESubtitleDownloadPage.Instance.ShowStatus(Constants.InternetIsNotAvailableTitle, Constants.InternetIsNotAvailable, InfoBarSeverity.Error);
            }
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ESubtitleDownloadPage.Instance.ShowStatus("Download Canceled!", null, InfoBarSeverity.Error);
                });
            }
            else if (e.Error != null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ESubtitleDownloadPage.Instance.ShowStatus(null, e.Error.Message, InfoBarSeverity.Error);
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressStatus.Visibility = Visibility.Collapsed;
                    btnOpen.Visibility = Visibility.Visible;
                    btnDownload.Visibility = Visibility.Collapsed;
                    var downloadedFileName = ((DownloadPackage)e.UserState).FileName;
                    btnOpen.Tag = downloadedFileName;
                    Helper.DeCompressAndNotification(downloadedFileName, btnDownload, Content.XamlRoot);

                });
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => {
                if (ProgressStatus.IsIndeterminate == true)
                {
                    ProgressStatus.IsIndeterminate = false;
                }
                ProgressStatus.Value = e.ProgressPercentage;
            });
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpen.Tag != null)
            {
                Helper.OpenFolderAndSelectFile(btnOpen.Tag.ToString());
            }
        }
    }
}

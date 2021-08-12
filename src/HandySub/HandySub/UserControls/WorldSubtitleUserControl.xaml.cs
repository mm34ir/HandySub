using Downloader;
using HandySub.Common;
using HandySub.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.IO;

namespace HandySub.UserControls
{
    public sealed partial class WorldSubtitleUserControl : UserControl
    {
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

        string location = string.Empty;
        public WorldSubtitleUserControl()
        {
            this.InitializeComponent();
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (btnDownload.Content.Equals("Open Folder"))
            {
                Helper.OpenFolderAndSelectFile(btnDownload.Tag.ToString());
            }
            else
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
                            location = App.StartUpArguments.Path;
                        else // get location from config
                            location = Helper.Settings.DefaultDownloadLocation;

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
                    WorldSubtitleDownloadPage.Instance.ShowInfoBar("Error", ex.Message, InfoBarSeverity.Error);
                    btnDownload.IsEnabled = true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    WorldSubtitleDownloadPage.Instance.ShowInfoBar("Error", ex.Message, InfoBarSeverity.Error);
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
                    WorldSubtitleDownloadPage.Instance.ShowInfoBar("Error", ex.Message, InfoBarSeverity.Error);
                    btnDownload.IsEnabled = true;
                }
                finally
                {
                    ProgressStatus.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    WorldSubtitleDownloadPage.Instance.ShowInfoBar("Error", "Download Canceled", InfoBarSeverity.Error);
                });
            }
            else if (e.Error != null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    WorldSubtitleDownloadPage.Instance.ShowInfoBar("Error", e.Error.Message, InfoBarSeverity.Error);
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressStatus.Visibility = Visibility.Collapsed;
                    btnDownload.Content = "Open Folder";
                    btnDownload.IsEnabled = true;
                    var downloadedFileName = ((DownloadPackage)e.UserState).FileName;
                    btnDownload.Tag = downloadedFileName;
                    Helper.DeCompressAndNotification(downloadedFileName, btnDownload, Content.XamlRoot);
                });
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => {
                if (ProgressStatus.IsIndeterminate == true)
                {
                    ProgressStatus.IsIndeterminate = false;
                }
                ProgressStatus.Value = e.ProgressPercentage;
            });
        }
    }
}

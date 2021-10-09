using Downloader;
using HandySub.Common;
using HandySub.Pages;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SettingsUI.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace HandySub.UserControls
{
    public sealed partial class ISubtitleUserControl : UserControl
    {
        #region DependencyProperty
        public static readonly DependencyProperty LinkProperty =
        DependencyProperty.Register("Link", typeof(string), typeof(ISubtitleUserControl),
           new PropertyMetadata(string.Empty));

        public string Link
        {
            get { return (string)GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(ISubtitleUserControl),
            new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TranslatorProperty =
        DependencyProperty.Register("Translator", typeof(string), typeof(ISubtitleUserControl),
            new PropertyMetadata(string.Empty));

        public string Translator
        {
            get { return (string)GetValue(TranslatorProperty); }
            set { SetValue(TranslatorProperty, value); }
        }

        public static readonly DependencyProperty SubtitleLanguageProperty =
        DependencyProperty.Register("SubtitleLanguage", typeof(string), typeof(ISubtitleUserControl),
            new PropertyMetadata(string.Empty));

        public string SubtitleLanguage
        {
            get { return (string)GetValue(SubtitleLanguageProperty); }
            set { SetValue(SubtitleLanguageProperty, value); }
        }
        #endregion

        string location = string.Empty;
        public ISubtitleUserControl()
        {
            this.InitializeComponent();
        }
        
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadSubtitle();
        }
        public async void DownloadSubtitle()
        {
            ISubtitleDownloadPage.Instance.CloseStatus();

            if (GeneralHelper.IsNetworkAvailable())
            {
                if (!Helper.Settings.IsIDMEnabled)
                {
                    DownloadButton.IsEnabled = false;
                    ProgressStatus.IsIndeterminate = true;
                    ProgressStatus.Visibility = Visibility.Visible;
                    ProgressStatus.Value = 0;
                }

                try
                {
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(Link);

                    if (doc != null)
                    {
                        var downloadLink = Constants.ISubtitleBaseUrl + doc?.DocumentNode
                            ?.SelectSingleNode("//div[@class='col-lg-16 col-md-24 col-sm-16']//a")?.Attributes["href"]
                            ?.Value;

                        if (!string.IsNullOrEmpty(downloadLink))
                        {
                            // if luanched from ContextMenu set location next to the movie file
                            if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
                                location = App.StartUpArguments.Path;
                            else // get location from config
                                location = Helper.Settings.DefaultDownloadLocation;

                            if (!Helper.Settings.IsIDMEnabled)
                            {
                                downloadLink = await Helper.GetRedirectedUrl(downloadLink);
                                Debug.WriteLine(downloadLink);
                                var downloader = new DownloadService();
                                downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                                downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                                await downloader.DownloadFileTaskAsync(downloadLink, new DirectoryInfo(location));
                            }
                            else
                            {
                                ProgressStatus.Visibility = Visibility.Collapsed;
                                Helper.OpenLinkWithIDM(downloadLink);
                            }
                        }
                    }
                }
                catch (NullReferenceException ex)
                {
                    ISubtitleDownloadPage.Instance.ShowStatus(null, ex.Message, InfoBarSeverity.Error);
                    DownloadButton.IsEnabled = true;
                }
                catch (UnauthorizedAccessException ex)
                {
                    ISubtitleDownloadPage.Instance.ShowStatus(null, ex.Message, InfoBarSeverity.Error);
                    DownloadButton.IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (Exception ex)
                {
                    ISubtitleDownloadPage.Instance.ShowStatus(null, ex.Message, InfoBarSeverity.Error);
                    DownloadButton.IsEnabled = true;
                }
                finally
                {
                    ProgressStatus.Visibility = Visibility.Collapsed;
                    DownloadButton.IsEnabled = true;
                }
            }
            else
            {
                ISubtitleDownloadPage.Instance.ShowStatus(Constants.InternetIsNotAvailableTitle, Constants.InternetIsNotAvailable, InfoBarSeverity.Error);
            }

        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ISubtitleDownloadPage.Instance.ShowStatus("Download Canceled!", null, InfoBarSeverity.Error);
                });
            }
            else if (e.Error != null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ISubtitleDownloadPage.Instance.ShowStatus(null, e.Error.Message, InfoBarSeverity.Error);
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressStatus.Visibility = Visibility.Collapsed;
                    OpenFolderButton.Visibility = Visibility.Visible;
                    DownloadButton.Visibility = Visibility.Collapsed;
                    var downloadedFileName = ((DownloadPackage)e.UserState).FileName;
                    OpenFolderButton.Tag = downloadedFileName;
                    Helper.DeCompressAndNotification(downloadedFileName, OpenFolderButton, Content.XamlRoot);
                });
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (ProgressStatus.IsIndeterminate == true)
                {
                    ProgressStatus.IsIndeterminate = false;
                }
                ProgressStatus.Value = e.ProgressPercentage;
            });
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenFolderButton.Tag != null)
            {
                Helper.OpenFolderAndSelectFile(OpenFolderButton.Tag.ToString());
            }
            else
            {
                ISubtitleDownloadPage.Instance.ShowStatus(Title + " not downlaoded yet!", null, InfoBarSeverity.Informational);
            }
        }

        private void isubtitleView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (Helper.Settings.IsDoubleClickDownloadEnabled)
            {
                DownloadSubtitle();
            }
        }
    }
}

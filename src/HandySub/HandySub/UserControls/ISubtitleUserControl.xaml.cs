using Downloader;
using HandySub.Common;
using HandySub.Pages;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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
        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse ||
                e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                VisualStateManager.GoToState(sender as Control, "HoverButtonsShown", true);
            }
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "HoverButtonsHidden", true);
        }
        private void DownloadHoverButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadSubtitle();
        }
        public async void DownloadSubtitle()
        {
            if (!Helper.Settings.IsIDMEnabled)
            {
                DownloadHoverButton.IsEnabled = false;
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
                    var downloadLink = Consts.ISubtitleBaseUrl + doc?.DocumentNode
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
                ISubtitleDownloadPage.Instance.ShowInfoBar("Error", ex.Message, InfoBarSeverity.Error);
                DownloadHoverButton.IsEnabled = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                ISubtitleDownloadPage.Instance.ShowInfoBar("Error", ex.Message, InfoBarSeverity.Error);
                DownloadHoverButton.IsEnabled = true;
            }
            catch (NotSupportedException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (Exception ex)
            {
                ISubtitleDownloadPage.Instance.ShowInfoBar("Error", ex.Message, InfoBarSeverity.Error);
                DownloadHoverButton.IsEnabled = true;
            }
            finally
            {
                ProgressStatus.Visibility = Visibility.Collapsed;
            }
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ISubtitleDownloadPage.Instance.ShowInfoBar("Error", "Download Canceled", InfoBarSeverity.Error);
                });
            }
            else if (e.Error != null)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ISubtitleDownloadPage.Instance.ShowInfoBar("Error", e.Error.Message, InfoBarSeverity.Error);
                });
            }
            else
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressStatus.Visibility = Visibility.Collapsed;
                    OpenFolderButton.Visibility = Visibility.Visible;
                    DownloadHoverButton.Visibility = Visibility.Collapsed;
                    var downloadedFileName = ((DownloadPackage)e.UserState).FileName;
                    OpenFolderButton.Tag = downloadedFileName;
                    Helper.DeCompressAndNotification(downloadedFileName, OpenFolderButton, Content.XamlRoot);
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

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Helper.OpenFolderAndSelectFile(OpenFolderButton.Tag.ToString());
        }
    }
}

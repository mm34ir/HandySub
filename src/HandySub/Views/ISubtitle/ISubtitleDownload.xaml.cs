using HandyControl.Controls;
using HandySub.Assets;
using HandySub.Models;
using HtmlAgilityPack;
using ModernWpf.Controls;
using ModernWpf.Navigation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Page = ModernWpf.Controls.Page;
using HandyControl.Tools.Extension;
using HandyControl.Data;
using Path = System.IO.Path;
using System.Diagnostics;
using static HandySub.Assets.Helper;
using HandyControl.Tools;

namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for ISubtitleDownload.xaml
    /// </summary>
    public partial class ISubtitleDownload : Page
    {
        ObservableCollection<SubsceneDownloadModel> DataList = new ObservableCollection<SubsceneDownloadModel>();
        private string location = string.Empty;
        private string subtitleUrl = string.Empty;
        private string fileName = string.Empty;
        public ISubtitleDownload()
        {
            InitializeComponent();

            cmbLanguage.ItemsSource = SubtitleLanguages.LoadSubtitleLanguage();
            cmbLanguage.SelectedItem = Settings.SubtitleLanguage;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var link = e?.Parameter()?.ToString();
            if (!string.IsNullOrEmpty(link))
                subtitleUrl = link;

            if (!string.IsNullOrEmpty(subtitleUrl))
            {
                GetSubtitle();
            }
        }

        private async void GetSubtitle()
        {
            try
            {
                tgBlock.IsChecked = false;

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var items = doc.DocumentNode.SelectSingleNode("//table");
                if (items == null)
                {
                    Growl.ErrorGlobal(LocalizationManager.LocalizeString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    var movieData = items.SelectNodes("//td[@data-title='Release / Movie']");
                    var commentData = items.SelectNodes("//td[@data-title='Comment']");
                    var language = items.SelectNodes("//td[@data-title='Language']");
                    if (movieData != null)
                    {
                        string title = string.Empty;
                        string href = string.Empty;
                        string comment = string.Empty;
                        foreach (var row in movieData.GetEnumeratorWithIndex())
                        {
                            var currentRow = row.Value?.SelectNodes("a");
                            foreach (var cell in currentRow)
                            {
                                title = cell?.InnerText?.Trim();
                                href = $"{Consts.ISubtitleBaseUrl}{cell?.Attributes["href"]?.Value?.Trim()}";
                            }

                            comment = commentData[row.Index]?.InnerText?.Trim();
                            if (comment != null && comment.Contains("&nbsp;"))
                                comment = comment.Replace("&nbsp;", "");

                            if (!string.IsNullOrEmpty(title))
                            {
                                var item = new SubsceneDownloadModel
                                {
                                    Name = title,
                                    Translator = comment,
                                    Link = href,
                                    Language = language[row.Index]?.InnerText.Trim()
                                };

                                DataList.Add(item);
                            }

                        }
                    }
                 
                    listView.ItemsSource = DataList;
                }

                tgBlock.IsChecked = true;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (ArgumentNullException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (WebException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    Growl.ErrorGlobal(LocalizationManager.LocalizeString("ServerNotFound") + "\n" + ex.Message);
                }
            }
            catch (HttpRequestException hx)
            {
                if (!string.IsNullOrEmpty(hx.Message))
                {
                    Growl.ErrorGlobal(LocalizationManager.LocalizeString("ServerNotFound") + "\n" + hx.Message);
                }
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetSubtitle();
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tgBlock.IsChecked = false;
            prgStatus.Value = 0;
            prgStatus.IsIndeterminate = false;
            var item = listView.SelectedItem as SubsceneDownloadModel;
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(item.Link);

                var downloadLink = Consts.ISubtitleBaseUrl + doc?.DocumentNode
                        ?.SelectSingleNode("//div[@class='col-lg-16 col-md-24 col-sm-16']//a")?.Attributes["href"]
                        ?.Value;

                // if luanched from ContextMenu set location next to the movie file
                if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
                    location = App.StartUpArguments.Path;
                else // get location from config
                    location = Settings.StoreLocation;

                if (!Settings.IsIDMEnabled)
                {
                    fileName = $"{item.Name}.zip";
                    var link = await GetRedirectedUrl(downloadLink);
                    var client = new WebClient();
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    await client.DownloadFileTaskAsync(link, Path.Combine(location, fileName));
                }
                else
                {
                    tgBlock.IsChecked = true;
                    OpenLinkWithIDM(downloadLink, IDMNotFound);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Growl.ErrorGlobal(LocalizationManager.LocalizeString("AdminError"));
            }
            catch (NotSupportedException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (WebException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    Growl.ErrorGlobal(ex.Message);
                }
            }
            catch (HttpRequestException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    Growl.ErrorGlobal(ex.Message);
                }
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            tgBlock.IsChecked = true;
            prgStatus.Value = 0;
            prgStatus.IsIndeterminate = true;
            txtStatus.Text = string.Empty;
            if (Settings.IsShowNotification)
            {
                Growl.ClearGlobal();
                Growl.AskGlobal(new GrowlInfo
                {
                    CancelStr = LocalizationManager.LocalizeString("Cancel"),
                    ConfirmStr = LocalizationManager.LocalizeString("OpenFolder"),
                    Message = LocalizationManager.LocalizeString("FileDownloaded").Format(Path.GetFileNameWithoutExtension(fileName)),
                    ActionBeforeClose = b =>
                    {
                        if (!b) return true;

                        Process.Start("explorer.exe", "/select, \"" + Path.Combine(location, fileName) + "\"");
                        return true;
                    }
                });
            }
        }

        private void Client_DownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            prgStatus.Value = e.ProgressPercentage;
            txtStatus.Text = $"{(int)e.ProgressPercentage}%";
        }

        private void IDMNotFound()
        {
            Growl.WarningGlobal(LocalizationManager.LocalizeString("IDMNot"));
        }

        private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var lang = (string)cmbLanguage.SelectedItem;
            if (lang.Equals("All"))
            {
                DataList.ShapeView().Where(p => (p.Name.IndexOf(autoBox.Text, StringComparison.OrdinalIgnoreCase) != -1) ||
                         (p.Translator.IndexOf(autoBox.Text, StringComparison.OrdinalIgnoreCase) != -1)).Apply();
            }
            else
            {
                DataList.ShapeView().Where(p => (p.Language.Contains(lang, StringComparison.OrdinalIgnoreCase)) &&
                         (p.Name.IndexOf(autoBox.Text, StringComparison.OrdinalIgnoreCase) != -1) ||
                         (p.Language.Contains(lang, StringComparison.OrdinalIgnoreCase)) &&
                         (p.Translator.IndexOf(autoBox.Text, StringComparison.OrdinalIgnoreCase) != -1)).Apply();
            }
        }

        private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cmbLanguage.SelectedItem as string;
            if (item != Settings.SubtitleLanguage)
            {
                Settings.SubtitleLanguage = item;
            }
            if (item.Equals("All"))
            {
                DataList.ShapeView().ClearAll().Apply();
            }
            else
            {
                DataList.ShapeView().Where(x => x.Language.Contains(item.ToString(), StringComparison.OrdinalIgnoreCase)).Apply();
            }
        }
    }
}

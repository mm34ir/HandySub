using HandyControl.Controls;
using HandySub.Assets;
using HandySub.Models;
using HtmlAgilityPack;
using ModernWpf.Controls;
using ModernWpf.Navigation;
using System;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Page = ModernWpf.Controls.Page;
using System.Collections.ObjectModel;
using HandyControl.Tools.Extension;
using Downloader;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using HandyControl.Tools;
using HandyControl.Data;
using HandySub.Assets.Strings;
using static HandySub.Assets.Helper;
namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for SubsceneDownload.xaml
    /// </summary>
    public partial class SubsceneDownload : Page
    {
        ObservableCollection<SubsceneDownloadModel> DataList = new ObservableCollection<SubsceneDownloadModel>();

        private string location = string.Empty;

        private string subtitleUrl = string.Empty;
        public SubsceneDownload()
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
                subtitleUrl = Settings.SubsceneServer.Url + link;

            if (!string.IsNullOrEmpty(subtitleUrl))
            {
                GetSubtitle();
            }
        }

        private void GetSubtitle()
        {
            if (Settings.SubsceneServer.Url.Contains("subf2m"))
                Subf2mCore();
            else
                SubsceneCore();
        }

        private async void SubsceneCore()
        {
            try
            {
                tgBlock.IsChecked = false;
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var table = doc.DocumentNode.SelectSingleNode("//table[1]//tbody");
                if (table != null)
                {
                    DataList?.Clear();
                    foreach (var cell in table.SelectNodes(".//tr"))
                    {
                        if (cell.InnerText.Contains("There are no subtitles")) 
                            break;

                        var Language = cell.SelectSingleNode(".//td[@class='a1']//a//span[1]")?.InnerText.Trim();
                        var Name = cell.SelectSingleNode(".//td[@class='a1']//a//span[2]")?.InnerText.Trim();
                        var Translator = cell.SelectSingleNode(".//td[@class='a5']//a")?.InnerText.Trim();
                        var Comment = cell.SelectSingleNode(".//td[@class='a6']//div")?.InnerText.Trim();
                        if (Comment != null && Comment.Contains("&nbsp;")) Comment = Comment.Replace("&nbsp;", "");

                        Comment = Comment + Environment.NewLine + Translator;

                        var Link = cell.SelectSingleNode(".//td[@class='a1']//a")?.Attributes["href"]?.Value.Trim();

                        if (Name != null)
                        {
                            var item = new SubsceneDownloadModel
                            {
                                Name = Name,
                                Translator = Comment,
                                Link = Link,
                                Language = Language
                            };
                            DataList.Add(item);
                        }
                    }
                    listView.ItemsSource = DataList;
                }
                else
                {
                    Growl.ErrorGlobal(Lang.ResourceManager.GetString("SubNotFound"));
                }
                tgBlock.IsChecked = true;
            }
            catch (ArgumentNullException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (WebException ex)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }

        private async void Subf2mCore()
        {
            try
            {
                tgBlock.IsChecked = false;
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var repeater = doc.DocumentNode.SelectNodes("//ul[@class='scrolllist']");

                if (repeater == null)
                {
                    Growl.ErrorGlobal(Lang.ResourceManager.GetString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    foreach (var node in repeater.GetEnumeratorWithIndex())
                    {
                        var language = node.Value.SelectNodes("//div[@class='topright']//span[1]")[node.Index].InnerText;
                        var translator = node.Value.SelectNodes("//div[@class='comment-col']")[node.Index].InnerText;
                        var download_Link = node.Value.SelectNodes("//a[@class='download icon-download']")[node.Index].GetAttributeValue("href", "");

                        //remove empty lines
                        var singleLineTranslator = Regex.Replace(translator, @"\s+", " ").Replace("&nbsp;", "");
                        if (singleLineTranslator.Contains("&nbsp;"))
                            singleLineTranslator = singleLineTranslator.Replace("&nbsp;", "");

                        var item = new SubsceneDownloadModel
                        {
                            Name = node.Value.InnerText.Trim(),
                            Translator = singleLineTranslator.Trim(),
                            Link = download_Link.Trim(),
                            Language = language
                        };
                        DataList.Add(item);
                    }
                }
                listView.ItemsSource = DataList;
                tgBlock.IsChecked = true;
            }
            catch (ArgumentNullException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (WebException ex)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
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
                var doc = await web.LoadFromWebAsync(Settings.SubsceneServer.Url + item.Link);

                var downloadLink = Settings.SubsceneServer.Url + doc.DocumentNode.SelectSingleNode(
                        "//div[@class='download']//a").GetAttributeValue("href", "nothing");

                // if luanched from ContextMenu set location next to the movie file
                if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
                    location = App.WindowsContextMenuArgument[1];
                else // get location from config
                    location = Settings.StoreLocation;

                if (!Settings.IsIDMEnabled)
                {
                    var downloader = new DownloadService();
                    downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                    downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                    await downloader.DownloadFileTaskAsync(downloadLink, new DirectoryInfo(location));
                }
                else
                {
                    tgBlock.IsChecked = true;
                    OpenLinkWithIDM(downloadLink, IDMNotFound);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("AdminError"));
            }
            catch (NotSupportedException)
            {
            }
            catch (ArgumentException)
            {
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }
        private void IDMNotFound()
        {
            Growl.WarningGlobal(Lang.ResourceManager.GetString("IDMNot"));
        }
        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                tgBlock.IsChecked = true;
                prgStatus.Value = 0;
                prgStatus.IsIndeterminate = true;
                txtStatus.Text = string.Empty;
                if (Settings.IsShowNotification)
                {
                    var downlaodedFileName = ((DownloadPackage)e.UserState).FileName;

                    Growl.ClearGlobal();
                    Growl.AskGlobal(new GrowlInfo
                    {
                        CancelStr = Lang.ResourceManager.GetString("Cancel"),
                        ConfirmStr = Lang.ResourceManager.GetString("OpenFolder"),
                        Message = Lang.ResourceManager.GetString("FileDownloaded").Format(Path.GetFileNameWithoutExtension(downlaodedFileName)),
                        ActionBeforeClose = b =>
                        {
                            if (!b) return true;

                            Process.Start("explorer.exe", "/select, \"" + downlaodedFileName + "\"");
                            return true;
                        }
                    });
                }
            });
        }

        private void Downloader_DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(() => {
                prgStatus.Value = e.ProgressPercentage;
                txtStatus.Text = $"{(int)e.ProgressPercentage}%";
            });
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetSubtitle();
        }

        private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var lang = (string)cmbLanguage.SelectedItem;
            if (lang.Equals("All"))
            {
                DataList.ShapeView().Where(p=> (p.Name.IndexOf(autoBox.Text, StringComparison.OrdinalIgnoreCase) != -1) ||
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

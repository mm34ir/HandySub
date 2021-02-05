using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Downloader;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using HandySub.Data;
using HandySub.Language;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;
using MessageBox = HandyControl.Controls.MessageBox;

namespace HandySub.ViewModels
{
    public class SubsceneDownloadViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        private string location = string.Empty;

        private string subtitleUrl = string.Empty;

        public SubsceneDownloadViewModel()
        {
            MainWindowViewModel.Instance.IsBackEnabled = true;

            OpenSubtitlePageCommand = new DelegateCommand<SelectionChangedEventArgs>(OpenSubtitlePage);
            RefreshCommand = new DelegateCommand(GetSubtitle);
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var link = navigationContext.Parameters["link_key"] as string;
            if (!string.IsNullOrEmpty(link)) subtitleUrl = GlobalDataHelper<AppConfig>.Config.ServerUrl + link;

            GetSubtitle();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public bool KeepAlive => false;

        private async void OpenSubtitlePage(SelectionChangedEventArgs e)
        {
            IsBusy = true;
            IsEnabled = false;
            Progress = 0;
            if (e.AddedItems[0] is SubsceneDownloadModel item)
                try
                {
                    var web = new HtmlWeb();
                    var doc =
                        await web.LoadFromWebAsync(GlobalDataHelper<AppConfig>.Config.ServerUrl + item.Link);

                    var downloadLink = GlobalDataHelper<AppConfig>.Config.ServerUrl + doc.DocumentNode
                        .SelectSingleNode(
                            "//div[@class='download']//a").GetAttributeValue("href", "nothing");

                    // if luanched from ContextMenu set location next to the movie file
                    if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
                        location = App.WindowsContextMenuArgument[1];
                    else // get location from config
                        location = GlobalDataHelper<AppConfig>.Config.StoreLocation;

                    if (!GlobalDataHelper<AppConfig>.Config.IsIDMEngine)
                    {
                        var downloader = new DownloadService();
                        downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                        downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                        await downloader.DownloadFileAsync(downloadLink, new DirectoryInfo(location));
                    }
                    else
                    {
                        IsBusy = false;
                        IsEnabled = true;
                        Helper.Current.OpenLinkWithIDM(downloadLink, IDMNotFound);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Error(Lang.ResourceManager.GetString("AdminError"),
                        Lang.ResourceManager.GetString("AdminErrorTitle"));
                    IsBusy = false;
                    IsEnabled = true;
                }
                catch (NotSupportedException)
                {
                }
                catch (ArgumentException)
                {
                }
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            IsEnabled = true;
            IsBusy = false;
            if (GlobalDataHelper<AppConfig>.Config.IsShowNotification)
            {
                var downlaodedFileName = ((DownloadPackage) e.UserState).FileName;

                Growl.ClearGlobal();
                Application.Current.Dispatcher.Invoke((Action) delegate
                {
                    Growl.AskGlobal(new GrowlInfo
                    {
                        CancelStr = Lang.ResourceManager.GetString("Cancel"),
                        ConfirmStr = Lang.ResourceManager.GetString("OpenFolder"),
                        Message = string.Format(Lang.ResourceManager.GetString("Downloaded"),
                            Path.GetFileNameWithoutExtension(downlaodedFileName)),
                        ActionBeforeClose = b =>
                        {
                            if (!b) return true;

                            Process.Start("explorer.exe", "/select, \"" + downlaodedFileName + "\"");
                            return true;
                        }
                    });
                });
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }

        private void GetSubtitle()
        {
            if (GlobalDataHelper<AppConfig>.Config.ServerUrl.Contains("subf2m"))
                Subf2mCore();
            else
                SubsceneCore();
        }

        private async void SubsceneCore()
        {
            IsBusy = true;
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var table = doc.DocumentNode.SelectSingleNode("//table[1]//tbody");
                if (table != null)
                {
                    DataList?.Clear();
                    foreach (var cell in table.SelectNodes(".//tr"))
                    {
                        if (cell.InnerText.Contains("There are no subtitles")) break;

                        var Name = cell.SelectSingleNode(".//td[@class='a1']//a//span[2]")?.InnerText.Trim();
                        var Translator = cell.SelectSingleNode(".//td[@class='a5']//a")?.InnerText.Trim();
                        var Comment = cell.SelectSingleNode(".//td[@class='a6']//div")?.InnerText.Trim();
                        if (Comment != null && Comment.Contains("&nbsp;")) Comment = Comment.Replace("&nbsp;", "");

                        Comment = Comment + Environment.NewLine + Translator;

                        var Link = cell.SelectSingleNode(".//td[@class='a1']//a")?.Attributes["href"]?.Value.Trim();

                        if (Name != null)
                        {
                            var item = new SubsceneDownloadModel
                                {Name = Name, Translator = Comment, Link = Link};
                            DataList.Add(item);
                        }
                    }
                }
                else
                {
                    MessageBox.Error(Lang.ResourceManager.GetString("SubNotFound"));
                }

                IsBusy = false;
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
                IsBusy = false;
            }
        }

        private async void Subf2mCore()
        {
            IsBusy = true;
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var repeater = doc.DocumentNode.SelectNodes("//ul[@class='scrolllist']");

                if (repeater == null)
                {
                    MessageBox.Error(Lang.ResourceManager.GetString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    foreach ((var node, var index) in repeater.WithIndex())
                    {
                        var translator = node.SelectNodes("//div[@class='comment-col']")[index].InnerText;
                        var download_Link = node.SelectNodes("//a[@class='download icon-download']")[index]
                            .GetAttributeValue("href", "");

                        //remove empty lines
                        var singleLineTranslator = Regex.Replace(translator, @"\s+", " ").Replace("&nbsp;", "");
                        if (singleLineTranslator.Contains("&nbsp;"))
                            singleLineTranslator = singleLineTranslator.Replace("&nbsp;", "");

                        var item = new SubsceneDownloadModel
                        {
                            Name = node.InnerText.Trim(), Translator = singleLineTranslator.Trim(),
                            Link = download_Link.Trim()
                        };
                        DataList.Add(item);
                    }
                }

                IsBusy = false;
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
                IsBusy = false;
            }
        }

        private void IDMNotFound()
        {
            MessageBox.Warning(LocalizationManager.Instance.Localize("IDMNot").ToString());
        }

        #region Property

        private double _progress;

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                DataList.ShapeView()
                    .Where(p => (p.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1) ||
                                (p.Translator.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1))
                    .Apply();
            }
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private ObservableCollection<SubsceneDownloadModel> _datalist =
            new();

        public ObservableCollection<SubsceneDownloadModel> DataList
        {
            get => _datalist;
            set => SetProperty(ref _datalist, value);
        }

        #endregion

        #region Command

        public DelegateCommand<SelectionChangedEventArgs> OpenSubtitlePageCommand { get; }
        public DelegateCommand RefreshCommand { get; }

        #endregion
    }
}
using Downloader;
using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Data;
using HandySub.Language;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using MessageBox = HandyControl.Controls.MessageBox;

namespace HandySub.ViewModels
{
    public class SubsceneDownloadViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
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
                ItemsView.Refresh();
            }
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        private bool _isEnabled = true;
        public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }

        private ObservableCollection<SubsceneDownloadModel> _datalist = new ObservableCollection<SubsceneDownloadModel>();
        public ObservableCollection<SubsceneDownloadModel> DataList
        {
            get => _datalist;
            set => SetProperty(ref _datalist, value);
        }

        #endregion

        #region Command
        public DelegateCommand<SelectionChangedEventArgs> OpenSubtitlePageCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        #endregion

        public ICollectionView ItemsView => CollectionViewSource.GetDefaultView(DataList);

        public bool KeepAlive => false;

        private string subtitleUrl = string.Empty;
        private string location = string.Empty;

        public SubsceneDownloadViewModel()
        {
            MainWindowViewModel.Instance.IsBackEnabled = true;

            OpenSubtitlePageCommand = new DelegateCommand<SelectionChangedEventArgs>(OpenSubtitlePage);
            ItemsView.Filter = new Predicate<object>(o => Filter(o as SubsceneDownloadModel));
            RefreshCommand = new DelegateCommand(GetSubtitle);
        }

        private bool Filter(SubsceneDownloadModel item)
        {
            return SearchText == null
                            || item.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1 || item.Translator.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1;
        }

        private async void OpenSubtitlePage(SelectionChangedEventArgs e)
        {
            IsBusy = true;
            IsEnabled = false;
            Progress = 0;
            if (e.AddedItems[0] is SubsceneDownloadModel item)
            {
                try
                {
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = await web.LoadFromWebAsync(GlobalDataHelper<AppConfig>.Config.ServerUrl + item.Link);

                    string downloadLink = GlobalDataHelper<AppConfig>.Config.ServerUrl + doc.DocumentNode.SelectSingleNode(
                                "//div[@class='download']//a").GetAttributeValue("href", "nothing");

                    // if luanched from ContextMenu set location next to the movie file
                    if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
                    {
                        location = App.WindowsContextMenuArgument[1];
                    }
                    else // get location from config
                    {
                        location = GlobalDataHelper<AppConfig>.Config.StoreLocation;
                    }

                    if (!GlobalDataHelper<AppConfig>.Config.IsIDMEngine)
                    {
                        var downloader = new DownloadService();
                        downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                        downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                        await downloader.DownloadFileAsync(downloadLink, new System.IO.DirectoryInfo(location));
                    }
                    else
                    {
                        IsBusy = false;
                        IsEnabled = true;
                        Helper.OpenLinkWithIDM(downloadLink, IDMNotFound);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    HandyControl.Controls.MessageBox.Error(Lang.ResourceManager.GetString("AdminError"), Lang.ResourceManager.GetString("AdminErrorTitle"));
                    IsBusy = false;
                    IsEnabled = true;

                }
                catch (NotSupportedException) { }
                catch (ArgumentException) { }
            }
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            IsEnabled = true;
            IsBusy = false;
            if (GlobalDataHelper<AppConfig>.Config.IsShowNotification)
            {
                Growl.ClearGlobal();
                Growl.AskGlobal(new GrowlInfo
                {
                    CancelStr = Lang.ResourceManager.GetString("Cancel"),
                    ConfirmStr = Lang.ResourceManager.GetString("OpenFolder"),
                    Message = Lang.ResourceManager.GetString("Subtitle Downloaded"),
                    ActionBeforeClose = b =>
                    {
                        if (!b)
                        {
                            return true;
                        }
                        System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + location + "\"");
                        return true;

                    }
                });
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }

        private void GetSubtitle()
        {
            if (GlobalDataHelper<AppConfig>.Config.ServerUrl.Contains("subf2m"))
            {
                Subf2mCore();
            }
            else
            {
                SubsceneCore();
            }
        }

        private async void SubsceneCore()
        {
            IsBusy = true;
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(subtitleUrl);

                HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[1]//tbody");
                if (table != null)
                {
                    DataList?.Clear();
                    foreach (HtmlNode cell in table.SelectNodes(".//tr"))
                    {
                        if (cell.InnerText.Contains("There are no subtitles"))
                        {
                            break;
                        }

                        string Name = cell.SelectSingleNode(".//td[@class='a1']//a//span[2]")?.InnerText.Trim();
                        string Translator = cell.SelectSingleNode(".//td[@class='a5']//a")?.InnerText.Trim();
                        string Comment = cell.SelectSingleNode(".//td[@class='a6']//div")?.InnerText.Trim();
                        if (Comment != null && Comment.Contains("&nbsp;"))
                        {
                            Comment = Comment.Replace("&nbsp;", "");
                        }
                        Comment = Comment + Environment.NewLine + Translator;

                        string Link = cell.SelectSingleNode(".//td[@class='a1']//a")?.Attributes["href"]?.Value.Trim();

                        if (Name != null)
                        {

                            SubsceneDownloadModel item = new SubsceneDownloadModel { Name = Name, Translator = Comment, Link = Link };
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
            catch (ArgumentNullException) { }
            catch (ArgumentOutOfRangeException) { }
            catch (System.Net.WebException ex)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
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
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(subtitleUrl);

                HtmlNodeCollection repeater = doc.DocumentNode.SelectNodes("//ul[@class='scrolllist']");

                if (repeater == null)
                {
                    MessageBox.Error(Lang.ResourceManager.GetString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    foreach ((HtmlNode node, int index) in repeater.WithIndex())
                    {
                        string translator = node.SelectNodes("//div[@class='comment-col']")[index].InnerText;
                        string download_Link = node.SelectNodes("//a[@class='download icon-download']")[index].GetAttributeValue("href", "");

                        //remove empty lines
                        string singleLineTranslator = Regex.Replace(translator, @"\s+", " ").Replace("&nbsp;", "");
                        if (singleLineTranslator.Contains("&nbsp;"))
                        {
                            singleLineTranslator = singleLineTranslator.Replace("&nbsp;", "");
                        }
                        SubsceneDownloadModel item = new SubsceneDownloadModel { Name = node.InnerText.Trim(), Translator = singleLineTranslator.Trim(), Link = download_Link.Trim() };
                        DataList.Add(item);
                    }
                }
                IsBusy = false;

            }
            catch (ArgumentNullException) { }
            catch (ArgumentOutOfRangeException) { }
            catch (System.Net.WebException ex)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            string link = navigationContext.Parameters["link_key"] as string;
            if (!string.IsNullOrEmpty(link))
            {
                subtitleUrl = GlobalDataHelper<AppConfig>.Config.ServerUrl + link;
            }
            GetSubtitle();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        private void IDMNotFound()
        {
            MessageBox.Warning(LocalizationManager.Instance.Localize("IDMNot").ToString());
        }
    }
}

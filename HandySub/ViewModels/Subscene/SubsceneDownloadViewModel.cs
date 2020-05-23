using HandyControl.Controls;
using HandyControl.Data;
using HtmlAgilityPack;
using Module.Core;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using HandySub.Data;
using HandySub.Language;
using HandySub.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using MessageBox = HandyControl.Controls.MessageBox;

namespace HandySub.ViewModels
{
    public class SubsceneDownloadViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        #region Property
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

        #region ToggleButton
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private string _content = LocalizationManager.Instance.Localize("SubDownload").ToString();
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        private bool _isChecked = false;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        private int _progress = 0;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }
        #endregion
        private string _episode;
        public string Episode
        {
            get => _episode;
            set => SetProperty(ref _episode, value);
        }

        private string _translator;
        public string Translator
        {
            get => _translator;
            set => SetProperty(ref _translator, value);
        }

        private string _info;
        public string Info
        {
            get => _info;
            set => SetProperty(ref _info, value);
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }


        private bool _isOpen;
        public bool IsOpen { get => _isOpen; set => SetProperty(ref _isOpen, value); }

        private bool _maskCanClose = true;
        public bool MaskCanClose { get => _maskCanClose; set => SetProperty(ref _maskCanClose, value); }

        private ObservableCollection<SubsceneDownloadModel> _datalist = new ObservableCollection<SubsceneDownloadModel>();
        public ObservableCollection<SubsceneDownloadModel> DataList
        {
            get => _datalist;
            set => SetProperty(ref _datalist, value);
        }

        #endregion

        #region Command
        public DelegateCommand OnDownloadClickCommand { get; private set; }
        public DelegateCommand<SelectionChangedEventArgs> OpenSubtitlePageCommand { get; private set; }
        public DelegateCommand GoBackCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        #endregion

        public ICollectionView ItemsView => CollectionViewSource.GetDefaultView(DataList);
        private readonly IRegionManager _regionManager;

        public bool KeepAlive => false;

        private readonly WebClient client = new WebClient();
        private string subtitleUrl = string.Empty;
        private string SubtitleDownloadLink = string.Empty;
        private string location = string.Empty;
        private string subName = string.Empty;

        public SubsceneDownloadViewModel() { }
        public SubsceneDownloadViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            OnDownloadClickCommand = new DelegateCommand(OnDownloadClick);
            OpenSubtitlePageCommand = new DelegateCommand<SelectionChangedEventArgs>(OpenSubtitlePage);
            ItemsView.Filter = new Predicate<object>(o => Filter(o as SubsceneDownloadModel));
            GoBackCommand = new DelegateCommand(GoBack);
            RefreshCommand = new DelegateCommand(GetSubtitle);
        }

        private void GoBack()
        {
            _regionManager.RequestNavigate("ContentRegion", "Subscene");
        }

        private bool Filter(SubsceneDownloadModel item)
        {
            return SearchText == null
                            || item.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1 || item.Translator.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1;
        }

        private void OpenSubtitlePage(SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] is SubsceneDownloadModel item)
            {
                Info = item.Name;
                Translator = item.Translator;

                //remove Unnecessary words
                if (Info.Contains("By"))
                {
                    int index = Info.IndexOf("By");
                    if (index > 0)
                    {
                        Info = Info.Substring(0, index);
                    }

                    Info = Regex.Replace(Info, @"\s+", " ");
                }

                Regex regex = new Regex("S[0-9].{1}E[0-9].{1}");
                Match match = regex.Match(Info);
                if (match.Success)
                {
                    Episode = match.Value;
                }

                SubtitleDownloadLink = item.Link;
                Content = Lang.ResourceManager.GetString("SubDownload");
                IsOpen = true;

                if (GlobalDataHelper<AppConfig>.Config.IsAutoDownloadSubtitle)
                {
                    OnDownloadClick();
                }
            }
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
                Growl.Error(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
            {
                Growl.Error(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + hx.Message);
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
                Growl.Error(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
            {
                Growl.Error(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + hx.Message);
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

        #region Downloader

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            Progress = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            IsChecked = false;
            IsEnabled = true;
            MaskCanClose = true;
            Progress = 0;
            Content = Lang.ResourceManager.GetString("OpenFolder");
            if (GlobalDataHelper<AppConfig>.Config.IsShowNotification)
            {
                Growl.Clear();
                Growl.Ask(new GrowlInfo
                {
                    CancelStr = Lang.ResourceManager.GetString("Cancel"),
                    ConfirmStr = Lang.ResourceManager.GetString("OpenFolder"),
                    Message = string.Format(Lang.ResourceManager.GetString("Downloaded"), Episode + subName),
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

        private async void OnDownloadClick()
        {
            try
            {

                if (Content != Lang.ResourceManager.GetString("OpenFolder"))
                {
                    MaskCanClose = false;
                    IsChecked = true;
                    IsEnabled = false;
                    Content = Lang.ResourceManager.GetString("Downloading");
                    Progress = 0;

                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = await web.LoadFromWebAsync(GlobalDataHelper<AppConfig>.Config.ServerUrl + SubtitleDownloadLink);

                    string downloadLink = doc.DocumentNode.SelectSingleNode(
                                "//div[@class='download']//a").GetAttributeValue("href", "nothing");

                    if (!GlobalDataHelper<AppConfig>.Config.IsIDMEngine)
                    {
                        // we need to get file name
                        byte[] data = client.DownloadData(GlobalDataHelper<AppConfig>.Config.ServerUrl + downloadLink);

                        if (!string.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
                        {
                            subName = client.ResponseHeaders["Content-Disposition"].Substring(client.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                        }

                        // if luanched from ContextMenu set location next to the movie file
                        if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
                        {
                            location = App.WindowsContextMenuArgument[1] + Episode + subName;
                        }
                        else // get location from config
                        {
                            location = GlobalDataHelper<AppConfig>.Config.StoreLocation + Episode + subName;
                        }
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                        client.DownloadFileAsync(new Uri(GlobalDataHelper<AppConfig>.Config.ServerUrl + downloadLink), location);
                    }
                    else
                    {
                        IsChecked = false;
                        IsEnabled = true;
                        MaskCanClose = true;
                        Progress = 0;
                        Content = LocalizationManager.Instance.Localize("SubDownload").ToString();
                        ModuleHelper.OpenLinkWithIDM(GlobalDataHelper<AppConfig>.Config.ServerUrl + downloadLink, IDMNotFound);
                    }
                }
                else
                {
                    System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + location + "\"");
                }
            }
            catch (UnauthorizedAccessException)
            {
                HandyControl.Controls.MessageBox.Error(Lang.ResourceManager.GetString("AdminError"), Lang.ResourceManager.GetString("AdminErrorTitle"));
            }
            catch (NotSupportedException) { }
            catch (ArgumentException) { }
        }
        private void IDMNotFound()
        {
            MessageBox.Warning(LocalizationManager.Instance.Localize("IDMNot").ToString());
        }
        #endregion
    }
}

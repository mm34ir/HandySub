using HandyControl.Controls;
using HandyControl.Data;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SubtitleDownloader.Data;
using SubtitleDownloader.Language;
using SubtitleDownloader.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SubtitleDownloader.ViewModels
{
    public class SubsceneViewModel : BindableBase, INavigationAware
    {
        internal static SubsceneViewModel Instance;
        private const string SearchAPI = "{0}/subtitles/searchbytitle?query={1}&l=";

        private readonly IRegionManager _regionManager;

        #region Property
        private string _DefaultSubLang;
        public string DefaultSubLang
        {
            get => LocalizationManager.Instance.Localize(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LocalizeCode).ToString();
            set => SetProperty(ref _DefaultSubLang, value);
        }

        private ObservableCollection<LanguageModel> _languageItems = new ObservableCollection<LanguageModel>();
        public ObservableCollection<LanguageModel> LanguageItems
        {
            get => _languageItems;
            set => SetProperty(ref _languageItems, value);
        }

        private Visibility _visibility = Visibility.Hidden;
        public Visibility ContentVisibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        private ObservableCollection<SubsceneModel> _dataList = new ObservableCollection<SubsceneModel>();
        public ObservableCollection<SubsceneModel> DataList
        {
            get => _dataList;
            set => SetProperty(ref _dataList, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        #endregion

        #region Command
        public DelegateCommand<SelectionChangedEventArgs> SubtitleLanguageCommand { get; private set; }

        public DelegateCommand<SelectionChangedEventArgs> OpenSubtitlePageCommand { get; private set; }

        public DelegateCommand<FunctionEventArgs<string>> OnSearchStartedCommand { get; private set; }

        #endregion

        public ICollectionView ItemsView => CollectionViewSource.GetDefaultView(LanguageItems);

        public SubsceneViewModel()
        {

        }

        public SubsceneViewModel(IRegionManager regionManager)
        {
            Instance = this;
            _regionManager = regionManager;
            OnSearchStartedCommand = new DelegateCommand<FunctionEventArgs<string>>(OnSearchStarted);
            OpenSubtitlePageCommand = new DelegateCommand<SelectionChangedEventArgs>(OpenSubtitlePage);
            SubtitleLanguageCommand = new DelegateCommand<SelectionChangedEventArgs>(SubtitleLanguageChanged);
            LoadLanguage();
        }

        public void LoadLanguage()
        {
            LanguageItems.Clear();
            DefaultSubLang = LocalizationManager.Instance.Localize(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LocalizeCode).ToString();
            LanguageItems.AddRange(SupportedLanguages.LoadSubtitleLanguage());
        }
        private void SubtitleLanguageChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            if (e.AddedItems[0] is LanguageModel item)
            {
                if (!item.Equals(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage))
                {
                    GlobalDataHelper<AppConfig>.Config.SubtitleLanguage = item;
                    GlobalDataHelper<AppConfig>.Save();
                }
            }
        }

        private void OpenSubtitlePage(SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] is SubsceneModel item)
            {
                if (item.Link != null)
                {
                    NavigationParameters parameters = new NavigationParameters
                    { { "link_key", item.Link } };

                    _regionManager.RequestNavigate("ContentRegion", "SubsceneDownload", parameters);
                }
            }
        }

        private async Task<string> getTitleByImdbId(string ImdbId)
        {
            string result = string.Empty;
            string url = $"http://www.omdbapi.com/?i={ImdbId}&apikey=2a59a17e";

            try
            {
                using HttpClient client = new HttpClient();
                string responseBody = await client.GetStringAsync(url);
                IMDBModel.Root parse = System.Text.Json.JsonSerializer.Deserialize<IMDBModel.Root>(responseBody);

                if (parse.Response.Equals("True"))
                {
                    result = parse.Title;
                }
                else
                {
                    Growl.Error(parse.Error);
                }

            }
            catch (HttpRequestException ex)
            {
                Growl.Error(ex.Message);
            }

            return result;
        }

        private async void OnSearchStarted(FunctionEventArgs<string> e)
        {
            try
            {
                if (string.IsNullOrEmpty(SearchText))
                {
                    return;
                }

                ContentVisibility = Visibility.Visible;
                IsBusy = true;


                //Get Title with imdb
                if (SearchText.StartsWith("tt"))
                {
                    SearchText = await getTitleByImdbId(SearchText);
                }

                string url = string.Format(SearchAPI, GlobalDataHelper<AppConfig>.Config.ServerUrl, SearchText);
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(url);

                HtmlNodeCollection repeater = doc.DocumentNode.SelectNodes("//div[@class='title']");
                if (repeater == null)
                {
                    MessageBox.Error(Lang.ResourceManager.GetString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    foreach (HtmlNode node in repeater)
                    {
                        if (node.InnerText.Contains("OFFER POST"))
                        {
                            continue;
                        }
                        SubsceneModel item = new SubsceneModel { Link = node.SelectSingleNode(".//a")?.Attributes["href"]?.Value + $"/{GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LanguageCode}/", Name = node.InnerText.Trim() };
                        DataList.Add(item);
                    }
                }
                IsBusy = false;
            }
            catch (ArgumentOutOfRangeException) { }
            catch (ArgumentNullException) { }
            catch (System.NullReferenceException) { }
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
            string name = navigationContext.Parameters["name_key"] as string;
            if (!string.IsNullOrEmpty(name))
            {
                SearchText = name;
                OnSearchStarted(null);
            }

            if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
            {
                SearchText = App.WindowsContextMenuArgument[0];
                OnSearchStarted(null);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}

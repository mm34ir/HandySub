using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Windows.Controls;
using HandyControl.Controls;
using HandySub.Data;
using HandySub.Language;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace HandySub.ViewModels
{
    public class SubsceneViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        public string SearchText = string.Empty;
        private const string SearchAPI = "{0}/subtitles/searchbytitle?query={1}&l=";

        internal static SubsceneViewModel Instance;

        private readonly IRegionManager _regionManager;

        public SubsceneViewModel()
        {
        }

        public SubsceneViewModel(IRegionManager regionManager)
        {
            Instance = this;
            MainWindowViewModel.Instance.IsBackEnabled = false;
            _regionManager = regionManager;
            OpenSubtitlePageCommand = new DelegateCommand<SelectionChangedEventArgs>(OpenSubtitlePage);
            SubtitleLanguageCommand = new DelegateCommand<SelectionChangedEventArgs>(SubtitleLanguageChanged);
            LoadLanguage();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
            {
                SearchText = App.WindowsContextMenuArgument[0];
                OnSearchStarted();
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public bool KeepAlive => GlobalDataHelper<AppConfig>.Config.IsKeepAlive;

        public void LoadLanguage()
        {
            LanguageItems.Clear();
            DefaultSubLang = LocalizationManager.Instance
                .Localize(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LocalizeCode).ToString();
            LanguageItems.AddRange(SupportedLanguages.LoadSubtitleLanguage());
        }

        private void SubtitleLanguageChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            if (e.AddedItems[0] is LanguageModel item)
                if (!item.Equals(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage))
                {
                    GlobalDataHelper<AppConfig>.Config.SubtitleLanguage = item;
                    GlobalDataHelper<AppConfig>.Save();
                }
        }

        private void OpenSubtitlePage(SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] is SubsceneModel item)
                if (item.Link != null)
                {
                    var parameters = new NavigationParameters
                        {{"link_key", item.Link}};

                    _regionManager.RequestNavigate("ContentRegion", "SubsceneDownload", parameters);
                }
        }


        public async void OnSearchStarted()
        {
            try
            {
                if (string.IsNullOrEmpty(SearchText)) return;

                IsBusy = true;
                DataList?.Clear();

                //Get Title with imdb
                if (SearchText.StartsWith("tt")) SearchText = await Helper.Current.GetTitleByImdbId(SearchText);

                var url = string.Format(SearchAPI, GlobalDataHelper<AppConfig>.Config.ServerUrl, SearchText);
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(url);

                var repeater = doc.DocumentNode.SelectNodes("//div[@class='title']");
                if (repeater == null)
                {
                    MessageBox.Error(Lang.ResourceManager.GetString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    foreach (var node in repeater)
                    {
                        if (node.InnerText.Contains("OFFER POST")) continue;

                        var item = new SubsceneModel
                        {
                            Link = node.SelectSingleNode(".//a")?.Attributes["href"]?.Value +
                                   $"/{GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LanguageCode}/",
                            Name = node.InnerText.Trim()
                        };
                        DataList.Add(item);
                    }
                }

                IsBusy = false;
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

        #region Property

        private string _DefaultSubLang;

        public string DefaultSubLang
        {
            get => LocalizationManager.Instance
                .Localize(GlobalDataHelper<AppConfig>.Config.SubtitleLanguage.LocalizeCode).ToString();
            set => SetProperty(ref _DefaultSubLang, value);
        }

        private ObservableCollection<LanguageModel> _languageItems = new();

        public ObservableCollection<LanguageModel> LanguageItems
        {
            get => _languageItems;
            set => SetProperty(ref _languageItems, value);
        }

        private ObservableCollection<SubsceneModel> _dataList = new();

        public ObservableCollection<SubsceneModel> DataList
        {
            get => _dataList;
            set => SetProperty(ref _dataList, value);
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Command

        public DelegateCommand<SelectionChangedEventArgs> SubtitleLanguageCommand { get; }

        public DelegateCommand<SelectionChangedEventArgs> OpenSubtitlePageCommand { get; }
        #endregion
    }
}
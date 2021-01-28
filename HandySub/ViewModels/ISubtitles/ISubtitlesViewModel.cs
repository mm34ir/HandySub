using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Windows.Controls;
using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Data;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace HandySub.ViewModels
{
    public class ISubtitlesViewModel : BindableBase, IRegionMemberLifetime
    {
        private readonly IRegionManager _regionManager;

        public ISubtitlesViewModel(IRegionManager regionManager)
        {
            MainWindowViewModel.Instance.IsBackEnabled = false;

            DataList.Clear();
            GoToLinkCommand = new DelegateCommand<string>(GotoLink);
            _regionManager = regionManager;
            OnSearchStartedCommand = new DelegateCommand<FunctionEventArgs<string>>(OnSearchStarted);
            SubtitleLanguageCommand = new DelegateCommand<SelectionChangedEventArgs>(SubtitleLanguageChanged);
            LoadLanguage();
        }

        public bool KeepAlive => GlobalData.Config.IsKeepAlive;

        private void GotoLink(string name)
        {
            var parameters = new NavigationParameters
                {{"link_key", name}};
            _regionManager.RequestNavigate("ContentRegion", "ISubtitlesDownload", parameters);
        }

        private async void OnSearchStarted(FunctionEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(SearchText)) return;

            try
            {
                DataList?.Clear();
                IsBusy = true;

                //Get Title with imdb
                if (SearchText.StartsWith("tt")) SearchText = await Helper.GetTitleByImdbId(SearchText, errorCallBack);

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync($"https://isubtitles.org/search?kwd={SearchText}");

                var items = doc.DocumentNode.SelectNodes("//div[@class='movie-list-info']");
                var itemsName = doc.DocumentNode.SelectNodes("//div[@class='col-lg-18 col-md-16 col-sm-18']");
                if (items == null)
                {
                    MessageBox.Error(LocalizationManager.Instance.Localize("SubNotFound").ToString());
                }
                else
                {
                    var index = 0;
                    DataList?.Clear();
                    foreach (var node in items)
                    {
                        var src =
                            $"https://isubtitles.org{node?.SelectSingleNode(".//div/div")?.SelectSingleNode("img")?.Attributes["src"]?.Value}";
                        var name = FixName(itemsName[index].SelectSingleNode(".//h3/a")?.InnerText.Trim());
                        var page = FixPage(
                            $"https://isubtitles.org{itemsName[index].SelectSingleNode(".//h3/a")?.Attributes["href"]?.Value}");

                        if (string.IsNullOrEmpty(src) && string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        var item = new AvatarModel2
                        {
                            AvatarUri = src,
                            DisplayName = name,
                            SubtitlePage = page
                        };

                        DataList.Add(item);
                        index += 1;
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
                Growl.ErrorGlobal(
                    LocalizationManager.Instance.Localize("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(
                    LocalizationManager.Instance.Localize("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string FixName(string name)
        {
            string rem = "&#160";
            if (name.Contains(rem))
            {
                return name.Replace(rem, "");
            }
            else
            {
                return name;
            }
        }

        private string FixPage(string page)
        {
            string rem = "-subtitles";
            if (page.Contains(rem))
            {
                var lang = GlobalData.Config.SubtitleLanguage.LanguageCode.Replace("_", "-");
                return page.Replace(rem, $"/{lang}-subtitles");
            }
            else
            {
                return page;
            }
        }

        private void errorCallBack(string e)
        {
            Growl.ErrorGlobal(e);
        }

        public void LoadLanguage()
        {
            LanguageItems.Clear();
            DefaultSubLang = LocalizationManager.Instance
                .Localize(GlobalData.Config.SubtitleLanguage.LocalizeCode).ToString();
            LanguageItems.AddRange(SupportedLanguages.LoadSubtitleLanguage());
        }

        private void SubtitleLanguageChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            if (e.AddedItems[0] is LanguageModel item)
                if (!item.Equals(GlobalData.Config.SubtitleLanguage))
                {
                    GlobalData.Config.SubtitleLanguage = item;
                    GlobalData.Save();
                }
        }

        #region Property

        private string _DefaultSubLang;

        public string DefaultSubLang
        {
            get => LocalizationManager.Instance
                .Localize(GlobalData.Config.SubtitleLanguage.LocalizeCode).ToString();
            set => SetProperty(ref _DefaultSubLang, value);
        }

        private ObservableCollection<LanguageModel> _languageItems = new();

        public ObservableCollection<LanguageModel> LanguageItems
        {
            get => _languageItems;
            set => SetProperty(ref _languageItems, value);
        }

        private ObservableCollection<AvatarModel2> _dataList = new();

        public ObservableCollection<AvatarModel2> DataList
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

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Command

        public DelegateCommand<SelectionChangedEventArgs> SubtitleLanguageCommand { get; }
        public DelegateCommand<FunctionEventArgs<string>> OnSearchStartedCommand { get; }
        public DelegateCommand<string> GoToLinkCommand { get; }

        #endregion
    }
}
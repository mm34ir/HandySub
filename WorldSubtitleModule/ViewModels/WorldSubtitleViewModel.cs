using HandyControl.Controls;
using HandyControl.Data;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using WorldSubtitleModule.Model;
using MessageBox = HandyControl.Controls.MessageBox;

namespace WorldSubtitleModule.ViewModels
{
    public class WorldSubtitleViewModel : BindableBase
    {
        private HtmlDocument doc;
        private readonly string BasePageUrl = "http://worldsubtitle.info/page/{0}?s=";

        private readonly IRegionManager _regionManager;

        #region Property
        private string _ModuleName = LocalizationManager.Instance.Localize("WorldSub").ToString();
        public string ModuleName
        {
            get => LocalizationManager.Instance.Localize("WorldSub").ToString();
            set => SetProperty(ref _ModuleName, value);
        }
        private ObservableCollection<AvatarModel> _dataList = new ObservableCollection<AvatarModel>();
        public ObservableCollection<AvatarModel> DataList
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

        private int _MaxPageCount;
        public int MaxPageCount { get => _MaxPageCount; set => SetProperty(ref _MaxPageCount, value); }

        private Visibility _IsPaginationVisible = Visibility.Collapsed;
        public Visibility IsPaginationVisible { get => _IsPaginationVisible; set => SetProperty(ref _IsPaginationVisible, value); }
        #endregion

        #region Command
        public DelegateCommand<FunctionEventArgs<string>> OnSearchStartedCommand { get; private set; }
        public DelegateCommand<string> GoToLinkCommand { get; private set; }
        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand { get; private set; }
        #endregion

        public WorldSubtitleViewModel(IRegionManager regionManager)
        {
            DataList.Clear();
            GoToLinkCommand = new DelegateCommand<string>(GotoLink);
            _regionManager = regionManager;
            OnSearchStartedCommand = new DelegateCommand<FunctionEventArgs<string>>(OnSearchStarted);
            PageUpdatedCommand = new DelegateCommand<FunctionEventArgs<int>>(OnPageUpdated);
        }

        private async void OnPageUpdated(FunctionEventArgs<int> e)
        {
            await LoadData(string.Format(BasePageUrl, e.Info));
        }

        private void GotoLink(string name)
        {
            NavigationParameters parameters = new NavigationParameters
                    { { "name_key", name } };
            _regionManager.RequestNavigate("ContentRegion", "WorldSubtitleDownload", parameters);
        }

        private async Task<bool> LoadData(string Url = "http://worldsubtitle.info/?s=")
        {
            try
            {
                IsBusy = true;

                //Get Title with imdb
                if (SearchText.StartsWith("tt"))
                {
                    SearchText = await getTitleByImdbId(SearchText);
                }

                HtmlWeb web = new HtmlWeb();
                doc = await web.LoadFromWebAsync(Url + SearchText);

                HtmlNodeCollection items = doc.DocumentNode.SelectNodes("//div[@class='cat-post-tmp']");
                if (items == null)
                {
                    MessageBox.Error(LocalizationManager.Instance.Localize("SubNotFound").ToString());
                }
                else
                {
                    DataList?.Clear();
                    foreach (HtmlNode node in items)
                    {
                        // get link
                        string Link = node.SelectSingleNode(".//a").Attributes["href"].Value;

                        //get title
                        string Title = node.SelectSingleNode(".//a").Attributes["title"].Value;
                        string Img = node.SelectSingleNode(".//a/img")?.Attributes["src"].Value;

                        DataList.Add(new AvatarModel
                        {
                            DisplayName = Title,
                            AvatarUri = Img ?? "https://file.soft98.ir/uploads/mahdi72/2019/12/24_12-error.jpg",
                            SubtitlePage = Link,
                        });
                    }
                    IsBusy = false;
                    return true;
                }
            }
            catch (ArgumentOutOfRangeException) { }
            catch (ArgumentNullException) { }
            catch (System.NullReferenceException) { }
            catch (System.Net.WebException ex)
            {
                Growl.Error(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
            {
                Growl.Error(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
            return false;
        }

        private async void OnSearchStarted(FunctionEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return;
            }
            try
            {
                if (await LoadData())
                {
                    HtmlNodeCollection pagenavi = doc.DocumentNode.SelectNodes("//div[@class='wp-pagenavi']");
                    if (pagenavi != null)
                    {
                        HtmlNode getPageInfo = pagenavi[0].SelectSingleNode(".//span");
                        int getMaxPage = Convert.ToInt32(getPageInfo.InnerText.Substring(10, getPageInfo.InnerText.Length - 10));
                        IsPaginationVisible = Visibility.Visible;
                        MaxPageCount = getMaxPage;
                    }
                    else
                    {
                        IsPaginationVisible = Visibility.Collapsed;
                    }
                }
            }
            catch (NullReferenceException) { }
            catch (FormatException) { }
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
    }
}

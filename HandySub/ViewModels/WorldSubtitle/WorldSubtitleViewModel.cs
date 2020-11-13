using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace HandySub.ViewModels
{
    public class WorldSubtitleViewModel : BindableBase, IRegionMemberLifetime
    {
        public bool KeepAlive => GlobalDataHelper<AppConfig>.Config.IsKeepAlive;

        private HtmlDocument doc;
        private readonly string BasePageUrl = "http://worldsubtitle.info/page/{0}?s=";

        private readonly IRegionManager _regionManager;

        #region Property

        private ObservableCollection<AvatarModel2> _dataList = new ObservableCollection<AvatarModel2>();
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
            MainWindowViewModel.Instance.IsBackEnabled = false;

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
                    SearchText = await Helper.GetTitleByImdbId(SearchText, errorCallBack);
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

                        DataList.Add(new AvatarModel2
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
                Growl.ErrorGlobal(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
            {
                Growl.ErrorGlobal(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
            return false;
        }

        private void errorCallBack(string e)
        {
            Growl.ErrorGlobal(e);
        }

        private async void OnSearchStarted(FunctionEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return;
            }
            try
            {
                DataList?.Clear();

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
    }
}
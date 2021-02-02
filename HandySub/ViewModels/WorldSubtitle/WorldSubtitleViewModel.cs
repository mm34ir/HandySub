using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using MessageBox = HandyControl.Controls.MessageBox;

namespace HandySub.ViewModels
{
    public class WorldSubtitleViewModel : BindableBase, IRegionMemberLifetime
    {
        internal static WorldSubtitleViewModel Instance;
        private readonly IRegionManager _regionManager;
        private readonly string BasePageUrl = "http://worldsubtitle.info/page/{0}?s=";

        private HtmlDocument doc;

        public WorldSubtitleViewModel(IRegionManager regionManager)
        {
            Instance = this;
            MainWindowViewModel.Instance.IsBackEnabled = false;

            DataList.Clear();
            GoToLinkCommand = new DelegateCommand<string>(GotoLink);
            _regionManager = regionManager;
            PageUpdatedCommand = new DelegateCommand<FunctionEventArgs<int>>(OnPageUpdated);
        }

        public bool KeepAlive => GlobalData.Config.IsKeepAlive;

        private async void OnPageUpdated(FunctionEventArgs<int> e)
        {
            await LoadData(string.Format(BasePageUrl, e.Info));
        }

        private void GotoLink(string name)
        {
            var parameters = new NavigationParameters
                {{"name_key", name}};
            _regionManager.RequestNavigate("ContentRegion", "WorldSubtitleDownload", parameters);
        }

        private async Task<bool> LoadData(string Url = "http://worldsubtitle.info/?s=")
        {
            try
            {
                IsBusy = true;

                //Get Title with imdb
                if (SearchText.StartsWith("tt")) SearchText = await Helper.Current.GetTitleByImdbId(SearchText, errorCallBack);

                var web = new HtmlWeb();
                doc = await web.LoadFromWebAsync(Url + SearchText);

                var items = doc.DocumentNode.SelectNodes("//div[@class='cat-post-tmp']");
                if (items == null)
                {
                    MessageBox.Error(LocalizationManager.Instance.Localize("SubNotFound").ToString());
                }
                else
                {
                    DataList?.Clear();
                    foreach (var node in items)
                    {
                        // get link
                        var Link = node.SelectSingleNode(".//a").Attributes["href"].Value;

                        //get title
                        var Title = node.SelectSingleNode(".//a").Attributes["title"].Value;
                        var Img = node.SelectSingleNode(".//a/img")?.Attributes["data-src"].Value;
                        DataList.Add(new AvatarModel2
                        {
                            DisplayName = Title,
                            AvatarUri = Img ?? "https://file.soft98.ir/uploads/mahdi72/2019/12/24_12-error.jpg",
                            SubtitlePage = Link
                        });
                    }

                    IsBusy = false;
                    return true;
                }
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

            return false;
        }

        private void errorCallBack(string e)
        {
            Growl.ErrorGlobal(e);
        }

        public async void OnSearchStarted()
        {
            if (string.IsNullOrEmpty(SearchText)) return;

            try
            {
                DataList?.Clear();

                if (await LoadData())
                {
                    var pagenavi = doc.DocumentNode.SelectNodes("//div[@class='wp-pagenavi']");
                    if (pagenavi != null)
                    {
                        var getPageInfo = pagenavi[0].SelectSingleNode(".//span");
                        var getMaxPage =
                            Convert.ToInt32(getPageInfo.InnerText.Substring(10, getPageInfo.InnerText.Length - 10));
                        IsPaginationVisible = Visibility.Visible;
                        MaxPageCount = getMaxPage;
                    }
                    else
                    {
                        IsPaginationVisible = Visibility.Collapsed;
                    }
                }
            }
            catch (NullReferenceException)
            {
            }
            catch (FormatException)
            {
            }
        }

        #region Property

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

        private int _MaxPageCount;

        public int MaxPageCount
        {
            get => _MaxPageCount;
            set => SetProperty(ref _MaxPageCount, value);
        }

        private Visibility _IsPaginationVisible = Visibility.Collapsed;

        public Visibility IsPaginationVisible
        {
            get => _IsPaginationVisible;
            set => SetProperty(ref _IsPaginationVisible, value);
        }

        #endregion

        #region Command

        public DelegateCommand<string> GoToLinkCommand { get; }
        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand { get; }

        #endregion
    }
}
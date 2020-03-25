using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using SubtitleDownloader.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SubtitleDownloader.ViewModels
{
    public class PopularSeriesViewModel : BindableBase, INavigationAware
    {

        private readonly IRegionManager _regionManager;
        private readonly HttpClient client = new HttpClient();

        #region Property
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
            set
            {
                SetProperty(ref _searchText, value);
                ItemsView.Refresh();
            }
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        #endregion

        #region Command
        public DelegateCommand<string> GoToLinkCommand { get; private set; }
        #endregion

        public ICollectionView ItemsView => CollectionViewSource.GetDefaultView(DataList);

        public PopularSeriesViewModel()
        {

        }

        public PopularSeriesViewModel(IRegionManager regionManager)
        {
            DataList.Clear();
            GoToLinkCommand = new DelegateCommand<string>(GotoLink);
            ItemsView.SortDescriptions.Add(new SortDescription("DisplayName", ListSortDirection.Ascending));
            ItemsView.Filter = new Predicate<object>(o => Filter(o as AvatarModel));
            _regionManager = regionManager;
        }

        private void GotoLink(string name)
        {
            NavigationParameters parameters = new NavigationParameters
                    { { "name_key", name } };
            _regionManager.RequestNavigate("ContentRegion", "Subscene", parameters);
        }

        private bool Filter(AvatarModel item)
        {
            return SearchText == null
                || item.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1;
        }


        private async Task<dynamic> ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            string jsonResult = await client.GetStringAsync("https://raw.githubusercontent.com/ghost1372/SubtitlePopular/master/Popular.json");
            dynamic result = JsonConvert.DeserializeObject<dynamic>(jsonResult);

            return result;
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsBusy = true;

            try
            {
                dynamic repositories = await ProcessRepositories();

                foreach (dynamic item in repositories)
                {
                    DataList.Add(new AvatarModel { DisplayName = item.name, AvatarUri = item.poster_url });
                }
            }
            catch
            {
            }
            finally
            {
                IsBusy = false;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}

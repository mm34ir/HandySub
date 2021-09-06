using CommunityToolkit.WinUI.UI;
using HandySub.Common;
using HandySub.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HandySub.Pages
{
    public sealed partial class FavoritePage : Page, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<FavoriteKeyModel> _favorites = new ObservableCollection<FavoriteKeyModel>();
        public ObservableCollection<FavoriteKeyModel> Favorites
        {
            get { return _favorites; }
            set 
            { 
                _favorites = value;
                NotifyPropertyChanged(nameof(Favorites));
            }
        }
        #endregion

        internal static FavoritePage Instance;

        AdvancedCollectionView FavoritesACV;
        public FavoritePage()
        {
            this.InitializeComponent();
            Instance = this;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Favorites = await FavoriteHelper.LoadFavorites();
            FavoritesACV = new AdvancedCollectionView(Favorites, true);
            FavoritesACV.SortDescriptions.Add(new SortDescription("Title", SortDirection.Ascending));
            ShowEmptyNotify();
        }

        #region Search and Filter
        private void AutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            statusInfo.IsOpen = false;
            Filter();
        }
        private bool SubtitleFilter(object subtitle)
        {
            var query = subtitle as FavoriteKeyModel;
           
            var title = query.Title ?? "";
            var server = query.Server.ToString() ?? "";

            return title.Contains(AutoSuggest.Text, StringComparison.OrdinalIgnoreCase)
                    || server.Contains(AutoSuggest.Text, StringComparison.OrdinalIgnoreCase);
        }

        private void Filter()
        {
            FavoritesACV.Filter = _ => true;

            if (SubListView.Items.Count == 0)
                return;
            
            FavoritesACV.Filter = SubtitleFilter;

            if (SubListView.Items.Count > 0)
            {
                ShowStatus(string.Format(Constants.FoundedResult, SubListView.Items.Count), null, InfoBarSeverity.Success);
            }
            else
            {
                ShowStatus(Constants.NoResult, null, InfoBarSeverity.Warning);
            }
        }
        #endregion
        
        private void SubListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Helper.Settings.IsDoubleClickEnabled)
            {
                GoToDownloadPage();
            }
        }

        private void SubListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (Helper.Settings.IsDoubleClickEnabled)
            {
                if (Helper.IsInDoubleTapArea(e))
                {
                    GoToDownloadPage();
                }
            }
        }

        private void GoToDownloadPage()
        {
            var item = SubListView.SelectedItem as FavoriteKeyModel;
            Type _page = null;
            string _link = string.Empty;
            if (item != null)
            {
                _link = item.Value;
                switch (item.Server)
                {
                    case Server.ESubtitle:
                        _page = typeof(ESubtitleDownloadPage);
                        break;
                    case Server.ISubtitle:
                        _page = typeof(ISubtitleDownloadPage);
                        break;
                    case Server.WorldSubtitle:
                        _page = typeof(WorldSubtitleDownloadPage);
                        break;
                    case Server.Subscene:
                        _page = typeof(SubsceneDownloadPage);
                        _link = Helper.Settings.SubsceneServer.Url + item.Value;
                        break;
                }
                Frame.Navigate(_page, new NavigationParamModel { Key = (int)item.Server + item.Title, Title = item.Title, Link = _link }, new DrillInNavigationTransitionInfo());
            }
        }

        public void ShowEmptyNotify()
        {
            if (Favorites.Count == 0)
            {
                ShowStatus(null, "You have not yet added a movie/series to your favorites list, Search for a subtitle and add it to your favorites list.", InfoBarSeverity.Success);
            }
        }

        public void ShowStatus(string title, string message, InfoBarSeverity severity)
        {
            statusInfo.Title = title;
            statusInfo.Message = message;
            statusInfo.Severity = severity;
            statusInfo.IsOpen = true;
        }
    }
}

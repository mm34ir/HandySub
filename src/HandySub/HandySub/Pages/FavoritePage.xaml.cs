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
            Favorites = await Helper.LoadFavorites();
            FavoritesACV = new AdvancedCollectionView(Favorites, true);
            ShowEmptyNotify();
        }

        #region Search and Filter
        private void AutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            FavoritesACV.Filter = _ => true;

            if (string.IsNullOrEmpty(AutoSuggest.Text))
            {
                return;
            }

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
            if (SubListView.Items.Count == 0)
                return;
            
            FavoritesACV.Filter = SubtitleFilter;

            if (SubListView.Items.Count > 0)
            {
                ShowInfoBar("Info", $"We found {SubListView.Items.Count} subtitle(s)!", InfoBarSeverity.Success);
            }
            else
            {
                ShowInfoBar("Warning", $"No result found!", InfoBarSeverity.Warning);
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

        public void ShowInfoBar(string title, string message, InfoBarSeverity severity)
        {
            Helper.ShowInfoBar(Notify, title, message, severity);
        }
        public void ShowEmptyNotify()
        {
            if (Favorites.Count == 0)
            {
                ShowInfoBar("Info", "You have not yet added a movie/series to your favorites list, Search for a subtitle and add it to your favorites list.", InfoBarSeverity.Success);
            }
        }
    }
}

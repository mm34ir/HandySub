using Fluent.Icons;
using HandySub.Common;
using HandySub.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;

namespace HandySub
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow mainPage { get; set; }
        public MainWindow()
        {
            this.InitializeComponent();
            mainPage = this;
        }

        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Fav", typeof(FavoritePage)),
            ("Subscene", typeof(SubscenePage)),
            ("ESubtitle", typeof(ESubtitlePage)),
            ("WorldSubtitle", typeof(WorldSubtitlePage)),
            ("ISubtitle", typeof(ISubtitlePage)),
            ("IMDBPage", typeof(IMDBPage)),
            ("SubCompare", typeof(SubtitleDetails)),
        };

        public void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "Favorite",
                Icon = new FluentIconElement(FluentSymbol.ThumbLike24),
                Tag = "Fav"
            });
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "Subscene",
                Icon = new FluentIconElement(FluentSymbol.Home24),
                Tag = "Subscene"
            });
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "E Subtitle",
                Icon = new FluentIconElement(FluentSymbol.ClosedCaption24),
                Tag = "ESubtitle"
            });
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "World Subtitle",
                Icon = new FluentIconElement(FluentSymbol.Globe24),
                Tag = "WorldSubtitle"
            });
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "I Subtitle",
                Icon = new FluentIconElement(FluentSymbol.ClosedCaption24),
                Tag = "ISubtitle"
            });
            
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "Get IMDB Info",
                Icon = new FluentIconElement(FluentSymbol.MoviesandTV24),
                Tag = "IMDBPage"
            });
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "Compare Subtitles",
                Icon = new FluentIconElement(FluentSymbol.BranchCompare24),
                Tag = "SubCompare"
            });

            // Add handler for ContentFrame navigation.
            ContentFrame.Navigated += On_Navigated;

            // NavView doesn't load any page by default, so load home page.
            NavView.SelectedItem = NavView.MenuItems[0];
            // If navigation occurs on SelectionChanged, this isn't needed.
            // Because we use ItemInvoked to navigate, we need to call Navigate
            // here to load the home page.
            NavView_Navigate("Subscene", new EntranceNavigationTransitionInfo());
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;
            if (navItemTag == "settings")
            {
                _page = typeof(SettingsPage);
            }
            else
            {
                var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
                _page = item.Page;
            }
            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (!(_page is null) && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }
        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private bool TryGoBack()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                 NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }       

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
                NavView.Header = "Settings";
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .FirstOrDefault(n => n.Tag.Equals(item.Tag));

                NavView.Header =
                    ((NavigationViewItem)NavView.SelectedItem)?.Content?.ToString();
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ((sender as Grid).XamlRoot.Content as Grid).RequestedTheme = Helper.Settings.ApplicationTheme;
        }
    }
}

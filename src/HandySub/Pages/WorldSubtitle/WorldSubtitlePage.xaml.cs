using HandySub.Common;
using HandySub.Models;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Media.Animation;
using SettingsUI.Helpers;

namespace HandySub.Pages
{
    public sealed partial class WorldSubtitlePage : Page
    {
        internal static WorldSubtitlePage Instance;
        AutoSuggestBoxTextChangedEventArgs autoSuggestBoxTextChangedEventArgs;
        private ObservableCollection<SearchModel> _subtitles = new ObservableCollection<SearchModel>();
        public ObservableCollection<SearchModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }
        public WorldSubtitlePage()
        {
            this.InitializeComponent();
            Instance = this;
            if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
            {
                AutoSuggest.Text = App.StartUpArguments.Name;
                SearchSubtitle(AutoSuggest.Text);
            }
        }

        private void AutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            SearchSubtitle(args.QueryText);
        }

        private void AutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(AutoSuggest.Text))
                return;

            autoSuggestBoxTextChangedEventArgs = args;

            if (Helper.Settings.IsHistoryEnabled)
            {
                Helper.LoadHistory(sender, args);
            }
        }

        public void RefreshAutoSuggestTextChanged()
        {
            AutoSuggest_TextChanged(AutoSuggest, autoSuggestBoxTextChangedEventArgs);
        }
        private void GoToDownloadPage(SearchModel searchModel = null)
        {
            SearchModel item;
            if (searchModel != null)
            {
                item = searchModel;
            }
            else
            {
                item = SubListView.SelectedItem as SearchModel;
            }
            if (item != null)
            {
                Frame.Navigate(typeof(WorldSubtitleDownloadPage), new NavigationParamModel { Key = (int)Server.WorldSubtitle + item.Name, Title = item.Name, Link = item.Link }, new DrillInNavigationTransitionInfo());
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            SearchSubtitle(AutoSuggest.Text);
        }

        private void SubListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (Helper.Settings.IsDoubleClickEnabled)
            {
                if (Helper.IsInDoubleTapArea(e))
                {
                    GoToDownloadPage();
                }
            }
        }

        private void SubListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Helper.Settings.IsDoubleClickEnabled)
            {
                GoToDownloadPage();
            }
        }

        public async void SearchSubtitle(string queryText)
        {
            if (GeneralHelper.IsNetworkAvailable())
            {
                try
                {
                    errorInfo.IsOpen = false;
                    if (!string.IsNullOrEmpty(queryText))
                    {
                        if (Helper.Settings.IsHistoryEnabled)
                        {
                            Helper.AddToHistory(queryText);
                        }
                        progress.IsActive = true;
                        SubListView.Visibility = Visibility.Collapsed;
                        Subtitles.Clear();
                        if (queryText.StartsWith("tt"))
                            AutoSuggest.Text = await Helper.GetImdbIdFromTitle(queryText);

                        var url = string.Format(Constants.WorldSubtitleSearchAPI, queryText);
                        var web = new HtmlWeb();
                        var doc = await web.LoadFromWebAsync(url);

                        var items = doc.DocumentNode.SelectNodes("//div[@class='cat-post-tmp']");
                        var infoItems = doc.DocumentNode.SelectNodes("//div[@class='cat-post-info']");
                        if (items == null)
                        {
                            ShowError(Constants.NotFoundOrExist);
                        }
                        else
                        {
                            foreach (var node in items.GetEnumeratorWithIndex())
                            {
                                // get link
                                var Link = node.Value.SelectSingleNode(".//a").Attributes["href"]?.Value;

                                //get title
                                var Title = node.Value.SelectSingleNode(".//a").Attributes["title"]?.Value;
                                var Img = node.Value.SelectSingleNode(".//a/img")?.Attributes["src"]?.Value;
                                var date = infoItems[node.Index].SelectSingleNode("ul//li[1]");
                                var translator = infoItems[node.Index].SelectSingleNode("ul//li[3]");
                                var sync = infoItems[node.Index].SelectSingleNode("ul//li[5]");

                                foreach (var item in date.SelectNodes("b"))
                                {
                                    if (item.Name.ToLower() == "b")
                                    {
                                        date.RemoveChild(item);
                                    }
                                }

                                foreach (var item in translator.SelectNodes("b"))
                                {
                                    if (item.Name.ToLower() == "b")
                                    {
                                        translator.RemoveChild(item);
                                    }
                                }
                                foreach (var item in sync.SelectNodes("b"))
                                {
                                    if (item.Name.ToLower() == "b")
                                    {
                                        sync.RemoveChild(item);
                                    }
                                }

                                var desc = $"تاریخ ارسال: {date.InnerText.Trim()}{Environment.NewLine} مترجمان: {translator.InnerText.Trim()}{Environment.NewLine} هماهنگ با نسخه: {sync.InnerText.Trim()}";

                                Subtitles.Add(new SearchModel
                                {
                                    Name = Title,
                                    Poster = Img ?? "https://file.soft98.ir/uploads/mahdi72/2019/12/24_12-error.jpg",
                                    Link = Link,
                                    Desc = desc
                                });
                            }
                        }
                    }
                    progress.IsActive = false;
                    SubListView.Visibility = Visibility.Visible;
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
                    if (!string.IsNullOrEmpty(ex.Message))
                    {
                        ShowError(ex.Message);
                    }
                }
                catch (HttpRequestException hx)
                {
                    if (!string.IsNullOrEmpty(hx.Message))
                    {
                        ShowError(hx.Message);
                    }
                }
                finally
                {
                    progress.IsActive = false;
                    SubListView.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ShowError(Constants.InternetIsNotAvailable, Constants.InternetIsNotAvailableTitle);
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            var drop = await Helper.GridDrop(e);
            if (drop.IsDroped)
            {
                AutoSuggest.Text = drop.Name;
            }
        }

        private void ShowError(string message, string title = null)
        {
            errorInfo.Title = title;
            errorInfo.Message = message;
            errorInfo.IsOpen = true;
        }
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var item = VisualHelper.GetListViewItem<SearchModel>(sender as Button);
            GoToDownloadPage(item);
        }
    }
}

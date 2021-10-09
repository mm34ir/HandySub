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
    public sealed partial class ISubtitlePage : Page
    {
        internal static ISubtitlePage Instance {  get; set; }
        AutoSuggestBoxTextChangedEventArgs autoSuggestBoxTextChangedEventArgs;
        private ObservableCollection<SearchModel> _subtitles = new ObservableCollection<SearchModel>();
        public ObservableCollection<SearchModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }
        public ISubtitlePage()
        {
            this.InitializeComponent();
            Instance = this;
            if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
            {
                AutoSuggest.Text = App.StartUpArguments.Name;
                SearchSubtitle(AutoSuggest.Text);
            }
        }

        public async void SearchSubtitle(string queryText)
        {
            errorInfo.IsOpen = false;
            if (GeneralHelper.IsNetworkAvailable())
            {
                try
                {
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

                        var url = string.Format(Constants.ISubtitleSearchAPI, queryText);
                        var web = new HtmlWeb();
                        var doc = await web.LoadFromWebAsync(url);

                        var items = doc.DocumentNode.SelectNodes("//div[@class='movie-list-info']");
                        if (items == null)
                        {
                            ShowError(Constants.NotFoundOrExist);
                        }
                        else
                        {
                            foreach (var node in items)
                            {
                                var src = FixImg($"{Constants.ISubtitleBaseUrl}{node?.SelectSingleNode(".//div/div")?.SelectSingleNode("img")?.Attributes["src"]?.Value}");
                                var name = node?.SelectSingleNode(".//div/div[2]/h3/a");
                                var count = node?.SelectSingleNode(".//div/div[2]/div/div[3]/div/p[1]");
                                var date = node?.SelectSingleNode(".//div/div[2]/div/div[3]/div/p[3]");

                                var page = $"{Constants.ISubtitleBaseUrl}{name?.Attributes["href"]?.Value}";

                                var item = new SearchModel
                                {
                                    Poster = src,
                                    Name = Helper.GetDecodedStringFromHtml(name?.InnerText),
                                    Link = page,
                                    Desc = Helper.GetDecodedStringFromHtml(count?.InnerText.Trim() + Environment.NewLine + date?.InnerText.Trim())
                                };
                                if (!string.IsNullOrEmpty(item.Name))
                                {
                                    Subtitles.Add(item);
                                }
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
        private string FixImg(string img)
        {
            if (img.Contains(";"))
            {
                int index = img.IndexOf(";");
                return img.Substring(0, index);
            }
            else
            {
                return img;
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
                Frame.Navigate(typeof(ISubtitleDownloadPage), new NavigationParamModel { Key = (int)Server.ISubtitle + item.Name, Title = item.Name, Link = item.Link }, new DrillInNavigationTransitionInfo());
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
        private void ShowError(string message, string title = null)
        {
            errorInfo.Title = title;
            errorInfo.Message = message;
            errorInfo.IsOpen = true;
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

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var item = VisualHelper.GetListViewItem<SearchModel>(sender as Button);
            GoToDownloadPage(item);
        }
    }
}

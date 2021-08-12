using HandySub.Common;
using HandySub.Models;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net;
using ABI.Microsoft.UI.Xaml.Controls;
using SelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;
using AutoSuggestBoxTextChangedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs;
using AutoSuggestBox = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
using AutoSuggestBoxQuerySubmittedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs;
using Page = Microsoft.UI.Xaml.Controls.Page;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Media.Animation;

namespace HandySub.Pages
{
    public sealed partial class SubscenePage : Page
    {
        private ObservableCollection<SubsceneSearchModel> _subtitles = new ObservableCollection<SubsceneSearchModel>();
        public ObservableCollection<SubsceneSearchModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }

        public SubscenePage()
        {
            this.InitializeComponent();
        }

        private void AutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            SearchSubtitle(args.QueryText);
        }

        public async void SearchSubtitle(string queryText)
        {
            try
            {
                if (!string.IsNullOrEmpty(queryText))
                {
                    Helper.AddToHistory(queryText);
                    InfoError.IsOpen = false;
                    progress.IsActive = true;
                    SubListView.Visibility = Visibility.Collapsed;
                    Subtitles.Clear();
                    if (queryText.StartsWith("tt"))
                        AutoSuggest.Text = await Helper.GetImdbIdFromTitle(queryText);

                    var url = string.Format(Consts.SubsceneSearchAPI, Helper.Settings.SubsceneServer.Url, queryText);
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(url);

                    var titleCollection = doc.DocumentNode.SelectSingleNode("//div[@class='search-result']");
                    if (titleCollection == null || titleCollection.InnerText.Contains("No results found"))
                    {
                        ShowInfoBar("Subtitle not found or server is unavailable.");
                    }
                    else
                    {
                        for (int i = 1; i < 4; i++)
                        {
                            var node = titleCollection.SelectSingleNode($"ul[{i}]");
                            foreach (var item in node.SelectNodes("li"))
                            {
                                var subNode = item?.SelectSingleNode("div//a");
                                var count = item.SelectSingleNode("span");
                                if (count == null)
                                {
                                    count = item.SelectSingleNode("div[@class='subtle count']");
                                }

                                var name = subNode?.InnerText.Trim();
                                var subtitle = new SubsceneSearchModel
                                {
                                    Name = name,
                                    Link = subNode?.Attributes["href"]?.Value.Trim(),
                                    Desc = count?.InnerText.Trim(),
                                    Key = GetSubtitleKey(i)
                                };
                                Subtitles.Add(subtitle);
                            }
                        }
                    }
                }
                progress.IsActive = false;
                SubListView.Visibility = Visibility.Visible;
                var groups = from c in Subtitles
                             group c by c.Key;
                SubtitleCVS.Source = groups;
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
                    ShowInfoBar(ex.Message);
                }
            }
            catch (HttpRequestException hx)
            {
                if (!string.IsNullOrEmpty(hx.Message))
                {
                    ShowInfoBar(hx.Message);
                }
            }
            finally
            {
                progress.IsActive = false;
                SubListView.Visibility = Visibility.Visible;
            }
        }
        private string GetSubtitleKey(int index)
        {
            switch (index)
            {
                case 1:
                    return "TVSeries";
                case 2:
                    return "Close";
                case 3:
                    return "Popular";
            }
            return null;
        }
        private void AutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(AutoSuggest.Text))
                return;

            Helper.LoadHistory(sender, args, AutoSuggest);
        }

        private void SubListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Helper.Settings.IsDoubleClickEnabled)
            {
                GoToDownloadPage();
            }
        }
        private void GoToDownloadPage()
        {
            var item = SubListView.SelectedItem as SubsceneSearchModel;
            if (item != null)
            {
                Frame.Navigate(typeof(SubsceneDownloadPage), new DownloadModel { DisplayName = (int)Server.Subscene + item.Name, DownloadLink = Helper.Settings.SubsceneServer.Url + item.Link }, new DrillInNavigationTransitionInfo());
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            InfoError.IsOpen = false;

            SearchSubtitle(AutoSuggest.Text);
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

        private void ShowInfoBar(string message)
        {
            Helper.ShowErrorInfoBar(InfoError, message);
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
    }
}

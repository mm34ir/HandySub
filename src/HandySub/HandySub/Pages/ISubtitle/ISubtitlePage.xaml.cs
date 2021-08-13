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

namespace HandySub.Pages
{
    public sealed partial class ISubtitlePage : Page
    {
        private ObservableCollection<SearchModel> _subtitles = new ObservableCollection<SearchModel>();
        public ObservableCollection<SearchModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }
        public ISubtitlePage()
        {
            this.InitializeComponent();

            if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
            {
                AutoSuggest.Text = App.StartUpArguments.Name;
                SearchSubtitle(AutoSuggest.Text);
            }
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

                    var url = string.Format(Consts.ISubtitleSearchAPI, queryText);
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(url);

                    var items = doc.DocumentNode.SelectNodes("//div[@class='movie-list-info']");
                    if (items == null)
                    {
                        ShowInfoBar("Subtitle not found or server is unavailable.");
                    }
                    else
                    {
                        foreach (var node in items)
                        {
                            var src = FixImg($"{Consts.ISubtitleBaseUrl}{node?.SelectSingleNode(".//div/div")?.SelectSingleNode("img")?.Attributes["src"]?.Value}");
                            var name = node?.SelectSingleNode(".//div/div[2]/h3/a");
                            var count = node?.SelectSingleNode(".//div/div[2]/div/div[3]/div/p[1]");
                            var date = node?.SelectSingleNode(".//div/div[2]/div/div[3]/div/p[3]");

                            var page = $"{Consts.ISubtitleBaseUrl}{name?.Attributes["href"]?.Value}";

                            var item = new SearchModel
                            {
                                Poster = src,
                                Name = FixName(name?.InnerText),
                                Link = page,
                                Desc = count?.InnerText.Trim() + Environment.NewLine + date?.InnerText.Trim()
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

        private string FixName(string name)
        {
            string rem = "&#160";
            if (!string.IsNullOrEmpty(name) && name.Contains(rem))
            {
                return name.Replace(rem, "");
            }
            else
            {
                return name;
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

            Helper.LoadHistory(sender, args, AutoSuggest);
        }

        private void GoToDownloadPage()
        {
            var item = SubListView.SelectedItem as SearchModel;
            if (item != null)
            {
                Frame.Navigate(typeof(ISubtitleDownloadPage), new NavigationParamModel { Key = (int)Server.ISubtitle + item.Name, Title = item.Name, Link = item.Link }, new DrillInNavigationTransitionInfo());
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            InfoError.IsOpen = false;

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

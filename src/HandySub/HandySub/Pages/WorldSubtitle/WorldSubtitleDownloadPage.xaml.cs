using HandySub.Common;
using HandySub.Models;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;

namespace HandySub.Pages
{
    public sealed partial class WorldSubtitleDownloadPage : Page
    {
        internal static WorldSubtitleDownloadPage Instance;

        private ObservableCollection<DownloadModel> _subtitles = new ObservableCollection<DownloadModel>();
        public ObservableCollection<DownloadModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }

        private string subtitleUrl = string.Empty;
        private string subtitleTitle = string.Empty;
        private string subtitleKey = string.Empty;
        public WorldSubtitleDownloadPage()
        {
            this.InitializeComponent();
            Instance = this;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var param = (NavigationParamModel)e?.Parameter;
            if (param != null)
            {
                subtitleUrl = param.Link;
                subtitleKey = param.Key;
                subtitleTitle = param.Title;
                txtTitle.Text = param.Title;
                if (await FavoriteHelper.IsFavoriteExist(subtitleKey))
                {
                    Favorite.Value = 1;
                }
            }

            if (!string.IsNullOrEmpty(subtitleUrl))
            {
                GetSubtitle();
            }
        }

        private async void GetSubtitle()
        {
            progress.IsActive = true;
            listView.Visibility = Visibility.Collapsed;
            CloseError();
            if (Helper.IsNetworkAvailable())
            {
                try
                {
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(subtitleUrl);

                    var items = doc.DocumentNode.SelectNodes(@"//div[@id='new-link']/ul/li");
                    if (items == null)
                    {
                        ShowError(Constants.NotFoundOrExist);
                    }
                    else
                    {
                        Subtitles?.Clear();
                        foreach (var node in items)
                        {
                            var displayName = node.SelectSingleNode(".//div[@class='new-link-1']").InnerText;
                            var status = node.SelectSingleNode(".//div[@class='new-link-2']").InnerText;
                            var link = node.SelectSingleNode(".//a")?.Attributes["href"]?.Value;

                            if (status.Contains("&nbsp;")) status = status.Replace("&nbsp;", "");

                            displayName = displayName.Trim() + " - " + status.Trim();

                            var item = new DownloadModel
                            {
                                DisplayName = displayName,
                                DownloadLink = link
                            };
                            Subtitles.Add(item);
                        }
                    }
                    progress.IsActive = false;
                    listView.Visibility = Visibility.Visible;
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
                    listView.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ShowError(Constants.InternetIsNotAvailable, Constants.InternetIsNotAvailableTitle);
            }
        }

        public void ShowError(string message, string title = null)
        {
            errorInfo.Title = title;
            errorInfo.Message = message;
            errorInfo.IsOpen = true;
        }

        public void CloseError()
        {
            errorInfo.IsOpen = false;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetSubtitle();
        }

        private void Favorite_ValueChanged(RatingControl sender, object args)
        {
            FavoriteHelper.AddToFavorite(Favorite.Value, new FavoriteKeyModel { Key = subtitleKey, Title = subtitleTitle, Value = subtitleUrl, Server = Server.WorldSubtitle });
        }
    }
}

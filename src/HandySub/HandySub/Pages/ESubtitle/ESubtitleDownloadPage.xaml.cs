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
    public sealed partial class ESubtitleDownloadPage : Page
    {
        internal static ESubtitleDownloadPage Instance;

        private ObservableCollection<DownloadModel> _subtitles = new ObservableCollection<DownloadModel>();
        public ObservableCollection<DownloadModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }
        private string subtitleUrl = string.Empty;
        private string subtitleTitle = string.Empty;
        private string subtitleKey = string.Empty;
        public ESubtitleDownloadPage()
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

                if (await Helper.IsFavoriteExist(subtitleKey))
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
            Notify.IsOpen = false;

            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var items = doc.DocumentNode.SelectNodes("//a[@class='Download']");
                if (items == null)
                {
                    ShowInfoBar("Error", "Subtitles not found or server is unavailable.", InfoBarSeverity.Error);
                }
                else
                {
                    Subtitles?.Clear();
                    foreach (var node in items)
                    {
                        var displayName = node.SelectSingleNode(".//span[last()]").InnerText;
                        var downloadLink = node.Attributes["href"].Value;
                        if (!displayName.Contains("جهت حمایت از ما کلیک کنید"))
                        {
                            var item = new DownloadModel
                            {
                                DisplayName = displayName,
                                DownloadLink = downloadLink
                            };
                            Subtitles.Add(item);
                        }
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
                    ShowInfoBar("Error", ex.Message, InfoBarSeverity.Error);
                }
            }
            catch (HttpRequestException hx)
            {
                if (!string.IsNullOrEmpty(hx.Message))
                {
                    ShowInfoBar("Error", hx.Message, InfoBarSeverity.Error);
                }
            }
            finally
            {
                progress.IsActive = false;
                listView.Visibility = Visibility.Visible;
            }
        }

        public void ShowInfoBar(string title, string message, InfoBarSeverity severity)
        {
            Helper.ShowInfoBar(Notify, title, message, severity);
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetSubtitle();
        }
        private void Favorite_ValueChanged(RatingControl sender, object args)
        {
            Helper.AddToFavorite(Favorite.Value, new FavoriteKeyModel { Key = subtitleKey, Title = subtitleTitle, Value = subtitleUrl, Server = Server.ESubtitle });
        }
    }
}

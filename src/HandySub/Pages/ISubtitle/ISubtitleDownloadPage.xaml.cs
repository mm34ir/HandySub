using CommunityToolkit.WinUI.UI;
using HandySub.Common;
using HandySub.Models;
using HtmlAgilityPack;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using Microsoft.UI.Xaml;
using SettingsUI.Helpers;

namespace HandySub.Pages
{
    public sealed partial class ISubtitleDownloadPage : Page
    {
        internal static ISubtitleDownloadPage Instance {  get; set; }

        private ObservableCollection<SubsceneDownloadModel> _subtitles = new ObservableCollection<SubsceneDownloadModel>();
        public ObservableCollection<SubsceneDownloadModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }

        AdvancedCollectionView SubtitlesACV;
        private string subtitleUrl = string.Empty;
        private string subtitleTitle = string.Empty;
        private string subtitleKey = string.Empty;
        public ISubtitleDownloadPage()
        {
            this.InitializeComponent();
            Instance = this;
            SubtitlesACV = new AdvancedCollectionView(Subtitles, true);

            cmbLanguage.SelectedItem = Helper.Settings.SubtitleLanguage;
            cmbQuaity.SelectedItem = Helper.Settings.SubtitleQuality;
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
            CloseStatus();
            if (GeneralHelper.IsNetworkAvailable())
            {
                try
                {
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(subtitleUrl);
                    var items = doc.DocumentNode.SelectSingleNode("//table");
                    if (items == null)
                    {
                        ShowStatus(Constants.NotFoundOrExist, null, InfoBarSeverity.Error);
                    }
                    else
                    {
                        Subtitles?.Clear();
                        var movieData = items.SelectNodes("//td[@data-title='Release / Movie']");
                        var commentData = items.SelectNodes("//td[@data-title='Comment']");
                        var language = items.SelectNodes("//td[@data-title='Language']");
                        if (movieData != null)
                        {
                            string title = string.Empty;
                            string href = string.Empty;
                            string comment = string.Empty;
                            foreach (var row in movieData.GetEnumeratorWithIndex())
                            {
                                var currentRow = row.Value?.SelectNodes("a");
                                foreach (var cell in currentRow)
                                {
                                    title = cell?.InnerText?.Trim();
                                    href = $"{Constants.ISubtitleBaseUrl}{cell?.Attributes["href"]?.Value?.Trim()}";
                                }

                                comment = commentData[row.Index]?.InnerText?.Trim();
                                if (comment != null && comment.Contains("&nbsp;"))
                                    comment = comment.Replace("&nbsp;", "");

                                if (!string.IsNullOrEmpty(title))
                                {
                                    var item = new SubsceneDownloadModel
                                    {
                                        Name = Helper.GetDecodedStringFromHtml(title),
                                        Translator = Helper.GetDecodedStringFromHtml(comment),
                                        Link = href,
                                        Language = language[row.Index]?.InnerText.Trim()
                                    };

                                    Subtitles.Add(item);
                                }
                            }
                        }
                    }
                    progress.IsActive = false;
                    listView.Visibility = Visibility.Visible;
                }
                catch (ArgumentNullException)
                {
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                catch (WebException ex)
                {
                    if (!string.IsNullOrEmpty(ex.Message))
                    {
                        ShowStatus(null, ex.Message, InfoBarSeverity.Error);
                    }
                }
                catch (HttpRequestException hx)
                {
                    if (!string.IsNullOrEmpty(hx.Message))
                    {
                        ShowStatus(null, hx.Message, InfoBarSeverity.Error);
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
                ShowStatus(Constants.InternetIsNotAvailableTitle, Constants.InternetIsNotAvailable, InfoBarSeverity.Error);
            }
        }
        public void ShowStatus(string title, string message, InfoBarSeverity severity)
        {
            statusInfo.Title = title;
            statusInfo.Message = message;
            statusInfo.Severity = severity;
            statusInfo.IsOpen = true;
        }

        public void CloseStatus()
        {
            statusInfo.IsOpen = false;
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetSubtitle();
        }

        private void Favorite_ValueChanged(RatingControl sender, object args)
        {
            FavoriteHelper.AddToFavorite(Favorite.Value, new FavoriteKeyModel { Key = subtitleKey, Title = subtitleTitle, Value = subtitleUrl, Server = Server.ISubtitle });
        }

        #region Search and Filter
        private bool SubtitleFilter(object subtitle)
        {
            var query = subtitle as SubsceneDownloadModel;
            var selectedLanguage = cmbLanguage.SelectedItem as string;
            var selectedQuality = cmbQuaity.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedQuality) || string.IsNullOrEmpty(selectedLanguage))
                return false;

            var name = query.Name ?? "";
            var translator = query.Translator ?? "";
            var comment = query.Comment ?? "";
            var language = query.Language ?? "";


            if (selectedLanguage.Equals(Constants.ALL_Language))
                selectedLanguage = "";

            if (selectedQuality.Equals(Constants.ALL_Qualities))
                selectedQuality = "";

            var episode = $"E{nbEpisode.Value.ToString("00")}";
            if (double.IsNaN(nbEpisode.Value) || nbEpisode.Value == 0)
                episode = "";

            return (name.Contains(selectedQuality, StringComparison.OrdinalIgnoreCase)
                    || translator.Contains(selectedQuality, StringComparison.OrdinalIgnoreCase)
                    || comment.Contains(selectedQuality, StringComparison.OrdinalIgnoreCase))
                    && language.Contains(selectedLanguage, StringComparison.OrdinalIgnoreCase)
                    && (name.Contains(AutoSuggest.Text, StringComparison.OrdinalIgnoreCase)
                    || translator.Contains(AutoSuggest.Text, StringComparison.OrdinalIgnoreCase)
                    || comment.Contains(AutoSuggest.Text, StringComparison.OrdinalIgnoreCase))
                    && (name.Contains(episode, StringComparison.OrdinalIgnoreCase)
                    || translator.Contains(episode, StringComparison.OrdinalIgnoreCase)
                    || comment.Contains(episode, StringComparison.OrdinalIgnoreCase));
        }
        private void Filter()
        {
            SubtitlesACV.Filter = _ => true;
            SubtitlesACV.Filter = SubtitleFilter;

            if (listView.Items.Count > 0)
            {
                ShowStatus(string.Format(Constants.FoundedResult, listView.Items.Count), null, InfoBarSeverity.Success);
            }
            else
            {
                ShowStatus(Constants.NoResult, null, InfoBarSeverity.Warning);
            }
        }
        private void AutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            CloseStatus();
            Filter();
        }
        private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLanguage = cmbLanguage.SelectedItem as string;

            if (selectedLanguage != Helper.Settings.SubtitleLanguage)
            {
                Helper.Settings.SubtitleLanguage = selectedLanguage;
            }
            Filter();
            if (progress.IsActive)
            {
                CloseStatus();
            }
        }
        private void cmbQuaity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedQuality = cmbQuaity.SelectedItem as string;

            if (selectedQuality != Helper.Settings.SubtitleQuality)
            {
                Helper.Settings.SubtitleQuality = selectedQuality;
            }
            Filter();
            if (progress.IsActive)
            {
                CloseStatus();
            }
        }
        private void nbEpisode_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (SubtitlesACV == null)
                return;

            if (double.IsNaN(nbEpisode.Value))
                nbEpisode.Value = 0;

            Filter();
        }
        #endregion

        private void listView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            foreach (var item in VisualHelper.FindVisualChildren<Button>(this))
            {
                if (item.Name == "DownloadButton")
                {
                    item.Visibility = Visibility.Visible;
                    item.IsEnabled = true;
                }
            }
        }
    }
}

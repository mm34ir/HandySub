using HandySub.Common;
using HandySub.Models;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI;
using SettingsUI.Helpers;

namespace HandySub.Pages
{
    public sealed partial class SubsceneDownloadPage : Page
    {
        internal static SubsceneDownloadPage Instance {  get; set; }
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
        public SubsceneDownloadPage()
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

        private void GetSubtitle()
        {
            progress.IsActive = true;
            listView.Visibility = Visibility.Collapsed;
            CloseStatus();

            if (GeneralHelper.IsNetworkAvailable())
            {
                if (Helper.Settings.SubsceneServer.Url.Contains("subf2m"))
                    Subf2mCore();
                else
                    SubsceneCore();
            }
            else
            {
                ShowStatus(Constants.InternetIsNotAvailableTitle, Constants.InternetIsNotAvailable, InfoBarSeverity.Error);
            }
        }

        private async void SubsceneCore()
        {
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var table = doc.DocumentNode.SelectSingleNode("//table[1]//tbody");
                if (table != null)
                {
                    Subtitles?.Clear();

                    foreach (var cell in table.SelectNodes(".//tr"))
                    {
                        if (cell.InnerText.Contains("There are no subtitles"))
                            break;

                        var Language = cell.SelectSingleNode(".//td[@class='a1']//a//span[1]")?.InnerText.Trim();
                        var Name = cell.SelectSingleNode(".//td[@class='a1']//a//span[2]")?.InnerText.Trim();
                        var Translator = cell.SelectSingleNode(".//td[@class='a5']//a")?.InnerText.Trim();
                        var Comment = cell.SelectSingleNode(".//td[@class='a6']//div")?.InnerText.Trim();
                        if (Comment != null && Comment.Contains("&nbsp;")) Comment = Comment.Replace("&nbsp;", "");

                        var Link = cell.SelectSingleNode(".//td[@class='a1']//a")?.Attributes["href"]?.Value.Trim();

                        if (Name != null)
                        {
                            var item = new SubsceneDownloadModel
                            {
                                Name = Name,
                                Title = Name,
                                Translator = Translator,
                                Comment = Comment,
                                Link = Link,
                                Language = Language
                            };
                            Subtitles.Add(item);
                        }
                    }

                    SetPosterAndIMDB(doc);
                }
                else
                {
                    ShowStatus(Constants.NotFoundOrExist, null, InfoBarSeverity.Error);
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
        private async void Subf2mCore()
        {
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var repeater = doc.DocumentNode.SelectNodes("//ul[@class='scrolllist']");

                if (repeater == null)
                {
                    ShowStatus(Constants.NotFoundOrExist, null, InfoBarSeverity.Error);
                }
                else
                {
                    Subtitles?.Clear();
                    foreach (var node in repeater.GetEnumeratorWithIndex())
                    {
                        var language = node.Value.SelectNodes("//div[@class='topright']//span[1]")[node.Index].InnerText;
                        var translator = node.Value.SelectNodes("//div[@class='comment-col']")[node.Index].InnerText;
                        var download_Link = node.Value.SelectNodes("//a[@class='download icon-download']")[node.Index].GetAttributeValue("href", "");

                        //remove empty lines
                        var singleLineTranslator = Regex.Replace(translator, @"\s+", " ").Replace("&nbsp;", "");
                        if (singleLineTranslator.Contains("&nbsp;"))
                            singleLineTranslator = singleLineTranslator.Replace("&nbsp;", "");

                        string _name = node.Value.InnerText.Trim();
                        int MaxLength = 100;
                        _name = _name.Length > MaxLength ? _name.Substring(0, MaxLength) : _name;

                        var item = new SubsceneDownloadModel
                        {
                            Name = node.Value.InnerText.Trim(),
                            Title = _name,
                            Translator = singleLineTranslator.Trim(),
                            Link = download_Link.Trim(),
                            Language = language
                        };
                        Subtitles.Add(item);
                    }
                    SetPosterAndIMDB(doc);
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

        private void SetPosterAndIMDB(HtmlDocument doc)
        {
            //var divPoster = doc?.DocumentNode?.SelectSingleNode("//div[@class='poster']");
            //var posterSource = divPoster?.SelectSingleNode("//img")?.Attributes["src"]?.Value;
            //if (!string.IsNullOrEmpty(posterSource))
            //{
            //    poster.Source = new BitmapImage(new Uri(posterSource));
            //}

            var imdbTag = doc?.DocumentNode?.SelectSingleNode("//a[@class='imdb']");
            var imdbHref = imdbTag?.Attributes["href"]?.Value;
            if (!string.IsNullOrEmpty(imdbHref))
            {
                imdbLink.NavigateUri = new Uri(imdbHref);
                imdbLink.Visibility = Visibility.Visible;
            }

            var yearTag = doc?.DocumentNode?.SelectSingleNode("//div[@class='header']//li")?.InnerText;
            if (!string.IsNullOrEmpty(yearTag))
            {
                txtYear.Text = yearTag.Replace("Year:", "").Trim();
                stackYear.Visibility = Visibility.Visible;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetSubtitle();
        }

        private void Favorite_ValueChanged(RatingControl sender, object args)
        {
            FavoriteHelper.AddToFavorite(Favorite.Value, new FavoriteKeyModel { Key = subtitleKey, Title = subtitleTitle, Value = subtitleUrl.Replace(Helper.Settings.SubsceneServer.Url, ""), Server = Server.Subscene });
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

        #region TeachingTip
        public void ShowTip1()
        {
            tip1.IsOpen = true;
        }

        private void tip1_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip1.IsOpen = false;
            tip2.IsOpen = true;
        }

        private void tip2_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip2.IsOpen = false;
            tip3.IsOpen = true;
        }

        private void tip3_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip3.IsOpen = false;
            tip4.IsOpen = true;
        }

        private void tip4_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip4.IsOpen = false;
            tip5.IsOpen = true;
        }

        private void tip5_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip5.IsOpen = false;
            tip6.IsOpen = true;
        }

        private void tip6_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip6.IsOpen = false;
            tip7.IsOpen = true;
        }

        private void tip7_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip7.IsOpen = false;
            tip8.IsOpen = true;
        }

        private void tip8_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            tip8.IsOpen = false;
            Helper.Settings.IsFirstRun = false;
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

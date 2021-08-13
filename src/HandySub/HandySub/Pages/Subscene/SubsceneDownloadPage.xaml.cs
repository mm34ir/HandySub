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
using AutoSuggestBoxTextChangedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs;
using AutoSuggestBox = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
using SelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;
using Page = Microsoft.UI.Xaml.Controls.Page;

namespace HandySub.Pages
{
    public sealed partial class SubsceneDownloadPage : Page
    {
        internal static SubsceneDownloadPage Instance;
        private ObservableCollection<SubsceneDownloadModel> _subtitles = new ObservableCollection<SubsceneDownloadModel>();
        public ObservableCollection<SubsceneDownloadModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }

        AdvancedCollectionView SubtitlesACV;
        private string subtitleUrl = string.Empty;
        private string subtitleDisplayName = string.Empty;
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
            var param = (DownloadModel)e?.Parameter;
            if (param != null)
            {
                subtitleUrl = param.DownloadLink;
                subtitleDisplayName = param.DisplayName;

                if (await Helper.IsFavoriteExist(subtitleDisplayName))
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
            Notify.IsOpen = false;
            if (Helper.Settings.SubsceneServer.Url.Contains("subf2m"))
                Subf2mCore();
            else
                SubsceneCore();
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
                                Translator = Translator,
                                Comment = Comment,
                                Link = Link,
                                Language = Language
                            };
                            Subtitles.Add(item);
                        }
                    }
                }
                else
                {
                    ShowInfoBar("Error", "Subtitles not found or server is unavailable.", InfoBarSeverity.Error);
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

        private async void Subf2mCore()
        {
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var repeater = doc.DocumentNode.SelectNodes("//ul[@class='scrolllist']");

                if (repeater == null)
                {
                    ShowInfoBar("Error", "Subtitles not found or server is unavailable.", InfoBarSeverity.Error);
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

                        var item = new SubsceneDownloadModel
                        {
                            Name = node.Value.InnerText.Trim(),
                            Translator = singleLineTranslator.Trim(),
                            Link = download_Link.Trim(),
                            Language = language
                        };
                        Subtitles.Add(item);
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

        private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLanguage = cmbLanguage.SelectedItem as string;

            if (selectedLanguage != Helper.Settings.SubtitleLanguage)
            {
                Helper.Settings.SubtitleLanguage = selectedLanguage;
            }
            SubtitlesACV.Filter = _ => true;
            Filter();
            if (progress.IsActive)
            {
                Notify.IsOpen = false;
            }
        }

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

            if (selectedLanguage.Equals(Consts.ALL_Language))
                selectedLanguage = "";
            
            if (selectedQuality.Equals(Consts.ALL_Qualities))
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
            SubtitlesACV.Filter = SubtitleFilter;

            if (listView.Items.Count > 0)
            {
                ShowInfoBar("Info", $"We found {listView.Items.Count} subtitle(s)!", InfoBarSeverity.Success);
            }
            else
            {
                ShowInfoBar("Warning", $"No result found!", InfoBarSeverity.Warning);
            }
        }

        private void AutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            SubtitlesACV.Filter = _ => true;

            if (string.IsNullOrEmpty(AutoSuggest.Text))
            {
                return;
            }

            Filter();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetSubtitle();
        }

        private void cmbQuaity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedQuality = cmbQuaity.SelectedItem as string;

            if (selectedQuality != Helper.Settings.SubtitleQuality)
            {
                Helper.Settings.SubtitleQuality = selectedQuality;
            }
            SubtitlesACV.Filter = _ => true;
            Filter();
            if (progress.IsActive)
            {
                Notify.IsOpen = false;
            }
        }

        private void Favorite_ValueChanged(RatingControl sender, object args)
        {
            Helper.AddToFavorite(Favorite.Value, new FavoriteKeyModel { Key = subtitleDisplayName, Title = subtitleDisplayName.Remove(0, 1), Value = subtitleUrl.Replace(Helper.Settings.SubsceneServer.Url,""), Server = Server.Subscene });
        }

        private void nbEpisode_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {   
            if (SubtitlesACV == null)
                return;

            if (double.IsNaN(nbEpisode.Value))
                nbEpisode.Value = 0;

            SubtitlesACV.Filter = _ => true;
            Filter();
        }
    }
}

using Downloader;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using HandySub.Assets;
using HandySub.Models;
using Microsoft.AppCenter.Utils.Files;
using Microsoft.Win32;
using ModernWpf.Controls;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for GetMovieInfo.xaml
    /// </summary>
    public partial class GetMovieInfo : UserControl
    {
        public GetMovieInfo()
        {
            InitializeComponent();
        }

        private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Helper.LoadHistory(sender, args, autoBox);
        }

        private void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                Helper.AddHistory(args.QueryText);
                OnSearchStarted();
            }
        }

        private async void OnSearchStarted()
        {
            if (string.IsNullOrEmpty(autoBox.Text))
                return;

            tgBlock.IsChecked = false;
            var url = string.Empty;
            url = autoBox.Text.StartsWith("tt")
                ? $"http://www.omdbapi.com/?i={autoBox.Text}&apikey=2a59a17e"
                : $"http://www.omdbapi.com/?t={autoBox.Text}&apikey=2a59a17e";

            try
            {
                using var client = new HttpClient();
                var responseBody = await client.GetStringAsync(url);
                var parse = JsonSerializer.Deserialize<IMDBModel>(responseBody);
                if (parse.Response.Equals("True"))
                {
                    txtImdbId.Text = "https://www.imdb.com/title/{0}".Format(parse.imdbID);
                    rate.Value = Convert.ToDouble(parse.imdbRating, CultureInfo.InvariantCulture);
                    txtTitle.Text = parse.Title;
                    txtYear.Text = parse.Year;
                    txtReleased.Text = parse.Released;
                    txtType.Text = parse.Type;
                    txtTotalSeason.Text = parse.totalSeasons;
                    txtLanguage.Text = parse.Language;
                    txtCountry.Text = parse.Country;
                    txtRated.Text = parse.Rated;
                    txtGenre.Text = parse.Genre;
                    txtDirector.Text = parse.Director;
                    txtWriter.Text = parse.Writer;
                    txtActors.Text = parse.Actors;
                    txtPlot.Text = parse.Plot;
                    src.Source = new BitmapImage(new Uri(parse.Poster));

                    grd.Visibility = System.Windows.Visibility.Visible;
                    tgBlock.IsChecked = true;
                }
                else
                {
                    grd.Visibility = System.Windows.Visibility.Collapsed;
                    Growl.ErrorGlobal(parse.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                Growl.ErrorGlobal(ex.Message);
                grd.Visibility = System.Windows.Visibility.Collapsed;
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }

        private void SaveToPC_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (src.Source != null)
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Png file (*.png)|*.png|Jpg file (*.jpg)|*.jpg",
                    FileName = Path.GetFileName(src.Source.ToString())
                };

                if (dialog.ShowDialog() == true)
                {
                    var downloader = new DownloadService();
                    downloader.DownloadFileTaskAsync(src.Source.ToString(), new DirectoryInfo(dialog.FileName));
                }
            }
        }
    }
}

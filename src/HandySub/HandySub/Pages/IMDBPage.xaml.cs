using HandySub.Common;
using HandySub.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using Windows.ApplicationModel.DataTransfer;
using AutoSuggestBox = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
using AutoSuggestBoxQuerySubmittedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs;
using AutoSuggestBoxTextChangedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs;
using Page = Microsoft.UI.Xaml.Controls.Page;

namespace HandySub.Pages
{
    public sealed partial class IMDBPage : Page
    {
        public IMDBPage()
        {
            this.InitializeComponent();
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                Helper.AddToHistory(args.QueryText);
                progress.IsActive = true;
                infoError.IsOpen = false;
                InfoPanel.Visibility = Visibility.Collapsed;
                Cover.Source = null;

                var url = string.Empty;
                url = AutoSuggest.Text.StartsWith("tt")
                    ? string.Format(Consts.IMDBIDAPI, args.QueryText)
                    : string.Format(Consts.IMDBTitleAPI, args.QueryText);
                try
                {
                    using var client = new HttpClient();
                    var responseBody = await client.GetStringAsync(url);
                    var options = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        ReadCommentHandling = JsonCommentHandling.Skip
                    };
                    var parse = JsonSerializer.Deserialize<IMDBModel>(responseBody, options);
                    if (parse.Response.Equals("True"))
                    {
                        txtImdbId.Text = string.Format(Consts.IMDBBaseUrl, parse.imdbID);
                        if (parse.imdbRating.Contains("N/A") || string.IsNullOrEmpty(parse.imdbRating))
                        {
                            rate.Value = 0;
                        }
                        else
                        {
                            rate.Value = Convert.ToDouble(parse.imdbRating, CultureInfo.InvariantCulture);
                        }
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
                        if (!parse.Poster.Contains("N/A"))
                        {
                            Cover.Source = new BitmapImage(new Uri(parse.Poster));
                        }
                        progress.IsActive = false;
                        InfoPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        progress.IsActive = false;
                        InfoPanel.Visibility = Visibility.Collapsed;
                        infoError.Message = parse.Error;
                        infoError.IsOpen = true;
                    }
                }
                catch (HttpRequestException ex)
                {
                    infoError.Message = ex.Message;
                    infoError.IsOpen = true;
                    progress.IsActive = false;
                    InfoPanel.Visibility = Visibility.Collapsed;
                }
                finally
                {
                    progress.IsActive = true;
                    progress.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(AutoSuggest.Text))
                return;
            
            Helper.LoadHistory(sender, args, AutoSuggest);
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

﻿using HandySub.Common;
using HandySub.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SettingsUI.Helpers;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using Windows.ApplicationModel.DataTransfer;

namespace HandySub.Pages
{
    public sealed partial class IMDBPage : Page
    {
        internal static IMDBPage Instance {  get; set; }
        AutoSuggestBoxTextChangedEventArgs autoSuggestBoxTextChangedEventArgs;
        public IMDBPage()
        {
            this.InitializeComponent();
            Instance = this;
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            errorInfo.IsOpen = false;

            if (GeneralHelper.IsNetworkAvailable())
            {
                if (!string.IsNullOrEmpty(args.QueryText))
                {
                    if (Helper.Settings.IsHistoryEnabled)
                    {
                        Helper.AddToHistory(args.QueryText);
                    }
                    progress.IsActive = true;
                    InfoPanel.Visibility = Visibility.Collapsed;
                    Cover.Source = null;

                    var url = string.Empty;
                    url = AutoSuggest.Text.StartsWith("tt")
                        ? string.Format(Constants.IMDBIDAPI, args.QueryText)
                        : string.Format(Constants.IMDBTitleAPI, args.QueryText);
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
                            txtImdbId.Text = string.Format(Constants.IMDBBaseUrl, parse.imdbID);
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
                            ShowError(parse.Error);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        ShowError(ex.Message);
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
            else
            {
                ShowError(Constants.InternetIsNotAvailable, Constants.InternetIsNotAvailableTitle);
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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
            AutoSuggestBox_TextChanged(AutoSuggest, autoSuggestBoxTextChangedEventArgs);
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

        private void ShowError(string message, string title = null)
        {
            errorInfo.Title = title;
            errorInfo.Message = message;
            errorInfo.IsOpen = true;
        }
    }
}

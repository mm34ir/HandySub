﻿using HandySub.Common;
using HandySub.Models;
using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net;
using ABI.Microsoft.UI.Xaml.Controls;
using System.Text.RegularExpressions;
using Page = Microsoft.UI.Xaml.Controls.Page;
using AutoSuggestBox = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
using AutoSuggestBoxQuerySubmittedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs;
using AutoSuggestBoxTextChangedEventArgs = Microsoft.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs;
using SelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Media.Animation;

namespace HandySub.Pages
{
    public sealed partial class ESubtitlePage : Page
    {
        private ObservableCollection<SearchModel> _subtitles = new ObservableCollection<SearchModel>();
        public ObservableCollection<SearchModel> Subtitles
        {
            get { return _subtitles; }
            set { _subtitles = value; }
        }
        private readonly List<string> wordsToRemove = "دانلود زیرنویس فارسی فیلم,دانلود زیرنویس فارسی سریال".Split(',').ToList();

        public ESubtitlePage()
        {
            this.InitializeComponent();

            if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
            {
                AutoSuggest.Text = App.StartUpArguments.Name;
                SearchSubtitle(AutoSuggest.Text);
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
                Frame.Navigate(typeof(ESubtitleDownloadPage), new NavigationParamModel { Key = (int)Server.ESubtitle + item.Name, Title = item.Name, Link = item.Link }, new DrillInNavigationTransitionInfo());
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

                    var url = string.Format(Consts.ESubtitleSearchAPI, queryText);
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(url);

                    var items = doc.DocumentNode.SelectNodes("//div[@class='poster_box']");
                    var itemsName = doc.DocumentNode.SelectNodes("//div[@class='text']");
                    if (items == null)
                    {
                        ShowInfoBar("Subtitle not found or server is unavailable.");
                    }
                    else
                    {
                        foreach (var node in items.GetEnumeratorWithIndex())
                        {
                            var src = node.Value?.SelectSingleNode(".//a//noscript")?.SelectSingleNode("img")?.Attributes["srcset"]?.Value;
                            src = FixImageSrc(src.Substring(src.LastIndexOf(",") + 1));
                            var item = new SearchModel
                            {
                                Poster = src,
                                Name = FixName(itemsName[node.Index].SelectSingleNode(".//a").InnerText.Trim()),
                                Link = node.Value.SelectSingleNode(".//a")?.Attributes["href"]?.Value,
                                Desc = itemsName[node.Index].SelectSingleNode(".//span").InnerText.Trim()
                            };

                            Subtitles.Add(item);
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

        private void ShowInfoBar(string message)
        {
            Helper.ShowErrorInfoBar(InfoError, message);
        }

        // Remove some persian text from movie/series name
        private string FixName(string name)
        {
            return Regex.Replace(name, "\\b" + string.Join("\\b|\\b", wordsToRemove) + "\\b", " ");
        }

        // select image url
        private string FixImageSrc(string src)
        {
            var regex = new Regex(@"(?<Protocol>\w+):\/\/(?<Domain>[\w@][\w.:@]+)\/?[\w\.?=%&=\-@/$,]*");
            var m = regex.Match(src);
            if (m.Success) return m.Value.Trim();

            return null;
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

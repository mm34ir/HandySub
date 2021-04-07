using HandyControl.Controls;
using HandySub.Assets;
using HtmlAgilityPack;
using ModernWpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using System.Windows.Controls;
using HandySub.Models;
using HandyControl.Tools.Extension;
using static HandySub.Assets.Helper;
using HandyControl.Tools;

namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for Subscene.xaml
    /// </summary>
    public partial class Subscene : UserControl
    {
        ObservableCollection<SubsceneSearchModel> DataList = new ObservableCollection<SubsceneSearchModel>();

        public Subscene()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
            {
                autoBox.Text = App.WindowsContextMenuArgument[0];
                OnSearchStarted();
            }
        }

        private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            LoadHistory(sender, args, autoBox);
        }

        private void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                AddHistory(args.QueryText);
                OnSearchStarted();
            }
        }

        public async void OnSearchStarted()
        {
            try
            {
                if (string.IsNullOrEmpty(autoBox.Text))
                    return;

                tgBlock.IsChecked = false;
                DataList?.Clear();
                DataList.ShapeView().ClearAll().Apply();
                //Get Title with imdb
                if (autoBox.Text.StartsWith("tt"))
                    autoBox.Text = await GetImdbIdFromTitle(autoBox.Text);

                var url = string.Format(Consts.SubsceneSearchAPI, Settings.SubsceneServer.Url, autoBox.Text);
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(url);

                var titleCollection = doc.DocumentNode.SelectSingleNode("//div[@class='search-result']");
                if (titleCollection == null || titleCollection.InnerText.Contains("No results found"))
                {
                    Growl.ErrorGlobal(LocalizationManager.LocalizeString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    for (int i = 1; i < 4; i++)
                    {
                        var node = titleCollection.SelectSingleNode($"ul[{i}]");
                        foreach (var item in node.SelectNodes("li"))
                        {
                            var subNode = item?.SelectSingleNode("div//a");
                            var count = item.SelectSingleNode("span");
                            if (count == null)
                            {
                                count = item.SelectSingleNode("div[@class='subtle count']");
                            }

                            var name = subNode?.InnerText.Trim();
                            var subtitle = new SubsceneSearchModel
                            {
                                Name = name,
                                Link = subNode?.Attributes["href"]?.Value.Trim(),
                                Desc = count?.InnerText.Trim(),
                                Key = GetSubtitleKey(i)
                            };
                            DataList.Add(subtitle);
                        }
                    }
                }
                DataList.ShapeView().GroupBy(x => x.Key).Apply();

                listView.ItemsSource = DataList;
                tgBlock.IsChecked = true;
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
                Growl.ErrorGlobal(LocalizationManager.LocalizeString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(LocalizationManager.LocalizeString("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }

        private string GetSubtitleKey(int index)
        {
            switch (index)
            {
                case 1:
                    return "TVSeries";
                case 2:
                    return "Close";
                case 3:
                    return "Popular";
            }
            return null;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = listView.SelectedItem as SubsceneSearchModel;
            if (item != null)
            {
                MainWindow.Instance.NavigateTo(typeof(SubsceneDownload), item.Link);
            }
        }
    }
}

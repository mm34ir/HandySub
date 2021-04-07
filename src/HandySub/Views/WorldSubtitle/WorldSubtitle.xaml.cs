using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Assets;
using HandySub.Models;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Tools.Extension;
using ModernWpf.Controls;
using HandyControl.Tools;

namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for WorldSubtitle.xaml
    /// </summary>
    public partial class WorldSubtitle : UserControl
    {
        ObservableCollection<SearchModel> DataList = new ObservableCollection<SearchModel>();

        private HtmlDocument doc;

        public WorldSubtitle()
        {
            InitializeComponent();
            
            if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
            {
                autoBox.Text = App.StartUpArguments.Name;
                OnSearchStarted();
            }
        }

        private async Task<bool> LoadData(string Url = null)
        {
            try
            {
                tgBlock.IsChecked = false;

                //Get Title with imdb
                if (autoBox.Text.StartsWith("tt"))
                    autoBox.Text = await Helper.GetImdbIdFromTitle(autoBox.Text, errorCallBack);

                if (string.IsNullOrEmpty(Url))
                {
                    Url = Consts.WorldSubtitleSearchAPI;
                }

                var web = new HtmlWeb();
                doc = await web.LoadFromWebAsync(Url + autoBox.Text);

                var items = doc.DocumentNode.SelectNodes("//div[@class='cat-post-tmp']");
                var infoItems = doc.DocumentNode.SelectNodes("//div[@class='cat-post-info']");
                if (items == null)
                {
                    Growl.ErrorGlobal(LocalizationManager.LocalizeString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    foreach (var node in items.GetEnumeratorWithIndex())
                    {
                        // get link
                        var Link = node.Value.SelectSingleNode(".//a").Attributes["href"].Value;

                        //get title
                        var Title = node.Value.SelectSingleNode(".//a").Attributes["title"].Value;
                        var Img = node.Value.SelectSingleNode(".//a/img")?.Attributes["data-src"].Value;
                        var date = infoItems[node.Index].SelectSingleNode("ul//li[1]");
                        var translator = infoItems[node.Index].SelectSingleNode("ul//li[3]");
                        var sync = infoItems[node.Index].SelectSingleNode("ul//li[5]");

                        foreach (var item in date.SelectNodes("b"))
                        {
                            if (item.Name.ToLower() == "b")
                            {
                                date.RemoveChild(item);
                            }
                        }

                        foreach (var item in translator.SelectNodes("b"))
                        {
                            if (item.Name.ToLower() == "b")
                            {
                                translator.RemoveChild(item);
                            }
                        }

                        foreach (var item in sync.SelectNodes("b"))
                        {
                            if (item.Name.ToLower() == "b")
                            {
                                sync.RemoveChild(item);
                            }
                        }

                        var desc = $"تاریخ ارسال: {date.InnerText.Trim()}{Environment.NewLine} مترجمان: {translator.InnerText.Trim()}{Environment.NewLine} هماهنگ با نسخه: {sync.InnerText.Trim()}";

                        DataList.Add(new SearchModel
                        {
                            Name = Title,
                            Poster = Img ?? "https://file.soft98.ir/uploads/mahdi72/2019/12/24_12-error.jpg",
                            Link = Link,
                            Desc = desc
                        });
                    }
                    listView.ItemsSource = DataList;
                    tgBlock.IsChecked = true;
                    return true;
                }
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
                    Growl.ErrorGlobal(LocalizationManager.LocalizeString("ServerNotFound") + "\n" + ex.Message);
                }
            }
            catch (HttpRequestException hx)
            {
                if (!string.IsNullOrEmpty(hx.Message))
                {
                    Growl.ErrorGlobal(LocalizationManager.LocalizeString("ServerNotFound") + "\n" + hx.Message);
                }
            }
            finally
            {
                tgBlock.IsChecked = true;
            }

            return false;
        }

        private void errorCallBack(string e)
        {
            if (!string.IsNullOrEmpty(e))
            {
                Growl.ErrorGlobal(e);
            }
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

        public async void OnSearchStarted()
        {
            if (string.IsNullOrEmpty(autoBox.Text))
                return;

            try
            {
                DataList?.Clear();

                if (await LoadData())
                {
                    var pagenavi = doc.DocumentNode.SelectNodes("//div[@class='wp-pagenavi']");
                    if (pagenavi != null)
                    {
                        var getPageInfo = pagenavi[0].SelectSingleNode(".//span");
                        var getMaxPage =
                            Convert.ToInt32(getPageInfo.InnerText.Substring(10, getPageInfo.InnerText.Length - 10));
                        paginaton.Visibility = Visibility.Visible;
                        paginaton.MaxPageCount = getMaxPage;
                    }
                    else
                    {
                        paginaton.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (NullReferenceException)
            {
            }
            catch (FormatException)
            {
            }
        }

        private async void paginaton_PageUpdated(object sender, FunctionEventArgs<int> e)
        {
            await LoadData(Consts.WorldSubtitlePageSearchAPI.Format(e.Info.ToString()));
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = listView.SelectedItem as SearchModel;
            if (item != null)
            {
                MainWindow.Instance.NavigateTo(typeof(WorldSubtitleDownload), item.Link);
            }
        }
    }
}

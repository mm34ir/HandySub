using HandyControl.Controls;
using HandySub.Assets;
using HandySub.Models;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using System.Windows.Controls;
using ModernWpf.Controls;
using HandyControl.Tools.Extension;
using HandyControl.Tools;

namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for ISubtitle.xaml
    /// </summary>
    public partial class ISubtitle : UserControl
    {
        ObservableCollection<SearchModel> DataList = new ObservableCollection<SearchModel>();

        public ISubtitle()
        {
            InitializeComponent();
            
            if (!string.IsNullOrEmpty(App.StartUpArguments.Name))
            {
                autoBox.Text = App.StartUpArguments.Name;
                OnSearchStarted(autoBox.Text);
            }
        }

        public async void OnSearchStarted(string query)
        {
            if (string.IsNullOrEmpty(query))
                return;

            try
            {
                DataList?.Clear();
                tgBlock.IsChecked = false;

                //Get Title with imdb
                if (query.StartsWith("tt"))
                    query = await Helper.GetImdbIdFromTitle(query, errorCallBack);

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(Consts.ISubtitleSearchAPI.Format(query));

                var items = doc.DocumentNode.SelectNodes("//div[@class='movie-list-info']");
                if (items == null)
                {
                    Growl.ErrorGlobal(LocalizationManager.LocalizeString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
                    foreach (var node in items)
                    {
                        var src = FixImg($"{Consts.ISubtitleBaseUrl}{node?.SelectSingleNode(".//div/div")?.SelectSingleNode("img")?.Attributes["src"]?.Value}");
                        var name = node?.SelectSingleNode(".//div/div[2]/h3/a");
                        var count = node?.SelectSingleNode(".//div/div[2]/div/div[3]/div/p[1]");
                        var date = node?.SelectSingleNode(".//div/div[2]/div/div[3]/div/p[3]");

                        var page = $"{Consts.ISubtitleBaseUrl}{name?.Attributes["href"]?.Value}";

                        var item = new SearchModel
                        {
                            Poster = src,
                            Name = FixName(name?.InnerText),
                            Link = page,
                            Desc = count?.InnerText.Trim() + Environment.NewLine + date?.InnerText.Trim()
                        };

                        if (!string.IsNullOrEmpty(item.Name))
                        {
                            DataList.Add(item);
                        }
                    }
                    listView.ItemsSource = DataList;
                }

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

        private string FixImg(string img)
        {
            if (img.Contains(";"))
            {
                int index = img.IndexOf(";");
                return img.Substring(0, index);
            }
            else
            {
                return img;
            }
        }

        private string FixName(string name)
        {
            string rem = "&#160";
            if (!string.IsNullOrEmpty(name) && name.Contains(rem))
            {
                return name.Replace(rem, "");
            }
            else
            {
                return name;
            }
        }

        private void errorCallBack(string e)
        {
            Growl.ErrorGlobal(e);
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
                OnSearchStarted(autoBox.Text);
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = listView.SelectedItem as SearchModel;
            if (item != null)
            {
                MainWindow.Instance.NavigateTo(typeof(ISubtitleDownload), item.Link);
            }
        }
    }
}

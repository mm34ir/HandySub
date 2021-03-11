using HandyControl.Controls;
using HandySub.Assets;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Windows.Controls;
using HandySub.Models;
using System.Collections.ObjectModel;
using HandySub.Assets.Strings;
using System.Text.RegularExpressions;
using ModernWpf.Controls;
using HandyControl.Tools.Extension;

namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for ESubtitle.xaml
    /// </summary>
    public partial class ESubtitle : UserControl
    {
        ObservableCollection<SearchModel> DataList = new ObservableCollection<SearchModel>();

        private readonly List<string> wordsToRemove = "دانلود زیرنویس فارسی فیلم,دانلود زیرنویس فارسی سریال".Split(',').ToList();
        public ESubtitle()
        {
            InitializeComponent();
            
            if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
            {
                autoBox.Text = App.WindowsContextMenuArgument[0];
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
                var doc = await web.LoadFromWebAsync(Consts.ESubtitleSearchAPI.Format(query));

                var items = doc.DocumentNode.SelectNodes("//div[@class='poster_box']");
                var itemsName = doc.DocumentNode.SelectNodes("//div[@class='text']");
                if (items == null)
                {
                    Growl.ErrorGlobal(Lang.ResourceManager.GetString("SubNotFound"));
                }
                else
                {
                    DataList?.Clear();
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

                        DataList.Add(item);
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
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                tgBlock.IsChecked = true;
            }
        }

        private void errorCallBack(string e)
        {
            Growl.ErrorGlobal(e);
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
                MainWindow.Instance.NavigateTo(typeof(ESubtitleDownload), item.Link);
            }
        }
    }
}

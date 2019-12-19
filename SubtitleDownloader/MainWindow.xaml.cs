using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Properties.Langs;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using MessageBox = HandyControl.Controls.MessageBox;
using TabItem = HandyControl.Controls.TabItem;

namespace SubtitleDownloader
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Property
        const string SearchAPI = "https://subf2m.co/subtitles/searchbytitle?query={0}&l=";
        const string ItemResultAPI = "https://subf2m.co";
        string SubName = string.Empty;
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<SearchModel> _SearchResult;
        public ObservableCollection<SearchModel> SearchResult
        {
            get { return this._SearchResult; }
            set
            {
                if (this._SearchResult != value)
                {
                    this._SearchResult = value;
                    this.NotifyPropertyChanged("SearchResult");
                }
            }
        }

        private ObservableCollection<ItemResultModel> _ItemResult;
        public ObservableCollection<ItemResultModel> ItemResult
        {
            get { return this._ItemResult; }
            set
            {
                if (this._ItemResult != value)
                {
                    this._ItemResult = value;
                    this.NotifyPropertyChanged("ItemResult");
                }
            }
        }

        private FlowDirection _FlowDirection;
        public FlowDirection LayoutFlowDirection
        {
            get { return this._FlowDirection; }
            set
            {
                if (this._FlowDirection != value)
                {
                    this._FlowDirection = value;
                    this.NotifyPropertyChanged("LayoutFlowDirection");
                }
            }
        }
        #endregion
        #region Model
        public class SearchModel
        {
            public string Name { get; set; }
            public string Link { get; set; }
        }

        public class ItemResultModel
        {
            public string Name { get; set; }
            public string Language { get; set; }
            public string Translator { get; set; }
            public string Link { get; set; }
        }
        #endregion

        CollectionView view2;
        CollectionView view;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            setLayoutDirection();
        }
        #region Search in Listbox
        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(txtListBoxSearch.Text))
            {
                return true;
            }
            else
            {
                return ((item as SearchModel).Name.IndexOf(txtListBoxSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }
        private bool UserFilter2(object item)
        {
            if (string.IsNullOrEmpty(txtListBoxResult.Text))
            {
                return true;
            }
            else
            {
                return ((item as ItemResultModel).Name.IndexOf(txtListBoxResult.Text, StringComparison.OrdinalIgnoreCase) >= 0) || ((item as ItemResultModel).Translator.IndexOf(txtListBoxResult.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private void SearchListBox_OnSearchStarted(object sender, FunctionEventArgs<string> e)
        {
            if (view == null)
                return;

            CollectionViewSource.GetDefaultView(SearchResult).Refresh();
        }
        private void ResultListBox_OnSearchStarted(object sender, FunctionEventArgs<string> e)
        {
            if (view2 == null)
                return;

            CollectionViewSource.GetDefaultView(ItemResult).Refresh();
        }
        #endregion
        #region Change Skin and Language
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is SkinType tag)
            {
                PopupConfig.IsOpen = false;
                InIHelper.AddValue(SettingsKey.Skin, tag.ToString());
                ((App)Application.Current).UpdateSkin(tag);
            }
        }

        private void ButtonLangs_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is string tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(InIHelper.ReadValue(SettingsKey.Language))) return;
                Growl.AskGlobal(Properties.Langs.Lang.ChangeLangAsk, b =>
                {
                    if (!b) return true;
                    InIHelper.AddValue(SettingsKey.Language, tag);
                    var processModule = Process.GetCurrentProcess().MainModule;
                    if (processModule != null)
                        Process.Start(processModule.FileName);
                    Application.Current.Shutdown();
                    return true;
                });
            }
        }
        #endregion

        private void setLayoutDirection()
        {
            if (System.IO.File.Exists("config.ini"))
            {
                var lang = InIHelper.ReadValue(SettingsKey.Language);
                if (lang != null && lang.Equals("fa"))
                {
                    
                    LayoutFlowDirection = FlowDirection.RightToLeft;
                }
            }
        }
       
        private void SearchBar_SearchStarted(object sender, FunctionEventArgs<string> e)
        {
            SearchResult = new ObservableCollection<SearchModel>();
            var search= sender as SearchBar;
            if (string.IsNullOrEmpty(search.Text))
                return;
            try
            {
                var url = string.Format(SearchAPI, search.Text);
                var web = new HtmlWeb();
                var doc = web.Load(url);
                var getLanguage = InIHelper.ReadValue(SettingsKey.SubtitleLanguage);
                if (string.IsNullOrEmpty(getLanguage))
                {
                    getLanguage = "farsi_persian";
                }
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class='" + "title" + "']"))
                {
                    var item = new SearchModel { Link = node.SelectSingleNode(".//a").Attributes["href"].Value + $"/{getLanguage}/", Name = node.InnerText };
                    SearchResult.Add(item);
                }
                if (SearchResult != null)
                {
                    view = (CollectionView)CollectionViewSource.GetDefaultView(SearchResult);
                    view.Filter = UserFilter;
                }
            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (ArgumentNullException) { }

        }

        private void SearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ItemResult = new ObservableCollection<ItemResultModel>();
            var list = sender as ListBox;
            try
            {
                dynamic selectedItem = list.SelectedItems[0];
                string link = selectedItem.Link;

                var url = ItemResultAPI + link;
                var web = new HtmlWeb();
                var doc = web.Load(url);
                foreach (var (node, index) in doc.DocumentNode.SelectNodes("//ul[@class='" + "scrolllist" + "']").WithIndex())
                {
                    var translator = doc.DocumentNode.SelectNodes("//div[@class='" + "comment-col" + "']")[index].InnerText;
                    var download_Link = doc.DocumentNode.SelectNodes("//a[@class='" + "download icon-download" + "']")[index].GetAttributeValue("href", "");
                    var singleLineTranslator = Regex.Replace(translator, @"\s+", " ").Replace("&nbsp;", "");
                    if (singleLineTranslator.Contains("&nbsp;"))
                        singleLineTranslator = singleLineTranslator.Replace("&nbsp;", "");

                    var img = doc.DocumentNode.SelectSingleNode("//div[@class='poster']//img");

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    if (img != null)
                    {
                        bitmap.UriSource = new Uri(img.GetAttributeValue("src", "pack://application:,,,/SubtitleDownloader;component/Resources/Img/notfound.png"), UriKind.Absolute);
                    }
                    else
                    {
                        bitmap.UriSource = new Uri("pack://application:,,,/SubtitleDownloader;component/Resources/Img/notfound.png", UriKind.Absolute);
                    }
                    bitmap.EndInit();

                    poster.Source = bitmap;

                    var getLanguage = InIHelper.ReadValue(SettingsKey.SubtitleLanguage);
                    if (string.IsNullOrEmpty(getLanguage))
                    {
                        getLanguage = "farsi_persian";
                    }

                    var input = download_Link;

                    var output = Regex.Replace(input, $@"/subtitlesw*", string.Empty);
                    var output2 = Regex.Match(output, @"/\w*/");
                    //string key = output2.Value.Replace("/", "");

                    //var output3 = Regex.Replace(output, $@"/{key}w*", string.Empty);

                    //var output4 = Regex.Replace(output3, @"/\w*/", string.Empty);

                    //var output5 = @"/subtitles/" + key + output4;

                    var item = new ItemResultModel { Name = node.InnerText, Translator = singleLineTranslator, Link = input, Language = getLanguage };
                    ItemResult.Add(item);
                }
                if (ItemResult != null)
                {
                    view2 = (CollectionView)CollectionViewSource.GetDefaultView(ItemResult);
                    view2.Filter = UserFilter2;
                }
            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (ArgumentNullException) { }
        }

        private void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListBox;
            try
            {
                dynamic selectedItem = list.SelectedItems[0];
                string link = selectedItem.Link;

                Download.Link = ItemResultAPI + link;
                Download.Info = selectedItem.Name;
                Download.Translator = selectedItem.Translator;
                Download.LanguageTag = selectedItem.Language;
                string input = selectedItem.Name;
                if (input.Contains("By"))
                {
                    int index = input.IndexOf("By");
                    if (index > 0)
                        input = input.Substring(0, index);
                    input = Regex.Replace(input, @"\s+", " ");
                }

                CreateTabItem(new Download(), input);
            }
            catch (NullReferenceException) { }
            catch (ArgumentOutOfRangeException) { }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuItem;
            switch (menu.Tag)
            {
                case "setting":
                    CreateTabItem(new Settings(), Properties.Langs.Lang.Settings);
                    break;
                case "about":
                    CreateTabItem(new About(), Properties.Langs.Lang.About);
                    break;
                case "update":
                    CreateTabItem(new Update(), Properties.Langs.Lang.Update);
                    break;
            }
        }

        private void CreateTabItem(UserControl control, string Header)
        {
            TabItem tabItem = new HandyControl.Controls.TabItem();
            tabItem.Content = control;
            tabItem.ShowCloseButton = true;
            tab.Items.Insert(tab.Items.Count, tabItem);
            tabItem.Header = Header;

            var getSelectTab = InIHelper.ReadValue(SettingsKey.SelectTab);
            if (string.IsNullOrEmpty(getSelectTab))
            {
                getSelectTab = "true";
            }
            if(Convert.ToBoolean(getSelectTab))
                tab.SelectedIndex = tab.Items.Count - 1;
        }

        
    }
}

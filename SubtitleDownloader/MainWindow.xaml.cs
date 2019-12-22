﻿using HandyControl.Controls;
using HandyControl.Data;
using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TabItem = HandyControl.Controls.TabItem;
namespace SubtitleDownloader
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Property
        //instance for accessing MainWindow from everywhere
        internal static MainWindow mainWindow;

        private const string SearchAPI = "{0}/subtitles/searchbytitle?query={1}&l=";
        private readonly string SubName = string.Empty;
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private ObservableCollection<SearchModel> _SearchResult;
        public ObservableCollection<SearchModel> SearchResult
        {
            get => _SearchResult;
            set
            {
                if (_SearchResult != value)
                {
                    _SearchResult = value;
                    NotifyPropertyChanged("SearchResult");
                }
            }
        }

        private ObservableCollection<ItemResultModel> _ItemResult;
        public ObservableCollection<ItemResultModel> ItemResult
        {
            get => _ItemResult;
            set
            {
                if (_ItemResult != value)
                {
                    _ItemResult = value;
                    NotifyPropertyChanged("ItemResult");
                }
            }
        }

        private FlowDirection _FlowDirection;
        public FlowDirection LayoutFlowDirection
        {
            get => _FlowDirection;
            set
            {
                if (_FlowDirection != value)
                {
                    _FlowDirection = value;
                    NotifyPropertyChanged("LayoutFlowDirection");
                }
            }
        }

        private bool _IsDraggableTab;
        public bool IsDraggableTab
        {
            get => _IsDraggableTab;
            set
            {
                if (_IsDraggableTab != value)
                {
                    _IsDraggableTab = value;
                    NotifyPropertyChanged("IsDraggableTab");
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

        private CollectionView ViewResult;
        private CollectionView ViewSearch;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mainWindow = this;
            setFlowDirectionAndConfig();

            if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument))
            {
                txtSearch.Text = App.WindowsContextMenuArgument;
                SearchBar_SearchStarted(null, null);
            }
        }
        #region Search in Listbox
        private bool UserFilterSearch(object item)
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
        private bool UserFilterResult(object item)
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
            if (ViewSearch == null)
            {
                return;
            }

            CollectionViewSource.GetDefaultView(SearchResult).Refresh();
        }
        private void ResultListBox_OnSearchStarted(object sender, FunctionEventArgs<string> e)
        {
            if (ViewResult == null)
            {
                return;
            }

            CollectionViewSource.GetDefaultView(ItemResult).Refresh();
        }
        #endregion
        #region Change Skin and Language
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e)
        {
            PopupConfig.IsOpen = true;
        }

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is SkinType tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(GlobalData.Config.Skin)) return;
                GlobalData.Config.Skin = tag;
                GlobalData.Save();
                ((App)Application.Current).UpdateSkin(tag);
            }
        }

        private void ButtonLangs_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is string tag)
            {
                PopupConfig.IsOpen = false;
                if (tag.Equals(GlobalData.Config.UILang)) return;

                Growl.AskGlobal(new GrowlInfo
                {
                    Message = Properties.Langs.Lang.ChangeLangAsk,
                    CancelStr = Properties.Langs.Lang.Cancel,
                    ConfirmStr = Properties.Langs.Lang.Confirm,
                    ActionBeforeClose = isConfirmed =>
                    {

                        if (!isConfirmed)
                            return true;
                        GlobalData.Config.UILang = tag;
                        GlobalData.Save();
                        ProcessModule processModule = Process.GetCurrentProcess().MainModule;
                        if (processModule != null)
                        {
                            Process.Start(processModule.FileName);
                        }
                        Application.Current.Shutdown();
                        return true;

                    }
                });
            }
        }
        #endregion

        private void setFlowDirectionAndConfig()
        {
            //Set FlowDirection
            if (GlobalData.Config.UILang.Equals("fa"))
            {
                LayoutFlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                LayoutFlowDirection = FlowDirection.LeftToRight;
            }

            //Set Tab IsDraggable
            IsDraggableTab = GlobalData.Config.IsDraggableTab;
        }

        private async void SearchBar_SearchStarted(object sender, FunctionEventArgs<string> e)
        {
            SearchResult = new ObservableCollection<SearchModel>();
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                return;
            }
            try
            {
                string url = string.Format(SearchAPI, GlobalData.Config.ServerUrl, txtSearch.Text);
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(url);

                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class='title']"))
                {
                    SearchModel item = new SearchModel { Link = node.SelectSingleNode(".//a").Attributes["href"].Value + $"/{GlobalData.Config.SubtitleLang}/", Name = node.InnerText };
                    SearchResult.Add(item);
                }
                if (SearchResult != null)
                {
                    ViewSearch = (CollectionView)CollectionViewSource.GetDefaultView(SearchResult);
                    ViewSearch.Filter = UserFilterSearch;
                }
            }
            catch (ArgumentOutOfRangeException) { }
            catch (ArgumentNullException) { }
            catch (System.NullReferenceException) { }
            catch (System.Net.WebException ex)
            {
                Growl.ErrorGlobal(Properties.Langs.Lang.ServerOut + "\n" + ex.Message);
            }
        }

        private async void Subf2mCore()
        {
            ItemResult = new ObservableCollection<ItemResultModel>();
            try
            {
                dynamic selectedItem = lstSearch.SelectedItems[0];
                string url = GlobalData.Config.ServerUrl + selectedItem.Link;

                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(url);

                //get poster img
                HtmlNode img = doc.DocumentNode.SelectSingleNode("//div[@class='poster']//img");

                if (img != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(img.GetAttributeValue("src", ""), UriKind.Absolute);
                    bitmap.EndInit();
                    poster.Source = bitmap;
                }

                foreach ((HtmlNode node, int index) in doc.DocumentNode.SelectNodes("//ul[@class='scrolllist']").WithIndex())
                {

                    string translator = node.SelectNodes("//div[@class='comment-col']")[index].InnerText;
                    string download_Link = node.SelectNodes("//a[@class='download icon-download']")[index].GetAttributeValue("href", "");

                    //remove empty lines
                    string singleLineTranslator = Regex.Replace(translator, @"\s+", " ").Replace("&nbsp;", "");
                    if (singleLineTranslator.Contains("&nbsp;"))
                    {
                        singleLineTranslator = singleLineTranslator.Replace("&nbsp;", "");
                    }

                    ItemResultModel item = new ItemResultModel { Name = node.InnerText, Translator = singleLineTranslator, Link = download_Link, Language = GlobalData.Config.SubtitleLang };
                    ItemResult.Add(item);
                }

                //enable search
                if (ItemResult != null)
                {
                    ViewResult = (CollectionView)CollectionViewSource.GetDefaultView(ItemResult);
                    ViewResult.Filter = UserFilterResult;
                }
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
        }

        private async void SubsceneCore()
        {
            try
            {
                ItemResult = new ObservableCollection<ItemResultModel>();
                dynamic selectedItem = lstSearch.SelectedItems[0];
                string url = GlobalData.Config.ServerUrl + selectedItem.Link;

                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(url);

                //get poster img
                HtmlNode img = doc.DocumentNode.SelectSingleNode("//div[@class='poster']//img");

                if (img != null)
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(img.GetAttributeValue("src", ""), UriKind.Absolute);
                    bitmap.EndInit();
                    poster.Source = bitmap;
                }

                HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[1]//tbody");
                if (table != null)
                {
                    foreach ((var cell, int index) in table.SelectNodes(".//tr/td").WithIndex())
                    {
                        if (cell.InnerText.Contains("There are no subtitles"))
                            break;

                        var Name = cell.SelectNodes("//span[2]")[index].InnerText;
                        var Comment = doc.DocumentNode.SelectNodes(".//tr/td//div")[index].InnerText;

                        //remove empty line
                        if (Comment.Contains("&nbsp;"))
                        {
                            Comment = Comment.Replace("&nbsp;", "");
                        }

                        var Link = doc.DocumentNode.SelectNodes(".//tr/td//a")[index].Attributes["href"].Value;

                        //escape Unnecessary line
                        if (Link.Contains("/u/"))
                            continue;

                        ItemResultModel item = new ItemResultModel { Name = Name, Translator = Comment, Link = Link, Language = GlobalData.Config.SubtitleLang };
                        ItemResult.Add(item);
                    }
                }

                //enable search
                if (ItemResult != null)
                {
                    ViewResult = (CollectionView)CollectionViewSource.GetDefaultView(ItemResult);
                    ViewResult.Filter = UserFilterResult;
                }
            }
            catch (ArgumentNullException)
            {
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
        }

        private void SearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalData.Config.ServerUrl.Contains("subf2m.co"))
            {
                Subf2mCore();
            }
            else
            {
                SubsceneCore();
            }
        }

        private void ResultList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dynamic selectedItem = lst.SelectedItems[0];

                Download.Link = GlobalData.Config.ServerUrl + selectedItem.Link;
                Download.Info = selectedItem.Name;
                Download.Translator = selectedItem.Translator;
                Download.LanguageTag = selectedItem.Language;

                string input = selectedItem.Name;

                //remove Unnecessary words
                if (input.Contains("By"))
                {
                    int index = input.IndexOf("By");
                    if (index > 0)
                    {
                        input = input.Substring(0, index);
                    }

                    input = Regex.Replace(input, @"\s+", " ");
                }

                CreateTabItem(new Download(), input);
            }
            catch (NullReferenceException) { }
            catch (ArgumentOutOfRangeException) { }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menu = sender as MenuItem;
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
                case "popular":
                    CreateTabItem(new PopularSeries(), Properties.Langs.Lang.PopularSeries);
                    break;
            }
        }

        /// <summary>
        /// Create a New Tab
        /// </summary>
        /// <param name="control">UserControl, that used as Content</param>
        /// <param name="Header">Header of TabItem</param>
        private void CreateTabItem(UserControl control, string Header)
        {
            TabItem tabItem = new HandyControl.Controls.TabItem
            {
                Content = control,
                ShowCloseButton = true
            };
            tab.Items.Insert(tab.Items.Count, tabItem);

            //remove empty lines
            string input = Regex.Replace(Header, @"\s+", " ");
            tabItem.Header = input;

            if (GlobalData.Config.IsAutoSelectOpenedTab)
            {
                tab.SelectedIndex = tab.Items.Count - 1;
            }
        }

        /// <summary>
        /// Automatic Search, used when User Luanch App from ContextMenu
        /// </summary>
        /// <param name="txtDisplay"></param>
        public void getTabHome(string txtDisplay)
        {
            tab.SelectedIndex = 0;
            txtSearch.Text = txtDisplay;
            SearchBar_SearchStarted(null, null);
        }
    }
}

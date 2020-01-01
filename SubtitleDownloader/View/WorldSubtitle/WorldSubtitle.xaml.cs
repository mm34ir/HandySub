using HtmlAgilityPack;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for WorldSubtitle.xaml
    /// </summary>
    public partial class WorldSubtitle : INotifyPropertyChanged
    {
        private string BasePageUrl = "http://worldsubtitle.info/page/{0}?s=";
        HtmlDocument doc;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        private ObservableCollection<AvatarWorldModel> _DataList;
        public ObservableCollection<AvatarWorldModel> DataList
        {
            get => _DataList;
            set
            {
                if (_DataList != value)
                {
                    _DataList = value;
                    NotifyPropertyChanged("DataList");
                }
            }
        }
        public WorldSubtitle()
        {
            InitializeComponent();
            DataContext = this;

            //get subtitle if luanch is from ContextMenu
            if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[1]))
            {
                txtSearch.Text = App.WindowsContextMenuArgument[1];
                txtSearch_SearchStarted(null, null);
            }
        }

        private async Task<bool> LoadData(string Url = "http://worldsubtitle.info/?s=")
        {
            DataList = new ObservableCollection<AvatarWorldModel>();
            busyIndicator.IsBusy = true;
            try
            {
                HtmlWeb web = new HtmlWeb();
                doc = await web.LoadFromWebAsync(Url + txtSearch.Text);
                var repeaters = doc.DocumentNode.SelectNodes("//div[@class='cat-post-tmp']");
                if (repeaters != null)
                {
                    foreach (HtmlNode node in repeaters)
                    {
                        // get link
                        var Link = node.SelectSingleNode(".//a").Attributes["href"].Value;

                        //get title
                        var Title = node.SelectSingleNode(".//a").Attributes["title"].Value;
                        var Img = node.SelectSingleNode(".//a/img")?.Attributes["src"].Value;

                        DataList.Add(new AvatarWorldModel
                        {
                            DisplayName = Title,
                            AvatarUri = Img ?? "https://file.soft98.ir/uploads/mahdi72/2019/12/24_12-error.jpg",
                            Link = Link,
                        });

                        if (busyIndicator.IsBusy)
                            busyIndicator.IsBusy = false;
                    }
                    return true;
                }
                else
                {
                    DataList.Add(new AvatarWorldModel
                    {
                        DisplayName = Properties.Langs.Lang.NotFound,
                        AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/24_12-error.jpg",
                    });
                }
            }
            catch (NullReferenceException)
            {
            }

            busyIndicator.IsBusy = false;
            return false;
        }

        private async void txtSearch_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                return;
            }

            if (await LoadData())
            {
                var pagenavi = doc.DocumentNode.SelectNodes("//div[@class='wp-pagenavi']");
                if (pagenavi != null)
                {
                    var getPageInfo = pagenavi[0].SelectSingleNode(".//span");
                    int getMaxPage = Convert.ToInt32(getPageInfo.InnerText.Substring(10, getPageInfo.InnerText.Length - 10));
                    page.Visibility = System.Windows.Visibility.Visible;
                    page.MaxPageCount = getMaxPage;
                }
                else
                {
                    page.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private async void page_PageUpdated(object sender, HandyControl.Data.FunctionEventArgs<int> e)
        {
            await LoadData(string.Format(BasePageUrl, e.Info));
        }
    }
}

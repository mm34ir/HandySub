using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for WorldSubtitle.xaml
    /// </summary>
    public partial class WorldSubtitle : INotifyPropertyChanged
    {
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
        }

        private async void txtSearch_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            DataList = new ObservableCollection<AvatarWorldModel>();
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                return;
            }

            string url = "http://worldsubtitle.info/?s=" + txtSearch.Text;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = await web.LoadFromWebAsync(url);

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class='cat-post-tmp']"))
            {
                // get link
                var Link = node.SelectSingleNode(".//a").Attributes["href"].Value;

                //get title
                var Title = node.SelectSingleNode(".//a").Attributes["title"].Value;
                var Img = node.SelectSingleNode(".//a/img").Attributes["src"].Value;

                DataList.Add(new AvatarWorldModel
                {
                    DisplayName = Title,
                    AvatarUri = Img,
                    Link = Link,
                });
            }
        }
    }
}

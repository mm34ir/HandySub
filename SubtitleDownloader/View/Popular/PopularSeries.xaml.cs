using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Data;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for PopularSeries.xaml
    /// </summary>
    public partial class PopularSeries : UserControl
    {
        public List<AvatarModel> DataList { get; set; }
        public PopularSeries()
        {
            InitializeComponent();
            DataContext = this;
            DataList = GetContributorDataList();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(DataList);
            view.Filter = UserFilter;
        }

        internal List<AvatarModel> GetContributorDataList()
        {
            var client = new WebClient();
            var list = new List<AvatarModel>();
            try
            {
                var json = client.DownloadString(new Uri("https://raw.githubusercontent.com/ghost1372/SubtitlePopular/master/Popular.json"));
                var objList = JsonConvert.DeserializeObject<List<dynamic>>(json);

                list.AddRange(objList.Select(item => new AvatarModel
                {
                    DisplayName = item.name,
                    AvatarUri = item.poster_url,
                }));
            }
            catch
            {
                // ignored
            }
            return list;
        }

        private void SearchBar_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                return;
            }

            CollectionViewSource.GetDefaultView(DataList).Refresh();
        }
        private bool UserFilter(object item)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
            {
                return true;
            }
            else
            {
                return ((item as AvatarModel).DisplayName.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }
    }
}

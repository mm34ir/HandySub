using System;
using System.Collections.Generic;
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
            List<AvatarModel> list = new List<AvatarModel>();
            try
            {
                list.Add(new AvatarModel
                {
                    DisplayName = "Arrow",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-Arrow.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Better Call Saul",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-Better-Call-Saul.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Flash",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-Flash.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "His Dark Materials",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-His-Dark-Materials.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Power",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-Power.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Rick and Morty",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-Rick-and-Morty.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "See",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-See.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Sex Education",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-Sex-Education.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "The Blacklist",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-The-Blacklist.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "The Walking Dead",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-The-Walking-Dead.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "The Witcher",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-The-Witcher.jpg"
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Vikings",
                    AvatarUri = "https://file.soft98.ir/uploads/mahdi72/2019/12/20_12-Vikings.jpg"
                });
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

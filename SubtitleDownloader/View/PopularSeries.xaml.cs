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
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/Arrow.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=arrow&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Better Call Saul",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/Better Call Saul.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=Better+Call+Saul&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Flash",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/Flash.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=Flash&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "His Dark Materials",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/His Dark Materials.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=His+Dark+Materials&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Power",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/Power.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=Power&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Rick and Morty",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/Rick and Morty.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=Rick+and+Morty&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "See",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/See.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=See&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Sex Education",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/Sex Education.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=Education&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "The Blacklist",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/The Blacklist.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=The+Blacklist&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "The Walking Dead",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/The Walking Dead.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=The+Walking+Dead&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "The Witcher",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/The Witcher.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=The+Witcher&l="
                });
                list.Add(new AvatarModel
                {
                    DisplayName = "Vikings",
                    AvatarUri = "pack://application:,,,/SubtitleDownloader;component/Resources/Img/Cover/Vikings.jpg",
                    Link = "https://subf2m.co/subtitles/searchbytitle?query=Vikings&l="
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

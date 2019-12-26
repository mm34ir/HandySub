using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows.Controls;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for PopularSeries.xaml
    /// </summary>
    public partial class PopularSeries : INotifyPropertyChanged
    {
        internal static PopularSeries Popular;
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        private ObservableCollection<AvatarModel> _DataList;
        public ObservableCollection<AvatarModel> DataList
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
        public PopularSeries()
        {
            InitializeComponent();
            DataContext = Popular = this;

        }
        public async void Load()
        {
            DataList = new ObservableCollection<AvatarModel>();
            busyIndicator.IsBusy = true;
            var client = new WebClient();
            try
            {
                var json = await client.DownloadStringTaskAsync(new Uri("https://raw.githubusercontent.com/ghost1372/SubtitlePopular/master/Popular.json"));
                var objList = JsonConvert.DeserializeObject<ObservableCollection<dynamic>>(json);
                foreach (var item in objList)
                {
                    DataList.Add(new AvatarModel { DisplayName = item.name, AvatarUri = item.poster_url });

                    if (busyIndicator.IsBusy)
                        busyIndicator.IsBusy = false;
                }

            }
            catch
            {
                // ignored
            }
        }

        private void SearchBar_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (e.Info == null) return;
            foreach (AvatarModel item in lst.Items)
            {
                var listBoxItem = lst.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                listBoxItem?.Show(item.DisplayName.ToLower().Contains(e.Info.ToLower()));
            }
        }
    }
}

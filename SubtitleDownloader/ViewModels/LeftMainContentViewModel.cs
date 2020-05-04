using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SubtitleDownloader.ViewModels
{
    public class LeftMainContentViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        #region Command
        public DelegateCommand<SelectionChangedEventArgs> SwitchItemCmd { get; private set; }
        public DelegateCommand EmptyContentCommand { get; private set; }
        #endregion

        public LeftMainContentViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            SwitchItemCmd = new DelegateCommand<SelectionChangedEventArgs>(Switch);
            EmptyContentCommand = new DelegateCommand(EmptyContent);

            initPlugins();
        }

        private void EmptyContent()
        {
            _regionManager.Regions["ContentRegion"].RemoveAll();
        }

        private void Switch(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            if (e.AddedItems[0] is ListBoxItem item)
            {
                if (item.Tag != null)
                {
                    _regionManager.RequestNavigate("ContentRegion", item.Tag.ToString());
                }
            }
        }

        public class model
        {
            public string DisplayName { get; set; }
            public string Tag { get; set; }
            public bool IsNew { get; set; }
        }

        private ObservableCollection<model> _DataService = new ObservableCollection<model>();
        public ObservableCollection<model> DataService
        {
            get { return _DataService; }
            set { SetProperty(ref _DataService, value); }
        }

        public void initPlugins()
        {
            DataService.Add(new model { DisplayName = "Plugin", Tag = "Plugin", IsNew = false });
            DataService.Add(new model { DisplayName = "Plugin", Tag = "Plugin", IsNew = false });
            DataService.Add(new model { DisplayName = "Plugin", Tag = "Plugin", IsNew = true });
        }
    }
}

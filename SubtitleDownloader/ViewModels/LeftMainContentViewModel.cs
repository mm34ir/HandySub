using Module.Core;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Windows.Controls;
namespace SubtitleDownloader.ViewModels
{
    public class LeftMainContentViewModel : BindableBase
    {
        internal static LeftMainContentViewModel Instance;
        private readonly IRegionManager _regionManager;

        private ObservableCollection<ModuleModel> _dataService = new ObservableCollection<ModuleModel>();
        public ObservableCollection<ModuleModel> DataService
        {
            get => _dataService;
            set => SetProperty(ref _dataService, value);
        }
        #region Command
        public DelegateCommand<SelectionChangedEventArgs> SwitchItemCmd { get; private set; }
        public DelegateCommand<SelectionChangedEventArgs> SwitchModuleItemCmd { get; private set; }
        public DelegateCommand EmptyContentCommand { get; private set; }
        #endregion

        public LeftMainContentViewModel(IRegionManager regionManager)
        {
            Instance = this;
            _regionManager = regionManager;
            SwitchItemCmd = new DelegateCommand<SelectionChangedEventArgs>(Switch);
            SwitchModuleItemCmd = new DelegateCommand<SelectionChangedEventArgs>(SwitchModule);
            EmptyContentCommand = new DelegateCommand(EmptyContent);
        }

        private void SwitchModule(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            if (e.AddedItems[0] is ModuleModel item)
            {
                //Clear last module
                EmptyContent();

                // register module
                _regionManager.RegisterViewWithRegion("ContentRegion", item.DefaultView);
            }
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
    }
}

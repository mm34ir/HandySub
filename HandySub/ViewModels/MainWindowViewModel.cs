using HandyControl.Controls;
using HandySub.Views;
using ModernWpf.Controls;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Linq;

namespace HandySub.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        IRegionManager region;
        internal static MainWindowViewModel Instance;

        public DelegateCommand<NavigationViewSelectionChangedEventArgs> SwitchCommand { get; set; }
        public DelegateCommand<NavigationViewBackRequestedEventArgs> BackCommand { get; set; }

        private NavigationViewPaneDisplayMode _paneDisplayMode;
        public NavigationViewPaneDisplayMode PaneDisplayMode
        {
            get { return GlobalDataHelper<AppConfig>.Config.PaneDisplayMode; }
            set { SetProperty(ref _paneDisplayMode, value); }
        }

        private bool _IsFirstRun;
        public bool IsFirstRun
        {
            get { return GlobalDataHelper<AppConfig>.Config.IsFirstRun; }
            set { SetProperty(ref _IsFirstRun, value); }
        }

        private bool _isBackVisible;
        public bool IsBackVisible
        {
            get { return GlobalDataHelper<AppConfig>.Config.IsBackVisible; }
            set { SetProperty(ref _isBackVisible, value); }
        }

        private bool _IsBackEnabled;
        public bool IsBackEnabled
        {
            get { return _IsBackEnabled; }
            set { SetProperty(ref _IsBackEnabled, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            Instance = this;

            region = regionManager;
            SwitchCommand = new DelegateCommand<NavigationViewSelectionChangedEventArgs>(Switch);
            BackCommand = new DelegateCommand<NavigationViewBackRequestedEventArgs>(Back);
        }

        private void Back(NavigationViewBackRequestedEventArgs e)
        {
            var currentView = region.Regions["ContentRegion"].ActiveViews.FirstOrDefault().GetType().Name;

            if (currentView.Equals(typeof(SubsceneDownload).Name))
            {
                region.RequestNavigate("ContentRegion", "Subscene");
            }
            else if (currentView.Equals(typeof(ESubtitleDownload).Name))
            {
                region.RequestNavigate("ContentRegion", "ESubtitle");
            }
            else if (currentView.Equals(typeof(WorldSubtitleDownload).Name))
            {
                region.RequestNavigate("ContentRegion", "WorldSubtitle");
            }
            IsBackEnabled = false;

        }

        private void Switch(NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is NavigationViewItem item)
            {
                if (item.Tag != null)
                {
                    region.RequestNavigate("ContentRegion", item.Tag.ToString());
                }
            }
        }
    }
}

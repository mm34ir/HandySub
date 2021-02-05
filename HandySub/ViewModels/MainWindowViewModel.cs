using System.Linq;
using HandyControl.Controls;
using HandySub.Views;
using ModernWpf.Controls;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace HandySub.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        internal static MainWindowViewModel Instance;
        private readonly IRegionManager region;

        private bool _IsBackEnabled;

        private bool _isBackVisible;

        private bool _IsFirstRun;

        private NavigationViewPaneDisplayMode _paneDisplayMode;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            Instance = this;

            region = regionManager;
            SwitchCommand = new DelegateCommand<NavigationViewSelectionChangedEventArgs>(Switch);
            BackCommand = new DelegateCommand<NavigationViewBackRequestedEventArgs>(Back);
        }

        public DelegateCommand<NavigationViewSelectionChangedEventArgs> SwitchCommand { get; set; }
        public DelegateCommand<NavigationViewBackRequestedEventArgs> BackCommand { get; set; }

        public NavigationViewPaneDisplayMode PaneDisplayMode
        {
            get => GlobalData.Config.PaneDisplayMode;
            set => SetProperty(ref _paneDisplayMode, value);
        }

        public bool IsFirstRun
        {
            get => GlobalData.Config.IsFirstRun;
            set => SetProperty(ref _IsFirstRun, value);
        }

        public bool IsBackVisible
        {
            get => GlobalData.Config.IsBackVisible;
            set => SetProperty(ref _isBackVisible, value);
        }

        public bool IsBackEnabled
        {
            get => _IsBackEnabled;
            set => SetProperty(ref _IsBackEnabled, value);
        }

        private void Back(NavigationViewBackRequestedEventArgs e)
        {
            var currentView = region.Regions["ContentRegion"].ActiveViews.FirstOrDefault().GetType().Name;

            if (currentView.Equals(typeof(SubsceneDownload).Name))
                region.RequestNavigate("ContentRegion", "Subscene");
            else if (currentView.Equals(typeof(ESubtitleDownload).Name))
                region.RequestNavigate("ContentRegion", "ESubtitle");
            else if (currentView.Equals(typeof(WorldSubtitleDownload).Name))
                region.RequestNavigate("ContentRegion", "WorldSubtitle");

            IsBackEnabled = false;
        }

        private void Switch(NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem is NavigationViewItem item)
                if (item.Tag != null)
                    region.RequestNavigate("ContentRegion", item.Tag.ToString());
        }
    }
}
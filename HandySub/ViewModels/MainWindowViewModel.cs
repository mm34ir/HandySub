using ModernWpf.Controls;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace HandySub.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        IRegionManager region;
        public DelegateCommand<NavigationViewSelectionChangedEventArgs> SwitchCommand { get; set; }
        public MainWindowViewModel(IRegionManager regionManager)
        {
            region = regionManager;
            SwitchCommand = new DelegateCommand<NavigationViewSelectionChangedEventArgs>(Switch);
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

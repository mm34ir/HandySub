using Prism.Mvvm;
using System.Windows;

namespace SubtitleDownloader.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private FlowDirection _MainFlowDirection;
        public FlowDirection MainFlowDirection
        {
            get => _MainFlowDirection;
            set => SetProperty(ref _MainFlowDirection, value);
        }

        public MainWindowViewModel()
        {
            SetFlowDirection();
        }

        public FlowDirection SetFlowDirection()
        {
           return MainFlowDirection = GlobalData.Config.UILang.Equals("fa-IR") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
    }
}

using System.Windows;
using HandyControl.Controls;
using HandySub.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;

namespace HandySub
{
    public class Bootstrapper : PrismBootstrapper
    {
        protected override void InitializeShell(DependencyObject shell)
        {
            base.InitializeShell(shell);
            Container.Resolve<IRegionManager>().RequestNavigate("ContentRegion",
                GlobalData.Config.IsFirstRun ? "Settings" : "Subscene");
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ISubtitles>();
            containerRegistry.RegisterForNavigation<ISubtitlesDownload>();
            containerRegistry.RegisterForNavigation<WorldSubtitle>();
            containerRegistry.RegisterForNavigation<WorldSubtitleDownload>();
            containerRegistry.RegisterForNavigation<ESubtitle>();
            containerRegistry.RegisterForNavigation<ESubtitleDownload>();
            containerRegistry.RegisterForNavigation<Settings>();
            containerRegistry.RegisterForNavigation<Subscene>();
            containerRegistry.RegisterForNavigation<SubsceneDownload>();
            containerRegistry.RegisterForNavigation<GetMovieInfoIMDB>();
        }
    }
}
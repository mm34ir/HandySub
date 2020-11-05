using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using System.Windows;

namespace HandySub
{
    public class Bootstrapper : PrismBootstrapper
    {
        protected override void InitializeShell(DependencyObject shell)
        {
            base.InitializeShell(shell);
            Container.Resolve<IRegionManager>().RequestNavigate("ContentRegion", "Subscene");
        }
        protected override DependencyObject CreateShell()
        {
            MainWindow shell = Container.Resolve<MainWindow>();

            if (GlobalDataHelper<AppConfig>.Config.Skin != SkinType.Default)
            {
                ((App)Application.Current).UpdateSkin(GlobalDataHelper<AppConfig>.Config.Skin);
            }
            return shell;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<WorldSubtitle>();
            containerRegistry.RegisterForNavigation<WorldSubtitleDownload>();
            containerRegistry.RegisterForNavigation<ESubtitle>();
            containerRegistry.RegisterForNavigation<ESubtitleDownload>();
            containerRegistry.RegisterForNavigation<Settings>();
            containerRegistry.RegisterForNavigation<PopularSeries>();
            containerRegistry.RegisterForNavigation<Subscene>();
            containerRegistry.RegisterForNavigation<SubsceneDownload>();
            containerRegistry.RegisterForNavigation<GetMovieInfoIMDB>();
        }
    }
}

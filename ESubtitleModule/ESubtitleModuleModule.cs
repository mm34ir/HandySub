using ESubtitleModule.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace ESubtitleModule
{
    public class ESubtitleModuleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ESubtitleDownload>();
        }
    }
}
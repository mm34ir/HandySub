using Prism.Ioc;
using Prism.Modularity;
using WorldSubtitleModule.Views;

namespace WorldSubtitleModule
{
    public class WorldSubtitleModuleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<WorldSubtitleDownload>();
        }
    }
}
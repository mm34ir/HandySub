using Module.Core;
using System;

namespace WorldSubtitleModule
{
    public class ModuleService : IModuleService
    {
        public ModuleModel GetModule()
        {
            Type moduleAType = typeof(WorldSubtitleModuleModule);
            ModuleModel module = new ModuleModel
            {
                DisplayName = "WorldSub",
                ModuleName = moduleAType.Name,
                IsNew = false,
                DefaultView = typeof(Views.WorldSubtitle)
            };
            return module;
        }
    }
}

using Module.Core;
using System;

namespace ESubtitleModule
{
    public class ModuleService : IModuleService
    {
        public ModuleModel GetModule()
        {
            Type moduleAType = typeof(ESubtitleModuleModule);
            ModuleModel module = new ModuleModel
            {
                DisplayName = "ESubtitle",
                ModuleName = moduleAType.Name,
                IsNew = true,
                DefaultView = typeof(Views.ViewA)
            };
            return module;
        }
    }
}

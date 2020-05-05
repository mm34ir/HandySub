using Module.Core;
using System;

namespace SubtitleStarModule
{
    public class ModuleService : IModuleService
    {
        public ModuleModel GetModule()
        {
            Type moduleAType = typeof(SubtitleStarModuleModule);
            ModuleModel module = new ModuleModel
            {
                DisplayName = "SubStar",
                ModuleName = moduleAType.Name,
                IsNew = true,
                DefaultView = typeof(Views.ViewA)
            };
            return module;
        }
    }
}

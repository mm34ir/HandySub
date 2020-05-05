using System;
namespace Module.Core
{
    public class ModuleModel
    {
        public string DisplayName { get; set; }
        public string ModuleName { get; set; }
        public bool IsNew { get; set; }
        public Type DefaultView { get; set; }
    }
}

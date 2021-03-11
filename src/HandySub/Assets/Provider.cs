using HandyControl.Controls;
using HandySub.Assets.Strings;

namespace HandySub.Assets
{
    public class Provider : ILocalizationProvider
    {
        public object Localize(string key)
        {
            return Lang.ResourceManager.GetObject(key);
        }
    }
}

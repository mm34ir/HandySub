using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace HandySub.Language
{
    public class ResxProvider : ILocalizationProvider
    {
        public IEnumerable<CultureInfo> Cultures => throw new NotImplementedException();

        public object Localize(string key)
        {
            return Language.Lang.ResourceManager.GetObject(key);
        }
    }
}

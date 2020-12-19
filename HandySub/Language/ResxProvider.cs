using System;
using System.Collections.Generic;
using System.Globalization;
using HandyControl.Controls;

namespace HandySub.Language
{
    public class ResxProvider : ILocalizationProvider
    {
        public IEnumerable<CultureInfo> Cultures => throw new NotImplementedException();

        public object Localize(string key)
        {
            return Lang.ResourceManager.GetObject(key);
        }
    }
}
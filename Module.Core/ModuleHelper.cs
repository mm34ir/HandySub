using Module.Core.Model;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Module.Core
{
    public class ModuleHelper
    {
        public static async Task<string> GetTitleByImdbId(string ImdbId, Action<string> errorCallBack = null)
        {
            string result = string.Empty;
            string url = $"http://www.omdbapi.com/?i={ImdbId}&apikey=2a59a17e";

           
        }

       
    }
}

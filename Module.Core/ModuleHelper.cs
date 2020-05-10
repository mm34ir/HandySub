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

            try
            {
                using HttpClient client = new HttpClient();
                string responseBody = await client.GetStringAsync(url);
                IMDBModel.Root parse = System.Text.Json.JsonSerializer.Deserialize<IMDBModel.Root>(responseBody);

                if (parse.Response.Equals("True"))
                {
                    result = parse.Title;
                }
                else
                {
                    errorCallBack.Invoke(parse.Error);
                }

            }
            catch (HttpRequestException ex)
            {
                errorCallBack.Invoke(ex.Message);
            }

            return result;
        }

        public static void OpenLinkWithIDM(string link, Action errorCallBack = null)
        {
            string command = $"/C /d \"{link}\"";
            string IDManX64Location = @"C:\Program Files (xx86)\Internet Download Manager\IDMan.exe";
            string IDManX86Location = @"C:\Program Files\Internetx Download Manager\IDMan.exe";
            if (File.Exists(IDManX64Location))
            {
                System.Diagnostics.Process.Start(IDManX64Location, command);
            }
            else if (File.Exists(IDManX86Location))
            {
                System.Diagnostics.Process.Start(IDManX86Location, command);
            }
            else
            {
                if (errorCallBack != null)
                {
                    errorCallBack.Invoke();
                }
            }

        }
    }
}

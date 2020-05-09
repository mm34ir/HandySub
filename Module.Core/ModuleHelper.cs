using Module.Core.Model;
using System;
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

        public static void OpenLinkWithIDM(string link)
        {
            string strCmdText = $"/C /d \"{link}\"";
            try
            {
                System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Internet Download Manager\IDMan.exe", strCmdText);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                System.Diagnostics.Process.Start(@"C:\Program Files\Internet Download Manager\IDMan.exe", strCmdText);
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HandySub.Model;

namespace HandySub
{
    public class Helper
    {
        public static string ISubtitleBaseAddress = "https://isubtitles.org";

        public static async Task<string> GetTitleByImdbId(string ImdbId, Action<string> errorCallBack = null)
        {
            var result = string.Empty;
            var url = $"http://www.omdbapi.com/?i={ImdbId}&apikey=2a59a17e";

            try
            {
                using var client = new HttpClient();
                var responseBody = await client.GetStringAsync(url);
                var parse = JsonSerializer.Deserialize<IMDBModel.Root>(responseBody);

                if (parse.Response.Equals("True"))
                    result = parse.Title;
                else
                    errorCallBack.Invoke(parse.Error);
            }
            catch (HttpRequestException ex)
            {
                errorCallBack.Invoke(ex.Message);
            }

            return result;
        }

        public static void OpenLinkWithIDM(string link, Action errorCallBack = null)
        {
            var command = $"/C /d \"{link}\"";
            var IDManX64Location = @"C:\Program Files (x86)\Internet Download Manager\IDMan.exe";
            var IDManX86Location = @"C:\Program Files\Internet Download Manager\IDMan.exe";
            if (File.Exists(IDManX64Location))
            {
                Process.Start(IDManX64Location, command);
            }
            else if (File.Exists(IDManX86Location))
            {
                Process.Start(IDManX86Location, command);
            }
            else
            {
                if (errorCallBack != null) errorCallBack.Invoke();
            }
        }
    }
}
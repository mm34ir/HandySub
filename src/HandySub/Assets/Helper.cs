using HandyControl.Controls;
using HandyControl.Tools.Extension;
using HandySub.Assets.Strings;
using HandySub.Models;
using ModernWpf.Controls;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace HandySub.Assets
{
    public class Helper
    {
        public static ISettings Settings = JsonSettings.Load<ISettings>().EnableAutosave();

        public static async Task<string> GetImdbIdFromTitle(string ImdbId, Action<string> errorCallBack = null)
        {
            var result = string.Empty;
            var url = Consts.IMDBIDAPI.Format(ImdbId);

            try
            {
                using var client = new HttpClient();
                var responseBody = await client.GetStringAsync(url);
                var parse = JsonSerializer.Deserialize<IMDBModel>(responseBody);

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

        public static void StartProcess(string path)
        {
            try
            {
                var ps = new ProcessStartInfo(path)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
            catch (Win32Exception ex)
            {
                if (!ex.Message.Contains("The system cannot find the file specified."))
                {
                    Growl.ErrorGlobal(ex.Message);
                }
            }
        }
        public static void OpenLinkWithIDM(string link, Action errorCallBack = null)
        {
            var command = $"/C /d \"{link}\"";
            if (File.Exists(Consts.IDManX64Location))
            {
                Process.Start(Consts.IDManX64Location, command);
            }
            else if (File.Exists(Consts.IDManX86Location))
            {
                Process.Start(Consts.IDManX86Location, command);
            }
            else
            {
                if (errorCallBack != null) errorCallBack.Invoke();
            }
        }

        public static async Task<string> GetRedirectedUrl(string url)
        {
            //this allows you to set the settings so that we can get the redirect url
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            string redirectedUrl = null;

            using (HttpClient client = new HttpClient(handler))
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                // ... Read the response to see if we have the redirected url
                if (response.StatusCode == System.Net.HttpStatusCode.Found)
                {
                    HttpResponseHeaders headers = response.Headers;
                    if (headers != null && headers.Location != null)
                    {
                        redirectedUrl = headers.Location.AbsoluteUri;
                    }
                }
            }

            return redirectedUrl;
        }

        public static void AddHistory(string item)
        {
            int historyCount = Settings.History.Count;
            if (!Settings.History.Exists(x => x.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1))
            {
                if (historyCount > Settings.MaxHistoryNumber)
                {
                    Settings.History.RemoveAt(0);
                }
                Settings.History.Add(item);

                Settings.History = new List<string>(Settings.History);
            }
        }

        public static void OnClearHistory()
        {
            Settings.History = new List<string>();
        }

        public static void LoadHistory(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args, AutoSuggestBox autoBox)
        {
            var suggestions = new List<string>();
            var history = Settings.History;
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var querySplit = sender.Text.Split(' ');
                var matchingItems = history.Where(
                    item =>
                    {
                        bool flag = true;
                        foreach (string queryToken in querySplit)
                        {
                            if (item.IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) < 0)
                            {
                                flag = false;
                            }

                        }
                        return flag;
                    });
                foreach (var item in matchingItems)
                {
                    suggestions.Add(item);
                }
                if (suggestions.Count > 0)
                {
                    for (int i = 0; i < suggestions.Count; i++)
                    {
                        autoBox.ItemsSource = suggestions;
                    }
                }
                else
                {
                    autoBox.ItemsSource = new string[] { Lang.ResourceManager.GetString("SuggestNotFound") };
                }
            }
        }
    }
}

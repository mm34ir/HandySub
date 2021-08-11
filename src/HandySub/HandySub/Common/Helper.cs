using Nucs.JsonSettings.Modulation.Recovery;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Autosave;
using System.Collections.Generic;
using System;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using HandySub.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Win32;
using System.IO;
using System.Net.Http.Headers;
using Windows.UI;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace HandySub.Common
{
    public static class Helper
    {
        public static HandySubConfig Settings = JsonSettings.Configure<HandySubConfig>()
                                   .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                                   .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                                   .LoadNow()
                                   .EnableAutosave();

        public static void SetImportedSettings(HandySubConfig handySubConfig)
        {
            Settings.SubtitleLanguage = handySubConfig.SubtitleLanguage;
            Settings.DefaultDownloadLocation = handySubConfig.DefaultDownloadLocation;
            Settings.FileNameRegex = handySubConfig.FileNameRegex;
            Settings.SubtitleQuality = handySubConfig.SubtitleQuality;
            Settings.SubsceneServer = handySubConfig.SubsceneServer;
            Settings.ShellServer = handySubConfig.ShellServer;
            Settings.IsFirstRun = handySubConfig.IsFirstRun;
            Settings.IsIDMEnabled = handySubConfig.IsIDMEnabled;
            Settings.IsAddToContextMenuEnabled = handySubConfig.IsAddToContextMenuEnabled;
            Settings.IsOpenHandySubWithContextMenuEnabled = handySubConfig.IsOpenHandySubWithContextMenuEnabled;
            Settings.IsShowNotificationEnabled = handySubConfig.IsShowNotificationEnabled;
            Settings.IsAutoDeCompressEnabled = handySubConfig.IsAutoDeCompressEnabled;
            Settings.IsDoubleClickEnabled = handySubConfig.IsDoubleClickEnabled;
            Settings.IsDefaultRegexEnabled = handySubConfig.IsDefaultRegexEnabled;
            Settings.SearchHistory = handySubConfig.SearchHistory;
            Settings.ApplicationTheme = handySubConfig.ApplicationTheme;
        }

        public static async Task<(bool IsDroped, string Name)> GridDrop(DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems) && (await e.DataView.GetStorageItemsAsync()).Count == 1)
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var cleanName = RemoveSpecialWords(Path.GetFileNameWithoutExtension(items[0].Name));
                return (IsDroped: true, Name: cleanName);
            }
            return (IsDroped: false, Name: null);
        }
        public static string RemoveSpecialWords(string stringToClean)
        {
            Regex wordFilter = new Regex(Settings.FileNameRegex, RegexOptions.IgnoreCase);
            var cleaned = Regex.Replace(stringToClean, @"S[0-9].{1}E[0-9].{1}", "", RegexOptions.IgnoreCase); // remove SXXEXX ==> X is 0-9
            cleaned = Regex.Replace(cleaned, @"(\[[^\]]*\])|(\([^\)]*\))", ""); // remove between () and []

            cleaned = wordFilter.Replace(cleaned, " ").Trim();
            cleaned = Regex.Replace(cleaned, "[ ]{2,}", " "); // remove space [More than 2 space] and replace with one space

            return cleaned.Trim();
        }
        public static Color GetColorFromHex(string hexaColor)
        {
            return
                Color.FromArgb(
                  Convert.ToByte(hexaColor.Substring(1, 2), 16),
                    Convert.ToByte(hexaColor.Substring(3, 2), 16),
                    Convert.ToByte(hexaColor.Substring(5, 2), 16),
                    Convert.ToByte(hexaColor.Substring(7, 2), 16)
                );
        }
        public static List<string> SubtitleLanguage { get; set; } = new List<string>
        {
            "All",
                "Persian",
                "English",
                "Albanian",
                "Arabic",
                "Bengali",
                "Brazillian",
                "Burmese",
                "Croatian",
                "Danish",
                "Dutch",
                "Finnish",
                "French",
                "German",
                "Hebrew",
                "Hindi",
                "Indonesian",
                "Italian",
                "Japanese",
                "Korean",
                "Malay",
                "Malayalam",
                "Morwegian",
                "Romanian",
                "Russian",
                "Serbian",
                "Spanish",
                "Swedish",
                "Tamil",
                "Thai",
                "Turkish",
                "Urdu",
                "Vietnamese",
                "Hungarian",
                "Portuguese"
        };
        public static List<string> SubtitleQuality { get; set; } = new List<string>
        {
            "All",
            "1080",
            "720",
            "480",
            "AMZN",
            "Bluray",
            "CMRG",
            "EVO",
            "GalaxyRG",
            "H264",
            "H265",
            "HDRip",
            "HDTV",
            "HEVC",
            "ION10",
            "NF",
            "Pahe",
            "PSA",
            "RARBG",
            "RMTeam",
            "SPARKS",
            "Web",
            "X264",
            "X265",
            "XviD",
            "YIFY",
            "YTS"
        };

        public static async Task<ObservableCollection<FavoriteKeyModel>> LoadFavorites()
        {
            List<FavoriteKeyModel> favorites = new List<FavoriteKeyModel>();
            if (File.Exists(Consts.FavoritePath))
            {
                var json = await File.ReadAllTextAsync(Consts.FavoritePath);
                favorites = JsonConvert.DeserializeObject<List<FavoriteKeyModel>>(json);
            }

            return new ObservableCollection<FavoriteKeyModel>(favorites);
        }

        public static async Task<bool> IsFavoriteExist(string key)
        {
            List<FavoriteKeyModel> favorites = new List<FavoriteKeyModel>();
            if (File.Exists(Consts.FavoritePath))
            {
                var json = await File.ReadAllTextAsync(Consts.FavoritePath);
                favorites = JsonConvert.DeserializeObject<List<FavoriteKeyModel>>(json);
            }

            return favorites.Where(x => x.Key == key).Any();
        }
        public static async void AddToFavorite(double rate, FavoriteKeyModel favorite)
        {
            List<FavoriteKeyModel> favorites = new List<FavoriteKeyModel>();
            if (File.Exists(Consts.FavoritePath))
            {
                var json = await File.ReadAllTextAsync(Consts.FavoritePath);
                favorites = JsonConvert.DeserializeObject<List<FavoriteKeyModel>>(json);
            }

            var currentItem = favorites.Where(item => item.Key.Equals(favorite.Key));

            if (rate == 1)
            {
                if (!currentItem.Any())
                {
                    favorites.Add(favorite);
                }
            }
            else
            {
                if (currentItem.Any())
                {
                    favorites.Remove(currentItem.SingleOrDefault());
                }
            }

            string serialize = JsonConvert.SerializeObject(favorites, Formatting.Indented);
            await File.WriteAllTextAsync(Consts.FavoritePath, serialize);
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

        public static void DeCompressAndNotification(string filename, Button button, XamlRoot xamlRoot)
        {
            string _filename = null;

            if (Settings.IsAutoDeCompressEnabled)
            {
                var result = IsDeCompressed(filename);
                if (result.IsSuccess)
                {
                    File.Delete(filename);
                    _filename = result.FileName;
                    button.Tag = result.FileName ?? filename;
                }
            }
            if (Settings.IsShowNotificationEnabled)
            {
                OpenContentDialog(_filename ?? filename, filename, xamlRoot);
            }
        }

        private static (bool IsSuccess, string FileName) IsDeCompressed(string filename)
        {
            string _filename = string.Empty;
            try
            {
                using (Stream stream = File.OpenRead(filename))
                using (var reader = ReaderFactory.Open(stream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            if (string.IsNullOrEmpty(_filename))
                            {
                                var entry = reader.Entry.Key.Replace("/", "\\");
                                _filename = @$"{Path.GetDirectoryName(filename)}\{entry}";
                            }
                            reader.WriteEntryToDirectory(Path.GetDirectoryName(filename), new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                return (IsSuccess: true, FileName: _filename);
            }
            catch (Exception)
            {
                return (IsSuccess: false, FileName: null);
            }
            return (IsSuccess: false, FileName: null);
        }

        public static async void OpenContentDialog(string filename, string content, XamlRoot xamlRoot)
        {
            if (filename != null)
            {
                ContentDialog dialog = new ContentDialog()
                {
                    Title = "Do you want to Open Subtitle Folder?",
                    Content = $"{Path.GetFileNameWithoutExtension(content)} Downloaded",
                    PrimaryButtonText = "Open",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = xamlRoot
                };
                var result = await dialog.ShowAsyncQueue();
                if (result == ContentDialogResult.Primary)
                {
                    OpenFolderAndSelectFile(filename);
                }
            }
        }

        public static void OpenFolderAndSelectFile(string filename)
        {
            string cmd = "/select, \"" + filename + "\"";
            if (!File.Exists(filename))
            {
                cmd = Path.GetDirectoryName(filename) + "\"";
            }

            Process.Start("explorer.exe", cmd);
        }
        public static void ShowErrorInfoBar(InfoBar infoBar, string message)
        {
            infoBar.Message = message;
            infoBar.IsOpen = true;
        }

        public static void ShowInfoBar(InfoBar infoBar, string title, string message, InfoBarSeverity severity)
        {
            infoBar.Title = title;
            infoBar.Message = message;
            infoBar.Severity = severity;
            infoBar.IsOpen = true;
        }

        public static bool IsInDoubleTapArea(DoubleTappedRoutedEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            var parent = VisualTreeHelper.GetParent(obj);
            if (parent.GetType().Equals(typeof(Grid)) || parent.GetType().Equals(typeof(ListViewItem)))
            {
                return true;
            }
            return false;
        }
        public static void AddToHistory(string item)
        {
            Settings.SearchHistory.AddIfNotExists(item);
            Settings.SearchHistory = new ObservableCollection<string>(Settings.SearchHistory);
        }
        public static void LoadHistory(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args, AutoSuggestBox autoBox)
        {
            var suggestions = new List<string>();
            var history = Settings.SearchHistory;
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
                    autoBox.ItemsSource = new string[] { "No result found" };
                }
            }
        }

        public static async Task<string> GetImdbIdFromTitle(string ImdbId, Action<string> errorCallBack = null)
        {
            var result = string.Empty;
            var url = string.Format(Consts.IMDBIDAPI, ImdbId);

            try
            {
                using var client = new HttpClient();
                var responseBody = await client.GetStringAsync(url);
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                var parse = System.Text.Json.JsonSerializer.Deserialize<IMDBModel>(responseBody, options);

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

        public static void OpenLinkWithIDM(string link)
        {
            var check = IsIDMExist();
            if (check.IsExist)
            {
                var command = $"/C /d \"{link}\"";
                Process.Start(check.ExePath, command);
            }
        }

        public static (bool IsExist, string ExePath) IsIDMExist()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DownloadManager"))
            {
                if (key != null)
                {
                    var value = key.GetValue("ExePath");
                    if (value != null)
                    {
                        return (IsExist: true, ExePath: value.ToString());
                    }
                }
            }
            return (IsExist: false, ExePath: string.Empty);
        }
    }
}

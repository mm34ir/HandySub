using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Tools;
using HandySub.Language;
using HandySub.Model;
using ModernWpf.Controls;
using Prism.Commands;

namespace HandySub
{
    public class Helper
    {
        public static Helper Current { get; } = new Helper();

        public string ISubtitleBaseAddress = "https://isubtitles.org";

        public async Task<string> GetTitleByImdbId(string ImdbId, Action<string> errorCallBack = null)
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

        public void OpenLinkWithIDM(string link, Action errorCallBack = null)
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

        public void AddAutoSuggestBoxContextMenu(FrameworkElement element)
        {
            var flyout = new MenuFlyout();
            flyout.Items.Add(new MenuItem()
                { Header = "Copy", InputGestureText = "Ctrl+S", Command = ApplicationCommands.Copy, Icon = new PathIcon() { Data = ResourceHelper.GetResource<Geometry>("CopyGeometry") } });
            flyout.Items.Add(new MenuItem()
                { Header = "Cut", InputGestureText = "Ctrl+X", Command = ApplicationCommands.Cut, Icon = new PathIcon() { Data = ResourceHelper.GetResource<Geometry>("CutGeometry") } });
            flyout.Items.Add(new MenuItem()
                { Header = "Paste", InputGestureText = "Ctrl+V", Command = ApplicationCommands.Paste, Icon = new PathIcon() { Data = ResourceHelper.GetResource<Geometry>("PasteGeometry") } });
            flyout.Items.Add(new Separator());
            flyout.Items.Add(new MenuItem()
                { Header = Lang.ClearHistory, Command = ClearHistoryCommand, Icon = new PathIcon() { Data = ResourceHelper.GetResource<Geometry>("ClearGeometry") } });

            ContextFlyoutService.SetContextFlyout(element, flyout);
        }

        public void AddHistory(string item)
        {
            int historyCount = GlobalDataHelper<AppConfig>.Config.History.Count;
            
            if (!GlobalDataHelper<AppConfig>.Config.History.Exists(x=>x.IndexOf(item, StringComparison.OrdinalIgnoreCase) != -1))
            {
                if (historyCount > 19)
                {
                    GlobalDataHelper<AppConfig>.Config.History.RemoveAt(0);
                }
                GlobalDataHelper<AppConfig>.Config.History.Add(item);
                GlobalDataHelper<AppConfig>.Save();
                GlobalDataHelper<AppConfig>.Init();
            }
        }
        #region ClearHistory
        private DelegateCommand _clearHistoryCommand;
        public DelegateCommand ClearHistoryCommand =>
            _clearHistoryCommand ?? (_clearHistoryCommand = new DelegateCommand(OnClearHistory, CanExecute));

        private bool CanExecute()
        {
            return GlobalDataHelper<AppConfig>.Config.History.Count > 0;
        }

        public void OnClearHistory()
        {
            GlobalDataHelper<AppConfig>.Config.History = new List<string>();
            GlobalDataHelper<AppConfig>.Save();
            GlobalDataHelper<AppConfig>.Init();
            ClearHistoryCommand.RaiseCanExecuteChanged();
        }
        #endregion
        public void LoadHistory(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args, AutoSuggestBox autoBox)
        {
            var suggestions = new List<string>();
            var history = GlobalDataHelper<AppConfig>.Config.History;
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
                    autoBox.ItemsSource = new string[] { Lang.NoResult };
                }
            }
        }
    }
}
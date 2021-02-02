using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Downloader;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using HandySub.Language;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using MessageBox = HandyControl.Controls.MessageBox;

namespace HandySub.ViewModels
{
    public class ISubtitlesDownloadViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        private string location = string.Empty;

        private string subtitleUrl = string.Empty;

        public ISubtitlesDownloadViewModel()
        {
            MainWindowViewModel.Instance.IsBackEnabled = true;

            OpenSubtitlePageCommand = new DelegateCommand<SelectionChangedEventArgs>(OpenSubtitlePage);
            RefreshCommand = new DelegateCommand(GetSubtitle);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var link = navigationContext.Parameters["link_key"] as string;
            if (!string.IsNullOrEmpty(link))
                subtitleUrl = link;
            GetSubtitle();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public bool KeepAlive => false;

        private async void OpenSubtitlePage(SelectionChangedEventArgs e)
        {
            IsBusy = true;
            IsEnabled = false;
            Progress = 0;
            if (e.AddedItems[0] is SubsceneDownloadModel item)
                try
                {
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(item.Link);

                    var downloadLink = Helper.Current.ISubtitleBaseAddress + doc?.DocumentNode
                        ?.SelectSingleNode("//div[@class='col-lg-16 col-md-24 col-sm-16']//a")?.Attributes["href"]
                        ?.Value;

                    location = GlobalData.Config.StoreLocation;

                    if (!GlobalData.Config.IsIDMEngine)
                    {
                        var downloader = new DownloadService();
                        downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                        downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                        await downloader.DownloadFileAsync(downloadLink, new DirectoryInfo(location));
                    }
                    else
                    {
                        IsBusy = false;
                        IsEnabled = true;
                        Helper.Current.OpenLinkWithIDM(downloadLink, IDMNotFound);
                    }
                }
                catch (WebException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Error(Lang.ResourceManager.GetString("AdminError"),
                        Lang.ResourceManager.GetString("AdminErrorTitle"));
                }
                catch (NotSupportedException)
                {
                }
                catch (ArgumentException)
                {
                }
                finally
                {
                    IsBusy = false;
                    IsEnabled = true;
                }
        }

        private void Downloader_DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            IsEnabled = true;
            IsBusy = false;
            if (GlobalData.Config.IsShowNotification)
            {
                var downlaodedFileName = ((DownloadPackage) e.UserState).FileName;

                Growl.ClearGlobal();
                Application.Current.Dispatcher.Invoke((Action) delegate
                {
                    Growl.AskGlobal(new GrowlInfo
                    {
                        CancelStr = Lang.ResourceManager.GetString("Cancel"),
                        ConfirmStr = Lang.ResourceManager.GetString("OpenFolder"),
                        Message = string.Format(Lang.ResourceManager.GetString("Downloaded"),
                            Path.GetFileNameWithoutExtension(downlaodedFileName)),
                        ActionBeforeClose = b =>
                        {
                            if (!b) return true;

                            Process.Start("explorer.exe", "/select, \"" + downlaodedFileName + "\"");
                            return true;
                        }
                    });
                });
            }
        }

        private async void GetSubtitle()
        {
            IsBusy = true;
            try
            {
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var table = doc.DocumentNode.SelectSingleNode("//table");

                if (table != null)
                {
                    var movieData = table.SelectNodes("//td[@data-title='Release / Movie']");
                    var commentData = table.SelectNodes("//td[@data-title='Comment']");

                    if (movieData != null)
                    {
                        DataList?.Clear();
                        int index = 0;
                        string title = string.Empty;
                        string href = string.Empty;
                        string comment = string.Empty;
                        foreach (var row in movieData)
                        {
                            var currentRow = row.SelectNodes("a");
                            foreach (var cell in currentRow)
                            {
                                title = cell?.InnerText?.Trim();
                                href = $"{Helper.Current.ISubtitleBaseAddress}{cell?.Attributes["href"]?.Value?.Trim()}";
                            }

                            comment = commentData[index]?.InnerText?.Trim();
                            if (comment != null && comment.Contains("&nbsp;"))
                                comment = comment.Replace("&nbsp;", "");

                            if (!string.IsNullOrEmpty(title))
                            {
                                var item = new SubsceneDownloadModel
                                    {Name = title, Translator = comment, Link = href};
                                DataList.Add(item);
                            }

                            index += 1;
                        }
                    }
                }
                else
                {
                    MessageBox.Error(Lang.ResourceManager.GetString("SubNotFound"));
                }

                IsBusy = false;
            }
            catch (ArgumentNullException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (WebException ex)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(Lang.ResourceManager.GetString("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void IDMNotFound()
        {
            MessageBox.Warning(LocalizationManager.Instance.Localize("IDMNot").ToString());
        }

        #region Property

        private double _progress;

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                DataList.ShapeView()
                    .Where(p => (p.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1) ||
                                (p.Translator.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) != -1))
                    .Apply();
            }
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private ObservableCollection<SubsceneDownloadModel> _datalist =
            new();

        public ObservableCollection<SubsceneDownloadModel> DataList
        {
            get => _datalist;
            set => SetProperty(ref _datalist, value);
        }

        #endregion

        #region Command

        public DelegateCommand<SelectionChangedEventArgs> OpenSubtitlePageCommand { get; }
        public DelegateCommand RefreshCommand { get; }

        #endregion
    }
}
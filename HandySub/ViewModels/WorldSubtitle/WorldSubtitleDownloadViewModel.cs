using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using Downloader;
using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Language;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

namespace HandySub.ViewModels
{
    public class WorldSubtitleDownloadViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        private string location = string.Empty;
        private string subtitleUrl = string.Empty;

        public WorldSubtitleDownloadViewModel()
        {
            MainWindowViewModel.Instance.IsBackEnabled = true;

            RefreshCommand = new DelegateCommand(GetSubtitle);
            DownloadCommand = new DelegateCommand<string>(OnDownload);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var link = navigationContext.Parameters["name_key"] as string;
            if (!string.IsNullOrEmpty(link)) subtitleUrl = link;

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

        private async void GetSubtitle()
        {
            try
            {
                IsBusy = true;

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(subtitleUrl);

                var items = doc.DocumentNode.SelectNodes(@"//div[@id='new-link']/ul/li");
                if (items == null)
                {
                    MessageBox.Error(LocalizationManager.Instance.Localize("SubNotFound").ToString());
                }
                else
                {
                    DataList?.Clear();
                    foreach (var node in items)
                    {
                        var displayName = node.SelectSingleNode(".//div[@class='new-link-1']").InnerText;
                        var status = node.SelectSingleNode(".//div[@class='new-link-2']").InnerText;
                        var link = node.SelectSingleNode(".//a")?.Attributes["href"]?.Value;

                        if (status.Contains("&nbsp;")) status = status.Replace("&nbsp;", "");

                        displayName = displayName.Trim() + " - " + status.Trim();

                        var item = new DownloadModel {DisplayName = displayName, DownloadLink = link};
                        DataList.Add(item);
                    }
                }

                IsBusy = false;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (ArgumentNullException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (WebException ex)
            {
                Growl.ErrorGlobal(
                    LocalizationManager.Instance.Localize("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(
                    LocalizationManager.Instance.Localize("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnDownload(string link)
        {
            try
            {
                if (!string.IsNullOrEmpty(link))
                {
                    IsBusy = true;
                    IsEnabled = false;
                    Progress = 0;
                    location = GlobalDataHelper<AppConfig>.Config.StoreLocation;
                    if (!GlobalDataHelper<AppConfig>.Config.IsIDMEngine)
                    {
                        var downloader = new DownloadService();
                        downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                        downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                        await downloader.DownloadFileAsync(link, new DirectoryInfo(location));
                    }
                    else
                    {
                        IsBusy = false;
                        IsEnabled = true;
                        Helper.OpenLinkWithIDM(link, IDMNotFound);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Error(LocalizationManager.Instance.Localize("AdminError").ToString(),
                    LocalizationManager.Instance.Localize("AdminErrorTitle").ToString());
            }
            catch (NotSupportedException)
            {
            }
            catch (ArgumentException)
            {
            }
        }

        private void IDMNotFound()
        {
            MessageBox.Warning(LocalizationManager.Instance.Localize("IDMNot").ToString());
        }

        private void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            IsEnabled = true;
            IsBusy = false;
            if (GlobalDataHelper<AppConfig>.Config.IsShowNotification)
            {
                var downlaodedFileName = ((DownloadPackage) e.UserState).FileName;
                Growl.ClearGlobal();
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
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }

        #region Property

        private ObservableCollection<DownloadModel> _dataList = new();

        public ObservableCollection<DownloadModel> DataList
        {
            get => _dataList;
            set => SetProperty(ref _dataList, value);
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

        private double _progress;

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        #endregion

        #region Command

        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand<string> DownloadCommand { get; }

        #endregion
    }
}
using Downloader;
using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Language;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace HandySub.ViewModels
{
    public class WorldSubtitleDownloadViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        public bool KeepAlive => false;
        private string subtitleUrl = string.Empty;
        private string location = string.Empty;

        #region Property
        private ObservableCollection<DownloadModel> _dataList = new ObservableCollection<DownloadModel>();
        public ObservableCollection<DownloadModel> DataList
        {
            get => _dataList;
            set => SetProperty(ref _dataList, value);
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        #endregion

        #region Command
        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand<string> DownloadCommand { get; private set; }
        #endregion
        public WorldSubtitleDownloadViewModel()
        {
            MainWindowViewModel.Instance.IsBackEnabled = true;

            RefreshCommand = new DelegateCommand(GetSubtitle);
            DownloadCommand = new DelegateCommand<string>(OnDownload);
        }

        private async void GetSubtitle()
        {
            try
            {
                IsBusy = true;

                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(subtitleUrl);

                HtmlNodeCollection items = doc.DocumentNode.SelectNodes(@"//div[@id='new-link']/ul/li");
                if (items == null)
                {
                    MessageBox.Error(LocalizationManager.Instance.Localize("SubNotFound").ToString());
                }
                else
                {
                    DataList?.Clear();
                    foreach (HtmlNode node in items)
                    {
                        string displayName = node.SelectSingleNode(".//div[@class='new-link-1']").InnerText;
                        string status = node.SelectSingleNode(".//div[@class='new-link-2']").InnerText;
                        string link = node.SelectSingleNode(".//a")?.Attributes["href"]?.Value;

                        if (status.Contains("&nbsp;"))
                        {
                            status = status.Replace("&nbsp;", "");
                        }
                        displayName = displayName.Trim() + " - " + status.Trim();

                        DownloadModel item = new DownloadModel { DisplayName = displayName, DownloadLink = link };
                        DataList.Add(item);
                    }
                }
                IsBusy = false;
            }
            catch (ArgumentOutOfRangeException) { }
            catch (ArgumentNullException) { }
            catch (System.NullReferenceException) { }
            catch (System.Net.WebException ex)
            {
                Growl.ErrorGlobal(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
            {
                Growl.ErrorGlobal(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            string link = navigationContext.Parameters["name_key"] as string;
            if (!string.IsNullOrEmpty(link))
            {
                subtitleUrl = link;
            }
            GetSubtitle();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

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
                HandyControl.Controls.MessageBox.Error(LocalizationManager.Instance.Localize("AdminError").ToString(), LocalizationManager.Instance.Localize("AdminErrorTitle").ToString());
            }
            catch (NotSupportedException) { }
            catch (ArgumentException) { }
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
                var downlaodedFileName = ((DownloadPackage)e.UserState).FileName;
                Growl.ClearGlobal();
                Growl.AskGlobal(new GrowlInfo
                {
                    CancelStr = Lang.ResourceManager.GetString("Cancel"),
                    ConfirmStr = Lang.ResourceManager.GetString("OpenFolder"),
                    Message = string.Format(Lang.ResourceManager.GetString("Downloaded"), Path.GetFileNameWithoutExtension(downlaodedFileName)),
                    ActionBeforeClose = b =>
                    {
                        if (!b)
                        {
                            return true;
                        }
                        System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + downlaodedFileName + "\"");
                        return true;

                    }
                });
            }
        }

        private void Downloader_DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }
    }
}

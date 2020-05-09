using ESubtitleModule.Models;
using HandyControl.Controls;
using HandyControl.Data;
using HtmlAgilityPack;
using Module.Core;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace ESubtitleModule.ViewModels
{
    public class ESubtitleDownloadViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        private readonly IRegionManager _regionManager;
        public bool KeepAlive => false;
        private string subtitleUrl = string.Empty;
        private readonly WebClient client = new WebClient();
        private string location = string.Empty;
        private string subName = string.Empty;

        #region Property
        private ObservableCollection<EDownloadModel> _dataList = new ObservableCollection<EDownloadModel>();
        public ObservableCollection<EDownloadModel> DataList
        {
            get => _dataList;
            set => SetProperty(ref _dataList, value);
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }

        #region ToggleButton

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private bool _isChecked = false;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        private int _progress = 0;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }
        #endregion

        #endregion

        #region Command
        public DelegateCommand GoBackCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand<string> DownloadCommand { get; private set; }
        #endregion
        public ESubtitleDownloadViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            GoBackCommand = new DelegateCommand(GoBack);
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

                HtmlNodeCollection items = doc.DocumentNode.SelectNodes("//a[@class='Download']");
                if (items == null)
                {
                    MessageBox.Error(LocalizationManager.Instance.Localize("SubNotFound").ToString());
                }
                else
                {
                    DataList?.Clear();
                    foreach (HtmlNode node in items)
                    {
                        EDownloadModel item = new EDownloadModel { DisplayName = node.SelectSingleNode(".//span[last()]").InnerText, DownloadLink = node.Attributes["href"].Value };
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
                Growl.Error(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
            {
                Growl.Error(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void GoBack()
        {
            _regionManager.RequestNavigate("ContentRegion", "ESubtitle");
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

        #region Downloader
        private void OnDownload(string link)
        {
            if (!string.IsNullOrEmpty(link))
            {
                if (!Convert.ToBoolean(GetConfig().IsIDMEngine))
                {
                    try
                    {
                        IsChecked = true;
                        IsEnabled = false;
                        Progress = 0;
                        subName = Path.GetFileName(link);
                        location = GetConfig().StoreLocation + subName;

                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                        client.DownloadFileAsync(new Uri(link), location);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        HandyControl.Controls.MessageBox.Error(LocalizationManager.Instance.Localize("AdminError").ToString(), LocalizationManager.Instance.Localize("AdminErrorTitle").ToString());
                    }
                    catch (NotSupportedException) { }
                    catch (ArgumentException) { }
                }
                else
                {
                    ModuleHelper.OpenLinkWithIDM(link);
                }
            }
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            Progress = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            IsChecked = false;
            IsEnabled = true;
            Progress = 0;
            if (Convert.ToBoolean(GetConfig().IsShowNotification))
            {
                Growl.Clear();
                Growl.Ask(new GrowlInfo
                {
                    CancelStr = LocalizationManager.Instance.Localize("Cancel").ToString(),
                    ConfirmStr = LocalizationManager.Instance.Localize("OpenFolder").ToString(),
                    Message = string.Format(LocalizationManager.Instance.Localize("Downloaded").ToString(), subName),
                    ActionBeforeClose = b =>
                    {
                        if (!b)
                        {
                            return true;
                        }
                        System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + location + "\"");
                        return true;

                    }
                });
            }
        }
        private dynamic GetConfig()
        {
            string configFile = File.ReadAllText("AppConfig.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(configFile);
        }
        #endregion
    }
}

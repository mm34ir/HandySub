using HandyControl.Controls;
using HandyControl.Data;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace HandySub.ViewModels
{
    public class WorldSubtitleDownloadViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        private readonly IRegionManager _regionManager;
        public bool KeepAlive => false;
        private string subtitleUrl = string.Empty;
        private readonly WebClient client = new WebClient();
        private string location = string.Empty;
        private string subName = string.Empty;

        #region Property
        private ObservableCollection<DownloadModel> _dataList = new ObservableCollection<DownloadModel>();
        public ObservableCollection<DownloadModel> DataList
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
        public WorldSubtitleDownloadViewModel(IRegionManager regionManager)
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
                        displayName = displayName + " - " + status;

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

        private void GoBack()
        {
            _regionManager.RequestNavigate("ContentRegion", "WorldSubtitle");
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
            try
            {
                if (!string.IsNullOrEmpty(link))
                {
                    bool IsIDMEngine = GetConfig().IsIDMEngine ?? false;

                    if (!IsIDMEngine)
                    {
                        IsChecked = true;
                        IsEnabled = false;
                        Progress = 0;
                        subName = Path.GetFileName(link);
                        string StoreLocation = GetConfig().StoreLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
                        location = StoreLocation + subName;

                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                        client.DownloadFileAsync(new Uri(link), location);
                    }
                    else
                    {
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
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            Progress = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                IsChecked = false;
                IsEnabled = true;
                Progress = 0;
                bool IsShowNotification = GetConfig().IsShowNotification ?? true;

                if (IsShowNotification)
                {
                    Growl.Clear();
                    Growl.AskGlobal(new GrowlInfo
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
            catch (Exception) { }
        }
        private dynamic GetConfig()
        {
            string configFile = File.ReadAllText("AppConfig.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(configFile);
        }
        #endregion
    }
}

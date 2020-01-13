using HandyControl.Controls;
using HandyControl.Data;
using HtmlAgilityPack;
using System;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Download : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        private string _Episode;
        public string Episode
        {
            get => _Episode;
            set
            {
                if (_Episode != value)
                {
                    _Episode = value;
                    NotifyPropertyChanged("Episode");
                }
            }
        }
        internal static Download download;
        private readonly WebClient client = new WebClient();

        public static string Link = string.Empty;
        public static string Translator { get; set; } = string.Empty;
        public static string Info { get; set; } = string.Empty;
        public static string LanguageTag { get; set; } = string.Empty;

        private string location = string.Empty;
        private string generatedLinks = string.Empty;
        private string subName = string.Empty;

        public Download()
        {
            InitializeComponent();
            DataContext = download = this;
        }

        public async void Load()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = await web.LoadFromWebAsync(Link);

            string downloadLink = doc.DocumentNode.SelectSingleNode(
                        "//div[@class='download']//a").GetAttributeValue("href", "nothing");
            generatedLinks = GlobalData.Config.ServerUrl + downloadLink;

            //get Episode info
            Regex regex = new Regex("S[0-9].{1}E[0-9].{1}");
            Match match = regex.Match(Info);
            if (match.Success)
            {
                Episode = match.Value;
            }

            if (GlobalData.Config.IsAutoDownloadSubtitle)
            {
                tgDownload_Click(null, null);
            }
        }

        #region Downloader

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            tgDownload.Progress = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            tgDownload.IsChecked = false;
            tgDownload.IsEnabled = true;
            tgDownload.Progress = 0;
            tgDownload.Content = Properties.Langs.Lang.OpenFolder;
            if (GlobalData.Config.IsShowNotification)
            {
                Growl.InfoGlobal(new GrowlInfo
                {
                    CancelStr = Properties.Langs.Lang.Cancel,
                    ConfirmStr = Properties.Langs.Lang.OpenFolder,
                    Message = string.Format(Properties.Langs.Lang.DownloadCompleted, Episode + subName),
                    ActionBeforeClose = isConfirmed =>
                    {
                        if (!isConfirmed)
                        {
                            return true;
                        }

                        System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + location + "\"");
                        return true;
                    }
                });
            }
        }

        private void tgDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((string)tgDownload.Content != Properties.Langs.Lang.OpenFolder)
                {
                    tgDownload.IsChecked = true;
                    tgDownload.IsEnabled = false;
                    tgDownload.Content = Properties.Langs.Lang.Downloading;
                    tgDownload.Progress = 0;

                    // we need to get file name
                    byte[] data = client.DownloadData(generatedLinks);

                    if (!string.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
                    {
                        subName = client.ResponseHeaders["Content-Disposition"].Substring(client.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                    }

                    // if luanched from ContextMenu set location next to the movie file
                    if (!string.IsNullOrEmpty(App.WindowsContextMenuArgument[0]))
                    {
                        location = App.WindowsContextMenuArgument[2] + Episode + subName;
                    }
                    else // get location from config
                    {
                        location = GlobalData.Config.StoreLocation + Episode + subName;
                    }
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    client.DownloadFileAsync(new Uri(generatedLinks), location);
                }
                else
                {
                    System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + location + "\"");
                }
            }
            catch (UnauthorizedAccessException)
            {
                HandyControl.Controls.MessageBox.Error(Properties.Langs.Lang.AccessError, Properties.Langs.Lang.AccessErrorTitle);
            }
            catch (NotSupportedException) { }
            catch (ArgumentException) { }
        }
        #endregion
    }
}

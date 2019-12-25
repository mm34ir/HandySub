using HandyControl.Controls;
using HtmlAgilityPack;
using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for Download.xaml
    /// </summary>
    public partial class Download : UserControl
    {
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
            tgDownload.Progress = 0;
            tgDownload.Content = Properties.Langs.Lang.OpenFolder;
            Growl.InfoGlobal(string.Format(Properties.Langs.Lang.DownloadCompleted, subName));
        }

        private void tgDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((string)tgDownload.Content != Properties.Langs.Lang.OpenFolder)
                {
                    tgDownload.IsChecked = true;

                    tgDownload.Content = Properties.Langs.Lang.Downloading;
                    tgDownload.Progress = 0;

                    // we need to get file name
                    byte[] data = client.DownloadData(generatedLinks);

                    if (!string.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
                    {
                        subName = client.ResponseHeaders["Content-Disposition"].Substring(client.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                    }

                    location = GlobalData.Config.StoreLocation + subName;
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
        }
        #endregion
    }
}

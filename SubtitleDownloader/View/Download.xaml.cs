using HandyControl.Controls;
using HandyControl.Data;
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
            DataContext = this;

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(Link);

            string downloadLink = doc.DocumentNode.SelectSingleNode(
                        "//div[@class='download']//a").GetAttributeValue("href", "nothing");
            generatedLinks = GlobalData.Config.ServerUrl + downloadLink;

            if (GlobalData.Config.IsAutoDownloadSubtitle)
            {
                btnDownload_Click(null, null);
            }
        }

        #region Downloader

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            prgStatus.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            openFolder.IsEnabled = true;
            btnDownload.Content = Properties.Langs.Lang.Download;
            Growl.InfoGlobal(string.Format(Properties.Langs.Lang.DownloadCompleted, subName));
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            btnDownload.Content = Properties.Langs.Lang.Downloading;
            prgStatus.Value = 0;

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
        #endregion

        private void openFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + location + "\"");
        }
    }
}

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
        public static string Link = string.Empty;
        public static string Translator { get; set; } = string.Empty;
        public static string Info { get; set; } = string.Empty;
        public static string LanguageTag { get; set; } = string.Empty;

        private string location = string.Empty;
        private string generatedLinks = string.Empty;
        private readonly string BaseUrl = "https://subf2m.co";
        private string SubName = string.Empty;

        public Download()
        {
            InitializeComponent();
            DataContext = this;

            string url = Link;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            string downloadLink = doc.DocumentNode.SelectSingleNode(
                        "//div[@class='download']//a").GetAttributeValue("href", "nothing");
            generatedLinks = BaseUrl + downloadLink;

            string getAutoDownload = InIHelper.ReadValue(SettingsKey.AutoDownload);
            if (string.IsNullOrEmpty(getAutoDownload))
            {
                getAutoDownload = "false";
            }
            if (Convert.ToBoolean(getAutoDownload))
            {
                btnDownload_Click(null, null);
            }
            HandyControl.Controls.MessageBox.Show("jkh");
        }

        #region Downloader

        private readonly WebClient client = new WebClient();


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
            Growl.InfoGlobal(new GrowlInfo { Message = string.Format(Properties.Langs.Lang.DownloadCompleted, SubName), ShowDateTime = true });
        }
        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            btnDownload.Content = Properties.Langs.Lang.Downloading;
            prgStatus.Value = 0;

            string getLocation = InIHelper.ReadValue(SettingsKey.Location);
            if (string.IsNullOrEmpty(getLocation))
            {
                getLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
            }
            byte[] data = client.DownloadData(generatedLinks);
            string fileName = "";

            if (!string.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
            {
                fileName = client.ResponseHeaders["Content-Disposition"].Substring(client.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
            }
            SubName = fileName;
            location = getLocation + fileName;
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

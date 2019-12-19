using HandyControl.Controls;
using HandyControl.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = HandyControl.Controls.MessageBox;

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
        string location = string.Empty;
        string generatedLinks = string.Empty;
        string BaseUrl = "https://subf2m.co";
        string SubName = string.Empty;

        public Download()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        #region Downloader

        WebClient client = new WebClient();


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

            var getLocation = InIHelper.ReadValue(SettingsKey.Location);
            if (string.IsNullOrEmpty(getLocation))
            {
                getLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
            }
            var data = client.DownloadData(generatedLinks);
            string fileName = "";

            if (!String.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var url = Link;
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var downloadLink = doc.DocumentNode.SelectSingleNode(
                        "//div[@class='download']//a").GetAttributeValue("href", "nothing");
            generatedLinks = BaseUrl + downloadLink;

            var getAutoDownload = InIHelper.ReadValue(SettingsKey.AutoDownload);
            if (string.IsNullOrEmpty(getAutoDownload))
            {
                getAutoDownload = "false";
            }
            if (Convert.ToBoolean(getAutoDownload))
            {
                btnDownload_Click(null, null);
            }
        }
    }
}

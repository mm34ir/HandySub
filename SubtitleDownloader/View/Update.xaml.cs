using HandyControl.Controls;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : UserControl
    {
        public Update()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateHelper.GithubReleaseModel ver = UpdateHelper.CheckForUpdateGithubRelease("ghost1372", "SubtitleDownloader");
                lblCreatedAt.Text = ver.CreatedAt.ToString();
                lblPublishedAt.Text = ver.PublishedAt.ToString();
                lblDownloadUrl.CommandParameter = lblDownloadUrl.Content = ver.Asset[0].browser_download_url;
                lblCurrentVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                lblVersion.Text = ver.TagName.Replace("v", "");
                txtChangelog.Text = ver.Changelog;
                if (ver.IsExistNewVersion)
                {
                    Growl.InfoGlobal(Properties.Langs.Lang.NewVersion);
                }
                else
                {
                    Growl.ErrorGlobal(Properties.Langs.Lang.LatestVersion);
                }
            }
            catch (System.Exception)
            {

                Growl.ErrorGlobal(Properties.Langs.Lang.ReleaseNotFound);
            }
        }
    }
}

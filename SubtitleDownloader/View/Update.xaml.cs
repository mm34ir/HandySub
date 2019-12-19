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
            UpdateHelper.GithubReleaseModel ver = UpdateHelper.CheckForUpdateGithubRelease("ghost1372", "MoalemYar");
            lblCreatedAt.Text = ver.CreatedAt.ToString();
            lblPublishedAt.Text = ver.PublishedAt.ToString();
            lblDownloadUrl.Content = $"https://github.com/ghost1372/MoalemYar/releases/tag/{ver.Version}";
            lblDownloadUrl.CommandParameter = $"https://github.com/ghost1372/MoalemYar/releases/tag/{ver.Version}";
            lblCurrentVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            lblVersion.Text = ver.Version.Replace("v", "");
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
    }
}

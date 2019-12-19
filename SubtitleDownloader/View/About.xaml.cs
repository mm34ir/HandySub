using System.Reflection;
using System.Windows.Controls;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public About()
        {
            InitializeComponent();
            txtVersion.Text = Properties.Langs.Lang.Version + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}

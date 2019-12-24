using System.Windows;
using System.Windows.Media.Imaging;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for AvatarWorld.xaml
    /// </summary>
    public partial class AvatarWorld
    {
        public AvatarWorld()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
           "Source", typeof(BitmapFrame), typeof(AvatarWorld), new PropertyMetadata(default(BitmapFrame)));

        public BitmapFrame Source
        {
            get => (BitmapFrame)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register(
            "DisplayName", typeof(string), typeof(AvatarWorld), new PropertyMetadata(default(string)));

        public string DisplayName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public static readonly DependencyProperty LinkProperty = DependencyProperty.Register(
            "Link", typeof(string), typeof(AvatarWorld), new PropertyMetadata(default(string)));

        public string Link
        {
            get => (string)GetValue(LinkProperty);
            set => SetValue(LinkProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WorldSubtitleDownload.Link = Link;
            MainWindow.mainWindow.CreateTabItem(new WorldSubtitleDownload(), DisplayName);
        }
    }
}

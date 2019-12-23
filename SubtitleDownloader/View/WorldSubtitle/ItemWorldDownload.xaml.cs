using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SubtitleDownloader
{
    /// <summary>
    /// Interaction logic for ItemWorldDownload.xaml
    /// </summary>
    public partial class ItemWorldDownload 
    {
        public ItemWorldDownload()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
           "Source", typeof(BitmapFrame), typeof(ItemWorldDownload), new PropertyMetadata(default(BitmapFrame)));

        public BitmapFrame Source
        {
            get => (BitmapFrame)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register(
            "DisplayName", typeof(string), typeof(ItemWorldDownload), new PropertyMetadata(default(string)));

        public string DisplayName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public static readonly DependencyProperty LinkProperty = DependencyProperty.Register(
            "Link", typeof(string), typeof(ItemWorldDownload), new PropertyMetadata(default(string)));

        public string Link
        {
            get => (string)GetValue(LinkProperty);
            set => SetValue(LinkProperty, value);
        }
    }
}

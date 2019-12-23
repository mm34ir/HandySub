using HtmlAgilityPack;
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
    /// Interaction logic for WorldSubtitleDownload.xaml
    /// </summary>
    public partial class WorldSubtitleDownload
    {
        public static string Link = string.Empty;

        public WorldSubtitleDownload()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = await web.LoadFromWebAsync(Link);

            var xxxx = doc.DocumentNode.SelectSingleNode("//div[@class='single-box single-dl']//div[@class='single-box-body']");
            foreach ((var item, int index) in xxxx.SelectNodes("//div[@class='new-link-1']").WithIndex())
            {
                var x = item.SelectNodes("//div[@class='new-link-2']")[index].InnerText;
                
                var xx = item.SelectNodes("//div[@class='new-link-3']//a")[index];

                MessageBox.Show(item.InnerText);
                MessageBox.Show(x);
                MessageBox.Show(xx.Attributes["href"].Value);

            }
            //foreach ((var cell, int index) in doc.DocumentNode.SelectNodes("//div[@class='single-box single-dl']//div[@class='single-box-body']").WithIndex())
            //{
            //    var x = cell.SelectSingleNode("//div[@class='new-link-1']").InnerText;
            //    var xx = cell.SelectNodes("//div[@class='new-link-1']")[index].InnerText;
            //    //var x = node.SelectSingleNode(".//a").Attributes["href"].Value;
            //    MessageBox.Show(x);
            //    MessageBox.Show(xx);

            //}
        }
    }
}

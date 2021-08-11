using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using Windows.UI;

namespace HandySub.DiffViewer
{
    public class BrushFactory
    {
        public Dictionary<Color, SolidColorBrush> Brushes = new Dictionary<Color, SolidColorBrush>();

        public SolidColorBrush GetOrCreateSolidColorBrush(Color color)
        {
            if (Brushes.ContainsKey(color))
            {
                return Brushes[color];
            }
            else
            {
                Brushes[color] = new SolidColorBrush(color);
                return Brushes[color];
            }
        }
    }
}

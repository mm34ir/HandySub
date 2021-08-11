using HandySub.Common;
using HandySub.Models;
using HandySub.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Linq;

namespace HandySub.UserControls
{
    public sealed partial class FavoriteUserControl : UserControl
    {
        #region DependencyProperty
        public static readonly DependencyProperty KeyProperty =
        DependencyProperty.Register("Key", typeof(string), typeof(FavoriteUserControl),
           new PropertyMetadata(string.Empty));

        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(FavoriteUserControl),
            new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(string), typeof(FavoriteUserControl),
            new PropertyMetadata(string.Empty));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ServerProperty =
        DependencyProperty.Register("Server", typeof(Server), typeof(FavoriteUserControl),
            new PropertyMetadata(Server.Subscene));

        public Server Server
        {
            get { return (Server)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }
        #endregion
        public FavoriteUserControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse ||
                e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                VisualStateManager.GoToState(sender as Control, "HoverButtonsShown", true);
            }
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(sender as Control, "HoverButtonsHidden", true);
        }

        private void DeleteHoverButton_Click(object sender, RoutedEventArgs e)
        {
            var currentItem = new FavoriteKeyModel { Key = Key, Server = Server, Title = Title, Value = Value };
            if (currentItem != null)
            {
                var item = FavoritePage.Instance.Favorites.Where(item => item.Key.Equals(currentItem.Key));

                FavoritePage.Instance.Favorites.Remove(item.SingleOrDefault());
                Helper.AddToFavorite(0, currentItem);
                FavoritePage.Instance.ShowEmptyNotify();
            }
        }
    }
}

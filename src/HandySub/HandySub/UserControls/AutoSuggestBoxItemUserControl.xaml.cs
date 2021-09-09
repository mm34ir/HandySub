using HandySub.Common;
using HandySub.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HandySub.UserControls
{
    public sealed partial class AutoSuggestBoxItemUserControl : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
       DependencyProperty.Register("Title", typeof(string), typeof(AutoSuggestBoxItemUserControl),
           new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public AutoSuggestBoxItemUserControl()
        {
            this.InitializeComponent();
        }

        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            var history = Helper.Settings.SearchHistory;
            history.Remove(Title);
            Helper.Settings.SearchHistory = history;
            if (SubscenePage.Instance != null)
            {
                SubscenePage.Instance.RefreshAutoSuggestTextChanged();
            }
            if (ESubtitlePage.Instance != null)
            {
                ESubtitlePage.Instance.RefreshAutoSuggestTextChanged();
            }
            if (ISubtitlePage.Instance != null)
            {
                ISubtitlePage.Instance.RefreshAutoSuggestTextChanged();
            }
            if (WorldSubtitlePage.Instance != null)
            {
                WorldSubtitlePage.Instance.RefreshAutoSuggestTextChanged();
            }
            if (IMDBPage.Instance != null)
            {
                IMDBPage.Instance.RefreshAutoSuggestTextChanged();
            }
        }
    }
}

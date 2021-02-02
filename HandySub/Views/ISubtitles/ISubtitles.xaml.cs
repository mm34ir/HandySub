using System.Windows.Controls;
using HandySub.ViewModels;
using ModernWpf.Controls;

namespace HandySub.Views
{
    /// <summary>
    /// Interaction logic for ISubtitles.xaml
    /// </summary>
    public partial class ISubtitles : UserControl
    {
        public ISubtitles()
        {
            InitializeComponent();
            Helper.Current.AddAutoSuggestBoxContextMenu(autoBox);
        }

        private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            Helper.Current.LoadHistory(sender, args, autoBox);
        }

        private void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.QueryText))
            {
                Helper.Current.AddHistory(args.QueryText);
                ISubtitlesViewModel.Instance.OnSearchStarted(args.QueryText);
            }
        }
    }
}
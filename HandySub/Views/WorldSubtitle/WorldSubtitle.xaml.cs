using System.Windows.Controls;
using HandySub.ViewModels;
using ModernWpf.Controls;

namespace HandySub.Views
{
    /// <summary>
    ///     Interaction logic for WorldSubtitle
    /// </summary>
    public partial class WorldSubtitle : UserControl
    {
        public WorldSubtitle()
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
                WorldSubtitleViewModel.Instance.SearchText = args.QueryText;
                WorldSubtitleViewModel.Instance.OnSearchStarted();
            }
        }
    }
}
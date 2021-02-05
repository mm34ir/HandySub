using System.Windows.Controls;

namespace HandySub.Views
{
    /// <summary>
    ///     Interaction logic for Settings
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
            btnClear.IsEnabled = Helper.Current.ClearHistoryCommand.CanExecute();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Helper.Current.ClearHistoryCommand.Execute();
            btnClear.IsEnabled = Helper.Current.ClearHistoryCommand.CanExecute();
        }
    }
}
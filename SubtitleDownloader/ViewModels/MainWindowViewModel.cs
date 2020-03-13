using Prism.Mvvm;

namespace SubtitleDownloader.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Subtitle Downloader";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }


        public MainWindowViewModel()
        {
        }
    }
}

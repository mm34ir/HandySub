using Prism.Mvvm;
using System.Reflection;

namespace SubtitleDownloader.ViewModels
{
    public class AboutViewModel : BindableBase
    {
        private string _version;
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }
        public AboutViewModel()
        {
            Version = "نسخه " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}

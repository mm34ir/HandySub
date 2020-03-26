using Prism.Mvvm;
using SubtitleDownloader.Language;
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
            Version = string.Format(Lang.ResourceManager.GetString("Version"), Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}

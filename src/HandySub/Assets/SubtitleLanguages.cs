using HandyControl.Properties.Langs;
using System.Collections.ObjectModel;
namespace HandySub.Assets
{
    public class SubtitleLanguages
    {
        public static ObservableCollection<string> LoadSubtitleLanguage()
        {
            return new ObservableCollection<string>
            {
                Lang.ResourceManager.GetString("All"),
                "Persian",
                "English",
                "Albanian",
                "Arabic",
                "Bengali",
                "Brazillian",
                "Burmese",
                "Croatian",
                "Danish",
                "Dutch",
                "Finnish",
                "French",
                "German",
                "Hebrew",
                "Hindi",
                "Indonesian",
                "Italian",
                "Japanese",
                "Korean",
                "Malay",
                "Malayalam",
                "Morwegian",
                "Romanian",
                "Russian",
                "Serbian",
                "Spanish",
                "Swedish",
                "Tamil",
                "Thai",
                "Turkish",
                "Urdu",
                "Vietnamese",
                "Hungarian",
                "Portuguese"
            };
        }
    }
}
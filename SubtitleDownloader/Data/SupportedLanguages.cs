using SubtitleDownloader.Language;
using SubtitleDownloader.Model;
using System.Collections.ObjectModel;

namespace SubtitleDownloader.Data
{
    public class SupportedLanguages
    {
        private static ObservableCollection<LanguageModel> LanguageItems;
        public static ObservableCollection<LanguageModel> LoadSubtitleLanguage()
        {
            LanguageItems = new ObservableCollection<LanguageModel>
            {
                new LanguageModel { DisplayName = Lang.SLFarsi, LanguageCode = "farsi_persian", LocalizeCode = "SLFarsi" },
                new LanguageModel { DisplayName = Lang.SLEnglish, LanguageCode = "english", LocalizeCode = "SLEnglish" },
                new LanguageModel { DisplayName = Lang.SLAlbanian, LanguageCode = "albanian", LocalizeCode = "SLAlbanian" },
                new LanguageModel { DisplayName = Lang.SLArabic, LanguageCode = "arabic", LocalizeCode = "SLArabic" },
                new LanguageModel { DisplayName = Lang.SLBengali, LanguageCode = "bengali", LocalizeCode = "SLBengali" },
                new LanguageModel { DisplayName = Lang.SLBrazilian, LanguageCode = "brazillian-portuguese", LocalizeCode = "SLBrazilian" },
                new LanguageModel { DisplayName = Lang.SLBurmese, LanguageCode = "burmese", LocalizeCode = "SLBurmese" },
                new LanguageModel { DisplayName = Lang.SLCroatian, LanguageCode = "croatian", LocalizeCode = "SLCroatian" },
                new LanguageModel { DisplayName = Lang.SLDanish, LanguageCode = "danish", LocalizeCode = "SLDanish" },
                new LanguageModel { DisplayName = Lang.SLDutch, LanguageCode = "dutch", LocalizeCode = "SLDutch" },
                new LanguageModel { DisplayName = Lang.SLFinnish, LanguageCode = "finnish", LocalizeCode = "SLFinnish" },
                new LanguageModel { DisplayName = Lang.SLFrench, LanguageCode = "french", LocalizeCode = "SLFrench" },
                new LanguageModel { DisplayName = Lang.SLGerman, LanguageCode = "german", LocalizeCode = "SLGerman" },
                new LanguageModel { DisplayName = Lang.SLHebrew, LanguageCode = "hebrew", LocalizeCode = "SLHebrew" },
                new LanguageModel { DisplayName = Lang.SLHindi, LanguageCode = "hindi", LocalizeCode = "SLHindi" },
                new LanguageModel { DisplayName = Lang.SLIndonesian, LanguageCode = "indonesian", LocalizeCode = "SLIndonesian" },
                new LanguageModel { DisplayName = Lang.SLItalian, LanguageCode = "italian", LocalizeCode = "SLItalian" },
                new LanguageModel { DisplayName = Lang.SLJapanese, LanguageCode = "japanese", LocalizeCode = "SLJapanese" },
                new LanguageModel { DisplayName = Lang.SLKorean, LanguageCode = "korean", LocalizeCode = "SLKorean" },
                new LanguageModel { DisplayName = Lang.SLMalay, LanguageCode = "malay", LocalizeCode = "SLMalay" },
                new LanguageModel { DisplayName = Lang.SLMalayalam, LanguageCode = "malayalam", LocalizeCode = "SLMalayalam" },
                new LanguageModel { DisplayName = Lang.SLNorwegian, LanguageCode = "norwegian", LocalizeCode = "SLNorwegian" },
                new LanguageModel { DisplayName = Lang.SLRomanian, LanguageCode = "romanian", LocalizeCode = "SLRomanian" },
                new LanguageModel { DisplayName = Lang.SLRussian, LanguageCode = "russian", LocalizeCode = "SLRussian" },
                new LanguageModel { DisplayName = Lang.SLSerbian, LanguageCode = "serbian", LocalizeCode = "SLSerbian" },
                new LanguageModel { DisplayName = Lang.SLSpanish, LanguageCode = "spanish", LocalizeCode = "SLSpanish" },
                new LanguageModel { DisplayName = Lang.SLSwedish, LanguageCode = "swedish", LocalizeCode = "SLSwedish" },
                new LanguageModel { DisplayName = Lang.SLTamil, LanguageCode = "tamil", LocalizeCode = "SLTamil" },
                new LanguageModel { DisplayName = Lang.SLThai, LanguageCode = "thai", LocalizeCode = "SLThai" },
                new LanguageModel { DisplayName = Lang.SLTurkish, LanguageCode = "turkish", LocalizeCode = "SLTurkish" },
                new LanguageModel { DisplayName = Lang.SLUrdu, LanguageCode = "urdu", LocalizeCode = "SLUrdu" },
                new LanguageModel { DisplayName = Lang.SLVietnamese, LanguageCode = "vietnamese", LocalizeCode = "SLVietnamese" },
                new LanguageModel { DisplayName = Lang.SLHungarian, LanguageCode = "hungarian", LocalizeCode = "SLHungarian" },
                new LanguageModel { DisplayName = Lang.SLPortuguese, LanguageCode = "portuguese", LocalizeCode = "SLPortuguese" }
            };


            return LanguageItems;
        }
    }
}

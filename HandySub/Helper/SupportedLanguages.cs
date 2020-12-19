using System.Collections.ObjectModel;
using HandySub.Language;
using HandySub.Model;

namespace HandySub.Data
{
    public class SupportedLanguages
    {
        private static ObservableCollection<LanguageModel> LanguageItems;

        public static ObservableCollection<LanguageModel> LoadSubtitleLanguage()
        {
            LanguageItems = new ObservableCollection<LanguageModel>
            {
                new() {DisplayName = Lang.SLFarsi, LanguageCode = "farsi_persian", LocalizeCode = "SLFarsi"},
                new() {DisplayName = Lang.SLEnglish, LanguageCode = "english", LocalizeCode = "SLEnglish"},
                new() {DisplayName = Lang.SLAlbanian, LanguageCode = "albanian", LocalizeCode = "SLAlbanian"},
                new() {DisplayName = Lang.SLArabic, LanguageCode = "arabic", LocalizeCode = "SLArabic"},
                new() {DisplayName = Lang.SLBengali, LanguageCode = "bengali", LocalizeCode = "SLBengali"},
                new()
                {
                    DisplayName = Lang.SLBrazilian, LanguageCode = "brazillian-portuguese", LocalizeCode = "SLBrazilian"
                },
                new() {DisplayName = Lang.SLBurmese, LanguageCode = "burmese", LocalizeCode = "SLBurmese"},
                new() {DisplayName = Lang.SLCroatian, LanguageCode = "croatian", LocalizeCode = "SLCroatian"},
                new() {DisplayName = Lang.SLDanish, LanguageCode = "danish", LocalizeCode = "SLDanish"},
                new() {DisplayName = Lang.SLDutch, LanguageCode = "dutch", LocalizeCode = "SLDutch"},
                new() {DisplayName = Lang.SLFinnish, LanguageCode = "finnish", LocalizeCode = "SLFinnish"},
                new() {DisplayName = Lang.SLFrench, LanguageCode = "french", LocalizeCode = "SLFrench"},
                new() {DisplayName = Lang.SLGerman, LanguageCode = "german", LocalizeCode = "SLGerman"},
                new() {DisplayName = Lang.SLHebrew, LanguageCode = "hebrew", LocalizeCode = "SLHebrew"},
                new() {DisplayName = Lang.SLHindi, LanguageCode = "hindi", LocalizeCode = "SLHindi"},
                new() {DisplayName = Lang.SLIndonesian, LanguageCode = "indonesian", LocalizeCode = "SLIndonesian"},
                new() {DisplayName = Lang.SLItalian, LanguageCode = "italian", LocalizeCode = "SLItalian"},
                new() {DisplayName = Lang.SLJapanese, LanguageCode = "japanese", LocalizeCode = "SLJapanese"},
                new() {DisplayName = Lang.SLKorean, LanguageCode = "korean", LocalizeCode = "SLKorean"},
                new() {DisplayName = Lang.SLMalay, LanguageCode = "malay", LocalizeCode = "SLMalay"},
                new() {DisplayName = Lang.SLMalayalam, LanguageCode = "malayalam", LocalizeCode = "SLMalayalam"},
                new() {DisplayName = Lang.SLNorwegian, LanguageCode = "norwegian", LocalizeCode = "SLNorwegian"},
                new() {DisplayName = Lang.SLRomanian, LanguageCode = "romanian", LocalizeCode = "SLRomanian"},
                new() {DisplayName = Lang.SLRussian, LanguageCode = "russian", LocalizeCode = "SLRussian"},
                new() {DisplayName = Lang.SLSerbian, LanguageCode = "serbian", LocalizeCode = "SLSerbian"},
                new() {DisplayName = Lang.SLSpanish, LanguageCode = "spanish", LocalizeCode = "SLSpanish"},
                new() {DisplayName = Lang.SLSwedish, LanguageCode = "swedish", LocalizeCode = "SLSwedish"},
                new() {DisplayName = Lang.SLTamil, LanguageCode = "tamil", LocalizeCode = "SLTamil"},
                new() {DisplayName = Lang.SLThai, LanguageCode = "thai", LocalizeCode = "SLThai"},
                new() {DisplayName = Lang.SLTurkish, LanguageCode = "turkish", LocalizeCode = "SLTurkish"},
                new() {DisplayName = Lang.SLUrdu, LanguageCode = "urdu", LocalizeCode = "SLUrdu"},
                new() {DisplayName = Lang.SLVietnamese, LanguageCode = "vietnamese", LocalizeCode = "SLVietnamese"},
                new() {DisplayName = Lang.SLHungarian, LanguageCode = "hungarian", LocalizeCode = "SLHungarian"},
                new() {DisplayName = Lang.SLPortuguese, LanguageCode = "portuguese", LocalizeCode = "SLPortuguese"}
            };


            return LanguageItems;
        }
    }
}
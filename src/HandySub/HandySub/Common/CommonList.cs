using HandySub.Models;
using System.Collections.Generic;

namespace HandySub.Common
{
    public static class CommonList
    {
        public static List<string> Themes = new List<string>
        {
            "Light","Dark","Windows Default"
        };

        public static List<ServerModel> SubsceneServers = new List<ServerModel>
        {
            new ServerModel{ Index = 0, Key = "Subf2m", Url = "https://subf2m.co" },
            new ServerModel{ Index = 1, Key = "Delta Leech", Url = "https://sub.deltaleech.com" },
            new ServerModel{ Index = 2, Key = "Subscene", Url = "https://subscene.com" }
        };

        public static List<ServerModel> SubtitleServers = new List<ServerModel>
        {
            new ServerModel{ Index = 0, Key = "Subscene"},
            new ServerModel{ Index = 1, Key = "ESubtitle"},
            new ServerModel{ Index = 2, Key = "WorldSubtitle"},
            new ServerModel{ Index = 3, Key = "ISubtitles"}
        };
        public static List<string> SubtitleLanguage { get; set; } = new List<string>
        {
            "All",
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
        public static List<string> SubtitleQuality { get; set; } = new List<string>
        {
            "All",
            "1080",
            "720",
            "480",
            "AMZN",
            "Bluray",
            "CMRG",
            "EVO",
            "GalaxyRG",
            "H264",
            "H265",
            "HDRip",
            "HDTV",
            "HEVC",
            "ION10",
            "NF",
            "Pahe",
            "PSA",
            "RARBG",
            "RMTeam",
            "SPARKS",
            "Web",
            "X264",
            "X265",
            "XviD",
            "YIFY",
            "YTS"
        };
    }
}

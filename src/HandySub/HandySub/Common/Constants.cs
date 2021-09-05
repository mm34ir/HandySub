using System;
using System.Diagnostics;
using System.IO;

namespace HandySub.Common
{
    public static class Constants
    {
        public static readonly string AppName = "HandySubV4";
        public const string GetSubtitleContextMenuName = "Get Subtitle";
        public const string OpenHandySubContextMenuName = "Open HandySub";
        public static readonly string GetSubtitleShellCommand = $"\"{Process.GetCurrentProcess().MainModule.FileName}\" \"%1\"";
        public static readonly string OpenHandySubShellCommand = $"\"{Process.GetCurrentProcess().MainModule.FileName}\"";

        public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        public static readonly string AppConfigPath = Path.Combine(RootPath, "AppConfig.json");
        public static readonly string FavoritePath = Path.Combine(RootPath, "Favorite.json");
        public static readonly string CachePath = Path.Combine(RootPath, "Cache");
        public static readonly string OMDBAPI = "2a59a17e";

        public const string AppSecret = "3770b372-60d5-49a1-8340-36a13ae5fb71";

        public const string SubsceneSearchAPI = "{0}/subtitles/searchbytitle?query={1}&l=";
        public const string IMDBIDAPI = "http://www.omdbapi.com/?i={0}&apikey=2a59a17e";
        public const string IMDBTitleAPI = "http://www.omdbapi.com/?t={0}&apikey=2a59a17e";
        public const string IMDBBaseUrl = "https://www.imdb.com/title/{0}";
        public const string ESubtitleSearchAPI = "https://esubtitle.com/?s={0}";
        public const string WorldSubtitleSearchAPI = "http://worldsubtitle.site/?s={0}";
        public const string WorldSubtitlePageSearchAPI = "http://worldsubtitle.site/page/{0}?s=";
        public const string ISubtitleSearchAPI = "https://isubtitles.org/search?kwd={0}";
        public const string ISubtitleBaseUrl = "https://isubtitles.org";

        public const string FileNameRegex = @"(?:hd(?:tv|cam|r)|e(?:xm|vo)|RMT|DDP?5(?:\.1)?|YTS|Turkish|VideoFlix|Gisaengchung|Korean|8CH|BluRay|-|XVid|A(?:c3|VS)|web(?:-?(?:rip|dl))?|fgt|mp3|cmrg|pahe|10bit|(?:720|480|1080)[pi]?|H\.?26[45]|x26[45]|\d{3,}MB|H(?:MAX|EVC)|PS(?:A|iG)|RARBG|[26]CH|(?:CAM)?Rip|RM(?:X|Team)|msd|sva|mkvcage|megusta|tbs|amz|shitbox|nitro|Mr(?:Movie|CS)|BWBP|NT[bG]|Atmos|MZABI|20(?:1\d|2[01])|\/|GalaxyRG|YTS(?:\.(?:LT|MX))?|DV|ION10|SYNCOPY|Phoenix|Minx|AFG|Cakes|@Gemovies|GM|AvaMovie|FB|M3|\.)";

        public const string ALL_Language = "All";
        public const string ALL_Qualities = "All";
        public const string GuidSubtitle = "Avengers: Infinity War";
    }
}

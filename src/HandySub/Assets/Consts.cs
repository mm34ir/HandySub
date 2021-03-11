using System;
using System.Diagnostics;
using System.IO;

namespace HandySub.Assets
{
    public abstract class Consts
    {
        public static readonly string AppName = "HandySub";
        public static readonly string ContextMenuName = "Get Subtitle";
        public static readonly string ShellCommand = $"\"{Process.GetCurrentProcess().MainModule.FileName}\" \"%1\"";
        public static readonly string VersionKey = "VersionCode";

        public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
        public static readonly string ConfigPath = Path.Combine(RootPath, "Config.json");
        public static readonly string HistoryPath = Path.Combine(RootPath, "History.txt");
        public static readonly string CachePath = Path.Combine(RootPath, "Cache");
        
        public static readonly string AppSecret = "3770b372-60d5-49a1-8340-36a13ae5fb71";

        public static readonly string IDManX64Location = @"C:\Program Files (x86)\Internet Download Manager\IDMan.exe";
        public static readonly string IDManX86Location = @"C:\Program Files\Internet Download Manager\IDMan.exe";

        public static readonly string SubsceneSearchAPI = "{0}/subtitles/searchbytitle?query={1}&l=";
        public static readonly string IMDBIDAPI = "http://www.omdbapi.com/?i={0}&apikey=2a59a17e";
        public static readonly string ESubtitleSearchAPI = "https://esubtitle.com/?s={0}";
        public static readonly string WorldSubtitleSearchAPI = "http://worldsubtitle.info/?s=";
        public static readonly string WorldSubtitlePageSearchAPI = "http://worldsubtitle.info/page/{0}?s=";
        public static readonly string ISubtitleSearchAPI = "https://isubtitles.org/search?kwd={0}";
        public static readonly string ISubtitleBaseUrl = "https://isubtitles.org";

    }
}

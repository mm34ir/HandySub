using HandyControl.Controls;
using Newtonsoft.Json;
using System;
using System.IO;

namespace SubtitleDownloader
{
    internal class GlobalData
    {
        public static void Init()
        {
            if (File.Exists(AppConfig.SavePath))
            {
                try
                {
                    var json = File.ReadAllText(AppConfig.SavePath);
                    Config = (string.IsNullOrEmpty(json) ? new AppConfig() : JsonConvert.DeserializeObject<AppConfig>(json)) ?? new AppConfig();

                }
                catch
                {
                    Config = new AppConfig();
                }
            }
            else
            {
                Config = new AppConfig();
            }
        }

        public static void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Config);
                File.WriteAllText(AppConfig.SavePath, json);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Error(Properties.Langs.Lang.AccessError, Properties.Langs.Lang.AccessErrorTitle);
            }
        }

        public static AppConfig Config { get; set; }

    }
}
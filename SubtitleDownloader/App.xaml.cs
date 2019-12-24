using HandyControl.Data;
using HandyControl.Tools;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;

namespace SubtitleDownloader
{
    public partial class App
    {
        public static string WindowsContextMenuArgument = string.Empty;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.ToLower()
                         .EndsWith(".resources") ||
                     !name.ToLower()
                          .EndsWith(".dll"))
                    continue;
                EmbeddedAssembly.Load(name,
                                       name);
            }

            GlobalData.Init();

            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                   typeof(Analytics), typeof(Crashes));
            AppCenter.Start("3770b372-60d5-49a1-8340-36a13ae5fb71",
                               typeof(Analytics), typeof(Crashes));

            ConfigHelper.Instance.SetLang(GlobalData.Config.UILang);

            if (GlobalData.Config.Skin != SkinType.Default)
            {
                UpdateSkin(GlobalData.Config.Skin);
            }

            if (e.Args.Length > 0)
            {
                WindowsContextMenuArgument = Path.GetFileNameWithoutExtension(e.Args[0]);
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
        internal void UpdateSkin(SkinType skin)
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/Skin{skin.ToString()}.xaml")
            });
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
            });
        }
        static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var fields = args.Name.Split(',');
            var name = fields[0];
            var culture = fields[2];
            if (name.EndsWith(".resources") &&
                 !culture.EndsWith("neutral"))
                return null;

            return EmbeddedAssembly.Get(args.Name);
        }
    }
}

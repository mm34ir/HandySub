using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using HandyControl.Controls;
using HandySub.Model;
using HtmlAgilityPack;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace HandySub.ViewModels
{
    public class ESubtitleViewModel : BindableBase, IRegionMemberLifetime
    {
        internal static ESubtitleViewModel Instance;
        private readonly IRegionManager _regionManager;

        private readonly List<string> wordsToRemove = "دانلود زیرنویس فارسی فیلم,دانلود زیرنویس فارسی سریال"
            .Split(',').ToList();

        public ESubtitleViewModel(IRegionManager regionManager)
        {
            Instance = this;
            MainWindowViewModel.Instance.IsBackEnabled = false;

            DataList.Clear();
            GoToLinkCommand = new DelegateCommand<string>(GotoLink);
            _regionManager = regionManager;
        }

        public bool KeepAlive => GlobalDataHelper<AppConfig>.Config.IsKeepAlive;

        private void GotoLink(string name)
        {
            var parameters = new NavigationParameters
                {{"name_key", name}};
            _regionManager.RequestNavigate("ContentRegion", "ESubtitleDownload", parameters);
        }

        public async void OnSearchStarted(string query)
        {
            if (string.IsNullOrEmpty(query)) return;

            try
            {
                DataList?.Clear();
                IsBusy = true;

                //Get Title with imdb
                if (query.StartsWith("tt")) query = await Helper.Current.GetTitleByImdbId(query, errorCallBack);

                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync("https://esubtitle.com/?s=" + query);

                var items = doc.DocumentNode.SelectNodes("//div[@class='poster_box']");
                var itemsName = doc.DocumentNode.SelectNodes("//div[@class='text']");
                if (items == null)
                {
                    MessageBox.Error(LocalizationManager.Instance.Localize("SubNotFound").ToString());
                }
                else
                {
                    var index = 0;
                    DataList?.Clear();
                    foreach (var node in items)
                    {
                        var src = node?.SelectSingleNode(".//a//noscript")?.SelectSingleNode("img")
                            ?.Attributes["srcset"]?.Value;
                        src = FixImageSrc(src.Substring(src.LastIndexOf(",") + 1));
                        var item = new AvatarModel2
                        {
                            AvatarUri = src,
                            DisplayName = FixName(itemsName[index].SelectSingleNode(".//a").InnerText.Trim()),
                            SubtitlePage = node.SelectSingleNode(".//a")?.Attributes["href"]?.Value
                        };

                        DataList.Add(item);
                        index += 1;
                    }
                }

                IsBusy = false;
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            catch (ArgumentNullException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (WebException ex)
            {
                Growl.ErrorGlobal(
                    LocalizationManager.Instance.Localize("ServerNotFound") + "\n" + ex.Message);
            }
            catch (HttpRequestException hx)
            {
                Growl.ErrorGlobal(
                    LocalizationManager.Instance.Localize("ServerNotFound") + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void errorCallBack(string e)
        {
            Growl.ErrorGlobal(e);
        }

        // Remove some persian text from movie/series name
        private string FixName(string name)
        {
            return Regex.Replace(name, "\\b" + string.Join("\\b|\\b", wordsToRemove) + "\\b", " ");
        }

        // select image url
        private string FixImageSrc(string src)
        {
            var regex = new Regex(@"(?<Protocol>\w+):\/\/(?<Domain>[\w@][\w.:@]+)\/?[\w\.?=%&=\-@/$,]*");
            var m = regex.Match(src);
            if (m.Success) return m.Value.Trim();

            return null;
        }

        #region Property

        private ObservableCollection<AvatarModel2> _dataList = new();

        public ObservableCollection<AvatarModel2> DataList
        {
            get => _dataList;
            set => SetProperty(ref _dataList, value);
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion

        #region Command
        public DelegateCommand<string> GoToLinkCommand { get; }

        #endregion
    }
}
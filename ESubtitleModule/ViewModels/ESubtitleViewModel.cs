using ESubtitleModule.Model;
using HandyControl.Controls;
using HandyControl.Data;
using HtmlAgilityPack;
using Module.Core;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace ESubtitleModule.ViewModels
{
    public class ESubtitleViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        #region Property
        private string _ModuleName = LocalizationManager.Instance.Localize("ESubtitle").ToString();
        public string ModuleName
        {
            get => LocalizationManager.Instance.Localize("ESubtitle").ToString();
            set => SetProperty(ref _ModuleName, value);
        }
        private ObservableCollection<AvatarModel> _dataList = new ObservableCollection<AvatarModel>();
        public ObservableCollection<AvatarModel> DataList
        {
            get => _dataList;
            set => SetProperty(ref _dataList, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        #endregion

        #region Command
        public DelegateCommand<FunctionEventArgs<string>> OnSearchStartedCommand { get; private set; }
        public DelegateCommand<string> GoToLinkCommand { get; private set; }
        #endregion

        public ESubtitleViewModel(IRegionManager regionManager)
        {
            DataList.Clear();
            GoToLinkCommand = new DelegateCommand<string>(GotoLink);
            _regionManager = regionManager;
            OnSearchStartedCommand = new DelegateCommand<FunctionEventArgs<string>>(OnSearchStarted);
        }

        private void GotoLink(string name)
        {
            NavigationParameters parameters = new NavigationParameters
                    { { "name_key", name } };
            _regionManager.RequestNavigate("ContentRegion", "ESubtitleDownload", parameters);
        }

        private async void OnSearchStarted(FunctionEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return;
            }

            try
            {
                IsBusy = true;

                //Get Title with imdb
                if (SearchText.StartsWith("tt"))
                {
                    SearchText = await ModuleHelper.GetTitleByImdbId(SearchText, errorCallBack);
                }

                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync("https://esubtitle.com/?s=" + SearchText);

                HtmlNodeCollection items = doc.DocumentNode.SelectNodes("//div[@class='poster_box']");
                HtmlNodeCollection itemsName = doc.DocumentNode.SelectNodes("//div[@class='text']");
                if (items == null)
                {
                    MessageBox.Error(LocalizationManager.Instance.Localize("SubNotFound").ToString());
                }
                else
                {
                    int index = 0;
                    DataList?.Clear();
                    foreach (HtmlNode node in items)
                    {
                        string src = node.SelectSingleNode(".//a/img")?.Attributes["srcset"].Value.Trim().Split(",").ToList().LastOrDefault();
                        AvatarModel item = new AvatarModel { AvatarUri = FixImageSrc(src), DisplayName = FixName(itemsName[index].SelectSingleNode(".//a").InnerText.Trim()), SubtitlePage = node.SelectSingleNode(".//a")?.Attributes["href"]?.Value };

                        DataList.Add(item);
                        index += 1;
                    }
                }
                IsBusy = false;
            }
            catch (ArgumentOutOfRangeException) { }
            catch (ArgumentNullException) { }
            catch (System.NullReferenceException) { }
            catch (System.Net.WebException ex)
            {
                Growl.Error(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + ex.Message);
            }
            catch (System.Net.Http.HttpRequestException hx)
            {
                Growl.Error(LocalizationManager.Instance.Localize("ServerNotFound").ToString() + "\n" + hx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void errorCallBack(string e)
        {
            Growl.Error(e);
        }

        // Remove some persian text from movie/series name
        private string FixName(string name)
        {
            return Regex.Replace(name, "\\b" + string.Join("\\b|\\b", wordsToRemove) + "\\b", " ");
        }

        // select image url
        private string FixImageSrc(string src)
        {
            Regex regex = new Regex(@"(?<Protocol>\w+):\/\/(?<Domain>[\w@][\w.:@]+)\/?[\w\.?=%&=\-@/$,]*");
            Match m = regex.Match(src);
            if (m.Success)
            {
                return m.Value;
            }

            return null;
        }

        private readonly List<string> wordsToRemove = "دانلود زیرنویس فارسی فیلم,دانلود زیرنویس فارسی سریال"
           .Split(',').ToList();
    }
}

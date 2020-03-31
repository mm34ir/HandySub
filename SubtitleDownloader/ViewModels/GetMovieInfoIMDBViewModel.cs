using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using SubtitleDownloader.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SubtitleDownloader.ViewModels
{
    public class GetMovieInfoIMDBViewModel : BindableBase
    {
        public static GetMovieInfoIMDBViewModel Instance;
        public DelegateCommand<FunctionEventArgs<string>> OnSearchStartedCommand { get; private set; }
        public DelegateCommand<string> CopyCommand { get; private set; }
        public DelegateCommand<string> SaveToPcCommand { get; private set; }

        #region Property

        private FlowDirection _MainFlowDirection;
        public FlowDirection MainFlowDirection
        {
            get => _MainFlowDirection;
            set => SetProperty(ref _MainFlowDirection, value);
        }

        private Visibility _visibility = Visibility.Hidden;
        public Visibility ContentVisibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        private string _searchText = "tt1442449";
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }


        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { SetProperty(ref _Title, value); }
        }

        private string _Year;
        public string Year
        {
            get { return _Year; }
            set { SetProperty(ref _Year, value); }
        }

        private string _Released;
        public string Released
        {
            get { return _Released; }
            set { SetProperty(ref _Released, value); }
        }

        private string _Type;
        public string Type
        {
            get { return _Type; }
            set { SetProperty(ref _Type, value); }
        }

        private string _TotalSeasons;
        public string TotalSeasons
        {
            get { return _TotalSeasons; }
            set { SetProperty(ref _TotalSeasons, value); }
        }

        private string _Language;
        public string Language
        {
            get { return _Language; }
            set { SetProperty(ref _Language, value); }
        }

        private string _Country;
        public string Country
        {
            get { return _Country; }
            set { SetProperty(ref _Country, value); }
        }

        private string _Awards;
        public string Awards
        {
            get { return _Awards; }
            set { SetProperty(ref _Awards, value); }
        }

        private string _Rated;
        public string Rated
        {
            get { return _Rated; }
            set { SetProperty(ref _Rated, value); }
        }

        private string _Metascore;
        public string Metascore
        {
            get { return _Metascore; }
            set { SetProperty(ref _Metascore, value); }
        }

        private string _Genre;
        public string Genre
        {
            get { return _Genre; }
            set { SetProperty(ref _Genre, value); }
        }

        private string _Director;
        public string Director
        {
            get { return _Director; }
            set { SetProperty(ref _Director, value); }
        }

        private string _Writer;
        public string Writer
        {
            get { return _Writer; }
            set { SetProperty(ref _Writer, value); }
        }

        private string _Actors;
        public string Actors
        {
            get { return _Actors; }
            set { SetProperty(ref _Actors, value); }
        }

        private string _Plot;
        public string Plot
        {
            get { return _Plot; }
            set { SetProperty(ref _Plot, value); }
        }

        private string _Poster;
        public string Poster
        {
            get { return _Poster; }
            set { SetProperty(ref _Poster, value); }
        }

        private string _RatingSource;
        public string RatingSource
        {
            get { return _RatingSource; }
            set { SetProperty(ref _RatingSource, value); }
        }

        private string _RatingValue;
        public string RatingValue
        {
            get { return _RatingValue; }
            set { SetProperty(ref _RatingValue, value); }
        }

        private string _ImdbId;
        public string ImdbId
        {
            get { return _ImdbId; }
            set { SetProperty(ref _ImdbId, value); }
        }

        private string _ImdbRating;
        public string ImdbRating
        {
            get { return _ImdbRating; }
            set { SetProperty(ref _ImdbRating, value); }
        }

        private string _ImdbVotes;
        public string ImdbVotes
        {
            get { return _ImdbVotes; }
            set { SetProperty(ref _ImdbVotes, value); }
        }

        #endregion

        public GetMovieInfoIMDBViewModel()
        {
            Instance = this;
            OnSearchStartedCommand = new DelegateCommand<FunctionEventArgs<string>>(OnSearchStarted);
            CopyCommand = new DelegateCommand<string>(OnCopyToClipboard);
            SaveToPcCommand = new DelegateCommand<string>(OnSaveToPc);
            SetFlowDirection();
        }

        private void OnSaveToPc(string source)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Png file (*.png)|*.png|Jpg file (*.jpg)|*.jpg";
            dialog.FileName = Path.GetFileName(source);
            if (dialog.ShowDialog() == true)
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(source, dialog.FileName);
                }
            }
        }

        private void OnCopyToClipboard(string data)
        {
            Clipboard.SetText(data);
        }

        public FlowDirection SetFlowDirection()
        {
           return MainFlowDirection = GlobalData.Config.UILang.Equals("fa-IR") ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        private async void OnSearchStarted(FunctionEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return;
            }

            ContentVisibility = Visibility.Visible;
            IsBusy = true;
            string url = string.Empty;
            if (SearchText.StartsWith("tt"))
            {
                url = $"http://www.omdbapi.com/?i={SearchText}&apikey=2a59a17e";
            }
            else
            {
                url = $"http://www.omdbapi.com/?t={SearchText}&apikey=2a59a17e";
            }

            try
            {
                using var client = new HttpClient();
                string responseBody = await client.GetStringAsync(url);
                var parse = System.Text.Json.JsonSerializer.Deserialize<IMDBModel.Root>(responseBody);

                if (parse.Response.Equals("True"))
                {
                    ImdbId = parse.imdbID;
                    ImdbRating = parse.imdbRating;
                    ImdbVotes = parse.imdbVotes;
                    Title = parse.Title;
                    Year = parse.Year;
                    Released = parse.Released;
                    Type = parse.Type;
                    TotalSeasons = parse.totalSeasons;
                    Language = parse.Language;
                    Country = parse.Country;
                    Awards = parse.Awards;
                    Rated = parse.Rated;
                    Metascore = parse.Metascore;
                    Genre = parse.Genre;
                    Director = parse.Director;
                    Writer = parse.Writer;
                    Actors = parse.Actors;
                    Plot = parse.Plot;
                    Poster = parse.Poster;

                    if (parse.Ratings != null)
                    {
                        StringBuilder rSource = new StringBuilder();
                        foreach (var itemSource in parse.Ratings.ToList())
                        {
                            rSource.Append(itemSource.Source).Append("|");
                        }
                        RatingSource = rSource.ToString();

                        StringBuilder rValue = new StringBuilder();
                        foreach (var itemValue in parse.Ratings.ToList())
                        {
                            rValue.Append(itemValue.Value).Append(" | ");
                        }
                        RatingValue = rValue.ToString();
                    }
                    IsBusy = false;
                }
                else
                {
                    ContentVisibility = Visibility.Hidden;

                    Growl.Error(parse.Error);
                }
               
            }
            catch (HttpRequestException ex)
            {
                Growl.Error(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}

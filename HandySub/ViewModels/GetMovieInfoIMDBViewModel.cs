using HandyControl.Controls;
using HandyControl.Data;
using Prism.Commands;
using Prism.Mvvm;
using HandySub.Model;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace HandySub.ViewModels
{
    public class GetMovieInfoIMDBViewModel : BindableBase
    {
        public DelegateCommand<FunctionEventArgs<string>> OnSearchStartedCommand { get; private set; }
        public DelegateCommand<string> CopyCommand { get; private set; }
        public DelegateCommand<string> SaveToPcCommand { get; private set; }

        #region Property

        private Visibility _visibility = Visibility.Hidden;
        public Visibility ContentVisibility
        {
            get => _visibility;
            set => SetProperty(ref _visibility, value);
        }

        private string _searchText;
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
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string _Year;
        public string Year
        {
            get => _Year;
            set => SetProperty(ref _Year, value);
        }

        private string _Released;
        public string Released
        {
            get => _Released;
            set => SetProperty(ref _Released, value);
        }

        private string _Type;
        public string Type
        {
            get => _Type;
            set => SetProperty(ref _Type, value);
        }

        private string _TotalSeasons;
        public string TotalSeasons
        {
            get => _TotalSeasons;
            set => SetProperty(ref _TotalSeasons, value);
        }

        private string _Language;
        public string Language
        {
            get => _Language;
            set => SetProperty(ref _Language, value);
        }

        private string _Country;
        public string Country
        {
            get => _Country;
            set => SetProperty(ref _Country, value);
        }

        private string _Awards;
        public string Awards
        {
            get => _Awards;
            set => SetProperty(ref _Awards, value);
        }

        private string _Rated;
        public string Rated
        {
            get => _Rated;
            set => SetProperty(ref _Rated, value);
        }

        private string _Metascore;
        public string Metascore
        {
            get => _Metascore;
            set => SetProperty(ref _Metascore, value);
        }

        private string _Genre;
        public string Genre
        {
            get => _Genre;
            set => SetProperty(ref _Genre, value);
        }

        private string _Director;
        public string Director
        {
            get => _Director;
            set => SetProperty(ref _Director, value);
        }

        private string _Writer;
        public string Writer
        {
            get => _Writer;
            set => SetProperty(ref _Writer, value);
        }

        private string _Actors;
        public string Actors
        {
            get => _Actors;
            set => SetProperty(ref _Actors, value);
        }

        private string _Plot;
        public string Plot
        {
            get => _Plot;
            set => SetProperty(ref _Plot, value);
        }

        private string _Poster;
        public string Poster
        {
            get => _Poster;
            set => SetProperty(ref _Poster, value);
        }

        private string _RatingSource;
        public string RatingSource
        {
            get => _RatingSource;
            set => SetProperty(ref _RatingSource, value);
        }

        private string _RatingValue;
        public string RatingValue
        {
            get => _RatingValue;
            set => SetProperty(ref _RatingValue, value);
        }

        private string _ImdbId;
        public string ImdbId
        {
            get => _ImdbId;
            set => SetProperty(ref _ImdbId, value);
        }

        private string _ImdbRating;
        public string ImdbRating
        {
            get => _ImdbRating;
            set => SetProperty(ref _ImdbRating, value);
        }

        private string _ImdbVotes;
        public string ImdbVotes
        {
            get => _ImdbVotes;
            set => SetProperty(ref _ImdbVotes, value);
        }

        #endregion

        public GetMovieInfoIMDBViewModel()
        {
            OnSearchStartedCommand = new DelegateCommand<FunctionEventArgs<string>>(OnSearchStarted);
            CopyCommand = new DelegateCommand<string>(OnCopyToClipboard);
            SaveToPcCommand = new DelegateCommand<string>(OnSaveToPc);
        }

        private void OnSaveToPc(string source)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Png file (*.png)|*.png|Jpg file (*.jpg)|*.jpg",
                FileName = Path.GetFileName(source)
            };
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
                using HttpClient client = new HttpClient();
                string responseBody = await client.GetStringAsync(url);
                IMDBModel.Root parse = System.Text.Json.JsonSerializer.Deserialize<IMDBModel.Root>(responseBody);

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
                        foreach (IMDBModel.Rating itemSource in parse.Ratings.ToList())
                        {
                            rSource.Append(itemSource.Source).Append("|");
                        }
                        RatingSource = rSource.ToString();

                        StringBuilder rValue = new StringBuilder();
                        foreach (IMDBModel.Rating itemValue in parse.Ratings.ToList())
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

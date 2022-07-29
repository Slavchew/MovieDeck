namespace MovieDeck.Web.ViewModels.BaseViewModels
{
    using System;

    public class PersonMovieViewModel
    {
        public int MovieId { get; set; }

        public string MovieTitle { get; set; }

        public DateTime MovieReleaseDate { get; set; }

        public string MoviePosterPath { get; set; }

        public string MoviePosterUrl { get; set; }
    }
}

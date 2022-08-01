namespace MovieDeck.Web.ViewModels.Movies
{
    using System;
using System.Collections.Generic;
    using System.Linq;

    using AutoMapper;

    using MovieDeck.Common;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Mapping;
    using MovieDeck.Web.ViewModels.Genres;

    public class MovieViewModel : IMapFrom<Movie>, IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Plot { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public TimeSpan Runtime { get; set; }

        public double AverageRating { get; set; }

        public string PosterUrl { get; set; }

        public string BackdropUrl { get; set; }

        public List<GenreViewModel> Genres { get; set; }

        public int GenresCount => Genres.Count;

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Movie, MovieViewModel>()
                .ForMember(x => x.PosterUrl, opt =>
                    opt.MapFrom(x => x.PosterPath.Contains("-") ?
                        $"/images/{GlobalConstants.MoviesImagesFolder}/" + x.PosterPath :
                        string.Format(GlobalConstants.RemoteImagesUrl, x.PosterPath)))
                .ForMember(x => x.BackdropUrl, opt =>
                    opt.MapFrom(x => x.BackdropPath.Contains("-") ?
                        $"/images/{GlobalConstants.MoviesImagesFolder}/" + x.BackdropPath :
                        string.Format(GlobalConstants.RemoteImagesUrl, x.BackdropPath)))
                .ForMember(x => x.AverageRating, opt =>
                    opt.MapFrom(x => (x.RatingsCount + x.Ratings.Count) == 0
                        ? 0
                        : ((x.ImdbRating * x.RatingsCount)
                            + x.Ratings.Sum(x => x.Value)) / (x.RatingsCount + x.Ratings.Count)));
        }
    }
}

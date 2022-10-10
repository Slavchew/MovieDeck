using AutoMapper;
using MovieDeck.Common;
using MovieDeck.Data.Models;
using MovieDeck.Services.Mapping;
using MovieDeck.Web.ViewModels.Actors;
using MovieDeck.Web.ViewModels.Directors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MovieDeck.Web.ViewModels.Movies
{
    public class RelatedMovieViewModel : IMapFrom<Movie>, IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Plot { get; set; }

        public string PlotShort => this.Plot.Substring(0, 80);

        public DateTime? ReleaseDate { get; set; }

        public TimeSpan Runtime { get; set; }

        public double AverageRating { get; set; }

        public string PosterUrl { get; set; }

        public IEnumerable<DirectorViewModel> Directors { get; set; }

        public IEnumerable<ActorViewModel> Actors { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Movie, RelatedMovieViewModel>()
                .ForMember(x => x.PosterUrl, opt =>
                    opt.MapFrom(x => x.PosterPath == null ?
                        GlobalConstants.BlankMoviePoster :
                            x.PosterPath.Contains("-") ?
                            $"/images/{GlobalConstants.MoviesImagesFolder}/" + x.PosterPath :
                            string.Format(GlobalConstants.RemoteImagesUrl, x.PosterPath)))
                .ForMember(x => x.AverageRating, opt =>
                    opt.MapFrom(x => (x.RatingsCount + x.Ratings.Count) == 0
                        ? 0
                        : ((x.ImdbRating * x.RatingsCount)
                            + x.Ratings.Sum(x => x.Value)) / (x.RatingsCount + x.Ratings.Count)));
        }
    }
}

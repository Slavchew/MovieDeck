namespace MovieDeck.Web.ViewModels.Movies
{
    using System.Linq;

    using AutoMapper;

    using MovieDeck.Common;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Mapping;

    public class MovieInListViewModel : IMapFrom<Movie>, IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string PosterUrl { get; set; }

        public double AverageRating { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Movie, MovieInListViewModel>()
                .ForMember(x => x.PosterUrl, opt =>
                    opt.MapFrom(x => x.PosterPath.Contains("-") ?
                        $"/images/{GlobalConstants.MoviesImagesFolder}/" + x.PosterPath :
                        string.Format(GlobalConstants.RemoteImagesUrl, x.PosterPath)))
                .ForMember(x => x.AverageRating, opt =>
                    opt.MapFrom(x => x.RatingsCount + x.Ratings.Count == 0
                        ? 0
                        : ((x.ImdbRating * x.RatingsCount)
                            + x.Ratings.Sum(x => x.Value)) / (x.RatingsCount + x.Ratings.Count)));
        }
    }
}
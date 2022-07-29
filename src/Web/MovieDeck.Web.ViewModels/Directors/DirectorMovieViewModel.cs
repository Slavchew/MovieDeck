namespace MovieDeck.Web.ViewModels.Actors
{
    using AutoMapper;

    using MovieDeck.Common;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Mapping;
    using MovieDeck.Web.ViewModels.BaseViewModels;

    public class DirectorMovieViewModel : PersonMovieViewModel, IMapFrom<MovieDirector>, IHaveCustomMappings
    {
        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<MovieDirector, DirectorMovieViewModel>()
                .ForMember(x => x.MoviePosterUrl, opt =>
                    opt.MapFrom(x => x.Movie.PosterPath.Contains("-") ?
                        $"/images/{GlobalConstants.MoviesImagesFolder}/" + x.Movie.PosterPath :
                        string.Format(GlobalConstants.RemoteImagesUrl, x.Movie.PosterPath)));
        }
    }
}
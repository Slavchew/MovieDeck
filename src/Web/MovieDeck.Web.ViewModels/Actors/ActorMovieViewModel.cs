namespace MovieDeck.Web.ViewModels.Actors
{
    using AutoMapper;

    using MovieDeck.Common;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Mapping;
    using MovieDeck.Web.ViewModels.BaseViewModels;

    public class ActorMovieViewModel : PersonMovieViewModel, IMapFrom<MovieActor>, IHaveCustomMappings
    {
        public string CharacterName { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<MovieActor, ActorMovieViewModel>()
                .ForMember(x => x.MoviePosterUrl, opt =>
                    opt.MapFrom(x => x.Movie.PosterPath == null ?
                        GlobalConstants.BlankMoviePoster :
                            x.Movie.PosterPath.Contains("-") ?
                            $"/images/{GlobalConstants.MoviesImagesFolder}/" + x.Movie.PosterPath :
                            string.Format(GlobalConstants.RemoteImagesUrl, x.Movie.PosterPath)));
        }
    }
}

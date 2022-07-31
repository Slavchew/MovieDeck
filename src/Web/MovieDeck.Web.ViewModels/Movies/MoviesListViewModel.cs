namespace MovieDeck.Web.ViewModels.Movies
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using Microsoft.AspNetCore.Mvc.Rendering;

    public class MoviesListViewModel : PagingViewModel
    {
        public IEnumerable<MovieInListViewModel> Movies { get; set; }

        public SearchMovieInputModel SearchModel { get; set; }
    }
}

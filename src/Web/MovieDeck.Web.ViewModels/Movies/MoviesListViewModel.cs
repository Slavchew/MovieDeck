namespace MovieDeck.Web.ViewModels.Movies
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Mvc.Rendering;

    public class MoviesListViewModel : PagingViewModel
    {
        public IEnumerable<MovieInListViewModel> Movies { get; set; }

        public SearchMovieInputModel SearchModel { get; set; }

        public string Order { get; set; }

        public IEnumerable<SelectListItem> OrderOptions { get; set; }
    }
}

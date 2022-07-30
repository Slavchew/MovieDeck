namespace MovieDeck.Web.ViewModels.Movies
{
    using System.Collections.Generic;

    public class MoviesListViewModel : PagingViewModel
    {
        public IEnumerable<MovieInListViewModel> Movies { get; set; }
    }
}

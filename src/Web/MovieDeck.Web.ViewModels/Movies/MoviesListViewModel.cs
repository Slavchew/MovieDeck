namespace MovieDeck.Web.ViewModels.Movies
{
    using System.Collections.Generic;

    public class MoviesListViewModel
    {
        public IEnumerable<MoviesInListViewModel> Movies { get; set; }
    }
}

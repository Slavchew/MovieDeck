namespace MovieDeck.Services.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Rendering;

    using MovieDeck.Data.Models;
    using MovieDeck.Web.ViewModels;
    using MovieDeck.Web.ViewModels.Movies;

    public interface IMoviesService
    {
        Task CreateAsync(CreateMovieInputModel input, string userId, string imagePath);

        Task UpdateAsync(int id, EditMovieInputModel input);

        Task DeleteAsync(int id);

        IEnumerable<T> GetAllForHomePage<T>();

        Task<IEnumerable<T>> GetPopularMoviesAsync<T>(int page = 0);

        Task<IEnumerable<T>> GetUpcomingMoviesAsync<T>();

        Task<T> GetMovieByIdAsync<T>(int id);

        List<MovieVideoViewModel> GetMovieVideosForSingleMoviePage(int id);

        SearchMovieInputModel PopulateSearchInputModelWithGenres(SearchMovieInputModel viewModel);

        T PopulateMovieInputModelDropdownCollections<T>(T viewModel)
            where T : MovieInputModelDropdownItems;

        IEnumerable<SelectListItem> GetOrderOptions();

        IEnumerable<T> GetMoviesBySearch<T>(int id, int itemsPerPage, SearchMovieInputModel searchModel, string order, out int movieCount);

        Task<List<int>> GetActorsOrignalOrderIdsAsync(int id);
    }
}

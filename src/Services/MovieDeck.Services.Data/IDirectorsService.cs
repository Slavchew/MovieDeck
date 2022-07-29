namespace MovieDeck.Services.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using MovieDeck.Data.Models;
    using MovieDeck.Web.ViewModels.Directors;
    using MovieDeck.Web.ViewModels.Images;

    public interface IDirectorsService : IGetAllAsKeyValuePairs
    {
        T GetById<T>(int id);

        Director GetDirectorEntityById(int id);

        Task CreateAsync(CreateDirectorInputModel input, string userId, string imagePath);

        Task RemoveAllMovieDirectorsForMovieAsync(int id);

        List<DirectorImageViewModel> GetDirectorPhotosForSingleDirectorPage(int originalId);
    }
}

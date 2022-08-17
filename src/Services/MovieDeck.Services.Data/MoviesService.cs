namespace MovieDeck.Services.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    using MovieDeck.Common;
    using MovieDeck.Data.Common.Repositories;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Mapping;
    using MovieDeck.Services.TmdbApi;
    using MovieDeck.Web.ViewModels;
    using MovieDeck.Web.ViewModels.Movies;
    using Newtonsoft.Json.Linq;

    public class MoviesService : IMoviesService
    {
        private readonly IDeletableEntityRepository<Movie> moviesRepository;
        private readonly IActorsService actorsService;
        private readonly IGenresService genresService;
        private readonly IDirectorsService directorsService;
        private readonly ICompaniesService companiesService;
        private readonly IRatingsService ratingsService;
        private readonly IImagesService imagesService;
        private readonly ITmdbService tmdbService;
        private readonly ApplicationUser user;

        public MoviesService(
            IDeletableEntityRepository<Movie> moviesRepository,
            IActorsService actorsService,
            IGenresService genresService,
            IDirectorsService directorsService,
            ICompaniesService companiesService,
            IRatingsService ratingsService,
            IImagesService imagesService,
            ITmdbService tmdbService)
        {
            this.moviesRepository = moviesRepository;
            this.actorsService = actorsService;
            this.genresService = genresService;
            this.directorsService = directorsService;
            this.companiesService = companiesService;
            this.ratingsService = ratingsService;
            this.imagesService = imagesService;
            this.tmdbService = tmdbService;
        }

        public async Task CreateAsync(CreateMovieInputModel input, string userId, string imagePath)
        {
            var movie = new Movie
            {
                Title = input.Title,
                Plot = input.Plot,
                ReleaseDate = input.ReleaseDate,
                Runtime = input.Runtime,
                AddedByUserId = userId,
            };

            foreach (var genreId in input.GenresIds)
            {
                var genre = this.genresService.GetById(genreId);
                if (genre == null)
                {
                    continue;
                }

                movie.Genres.Add(new MovieGenre
                {
                    Genre = genre,
                });
            }

            Directory.CreateDirectory($"{imagePath}/{GlobalConstants.MoviesImagesFolder}/");

            // Create and Save Poster Image
            Image poster = this.imagesService.CreateImage(input.Poster, userId);
            await this.imagesService
                .SaveImageToWebRootAsync(imagePath, poster, input.Poster, GlobalConstants.MoviesImagesFolder);

            movie.PosterPath = $"/{poster.Id}.{poster.Extension}";

            // Create and Save Backdrop Image
            Image backdrop = this.imagesService.CreateImage(input.Backdrop, userId);
            await this.imagesService
                .SaveImageToWebRootAsync(imagePath, backdrop, input.Backdrop, GlobalConstants.MoviesImagesFolder);

            movie.BackdropPath = $"/{backdrop.Id}.{backdrop.Extension}";

            foreach (var image in input.Images)
            {
                Image dbImage = this.imagesService.CreateImage(image, userId);
                movie.Images.Add(dbImage);
                await this.imagesService
                    .SaveImageToWebRootAsync(imagePath, dbImage, image, GlobalConstants.MoviesImagesFolder);
            }

            foreach (var actorInput in input.Actors)
            {
                var actor = this.actorsService.GetActorEntityById(actorInput.ActorId);
                if (actor == null)
                {
                    continue;
                }

                if (movie.Actors.Any(x => x.Actor == actor))
                {
                    throw new Exception($"{actor.FullName} is already set as an actor");
                }

                movie.Actors.Add(new MovieActor
                {
                    Actor = actor,
                    CharacterName = actorInput.CharacterName,
                });
            }

            foreach (var directorId in input.DirectorsIds)
            {
                var director = this.directorsService.GetDirectorEntityById(directorId);
                if (director == null)
                {
                    continue;
                }

                movie.Directors.Add(new MovieDirector
                {
                    Director = director,
                });
            }

            foreach (var companyId in input.CompaniesIds)
            {
                var company = this.companiesService.GetById(companyId);
                if (company == null)
                {
                    continue;
                }

                movie.Companies.Add(new MovieCompany
                {
                    Company = company,
                });
            }

            await this.moviesRepository.AddAsync(movie);
            await this.moviesRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, EditMovieInputModel input)
        {
            var movie = await this.moviesRepository.All().FirstOrDefaultAsync(x => x.Id == id);
            movie.Title = input.Title;
            movie.Plot = input.Plot;
            movie.ReleaseDate = input.ReleaseDate;
            movie.Runtime = input.Runtime;

            await this.genresService.RemoveAllMovieGenresForMovieAsync(id);
            movie.Genres.Clear();

            if (input.GenresIds != null)
            {
                foreach (var genreId in input.GenresIds)
                {
                    var genre = this.genresService.GetById(genreId);
                    if (genre == null)
                    {
                        continue;
                    }

                    movie.Genres.Add(new MovieGenre
                    {
                        Genre = genre,
                    });
                }
            }

            await this.actorsService.RemoveAllMovieActorsForMovieAsync(id);
            movie.Actors.Clear();

            if (input.Actors != null)
            {
                foreach (var actorInput in input.Actors)
                {
                    var actor = this.actorsService.GetActorEntityById(actorInput.ActorId);
                    if (actor == null)
                    {
                        continue;
                    }

                    if (movie.Actors.Any(x => x.Actor == actor))
                    {
                        throw new Exception($"{actor.FullName} is already set as an actor");
                    }

                    movie.Actors.Add(new MovieActor
                    {
                        Actor = actor,
                        CharacterName = actorInput.CharacterName,
                    });
                }
            }


            await this.directorsService.RemoveAllMovieDirectorsForMovieAsync(id);
            movie.Directors.Clear();

            if (input.DirectorsIds != null)
            {
                foreach (var directorId in input.DirectorsIds)
                {
                    var director = this.directorsService.GetDirectorEntityById(directorId);
                    if (director == null)
                    {
                        continue;
                    }

                    movie.Directors.Add(new MovieDirector
                    {
                        Director = director,
                    });
                }
            }

            await this.companiesService.RemoveAllMovieCompaniesForMovieAsync(id);
            movie.Companies.Clear();

            if (input.CompaniesIds != null)
            {
                foreach (var companyId in input.CompaniesIds)
                {
                    var company = this.companiesService.GetById(companyId);
                    if (company == null)
                    {
                        continue;
                    }

                    movie.Companies.Add(new MovieCompany
                    {
                        Company = company,
                    });
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            var movie = await this.moviesRepository.All().FirstOrDefaultAsync(x => x.Id == id);
            this.moviesRepository.Delete(movie);
            await this.moviesRepository.SaveChangesAsync();
        }

        public IEnumerable<T> GetAllForHomePage<T>()
        {
            return this.moviesRepository.AllAsNoTracking()
                    .To<T>()
                    .OrderBy(x => Guid.NewGuid())
                    .Take(20)
                    .ToList();
        }

        public async Task<T> GetMovieByIdAsync<T>(int id)
        {
            return await this.moviesRepository.AllAsNoTracking()
                .Where(x => x.Id == id)
                .To<T>().FirstOrDefaultAsync();

            /* without AutoMapper
             * 
            .Select(x => new SingleMovieViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Plot = x.Plot,
                ReleaseDate = x.ReleaseDate,
                Runtime = x.Runtime,
                PosterUrl =
                    x.PosterPath.Contains("-") ?
                    "/images/movies/" + x.PosterPath :
                    this.tmdbService.GenereateImageUrl(x.PosterPath),
                AverageRating = this.ratingsService.GetAverageRatings(x.Id),
                RatingsCount = this.ratingsService.GetRatingsCount(x.Id),
                UserRating = (byte)(userId == null ? 0 : this.ratingsService.GetUserRating(x.Id, userId)),
                Genres = x.Genres.Select(g => new GenreViewModel
                {
                    Name = g.Genre.Name,
                }),
                Directors = x.Directors.Select(d => new DirectorViewModel
                {
                    Name = d.Director.FullName,
                    PhotoUrl = this.tmdbService.GenereateImageUrl(d.Director.PhotoPath),
                    PhotoPath = d.Director.PhotoPath,
                }),
                Actors = x.Actors.Select(a => new ActorViewModel
                {
                    FullName = a.Actor.FullName,
                    CharacterName = a.CharacterName,
                    PhotoUrl = this.tmdbService.GenereateImageUrl(a.Actor.PhotoPath),
                    PhotoPath = a.Actor.PhotoPath,
                }),
                Images = x.Images.Select(i => new ImageViewModel
                {
                    PhotoUrl =
                        i.RemoteImageUrl != null ?
                        this.tmdbService.GenereateImageUrl(i.RemoteImageUrl) :
                        $"/images/movies/{i.Id}.{i.Extension}",
                }),
            */
        }

        public async Task<List<int>> GetActorsOrignalOrderIdsAsync(int id)
        {
            return await this.tmdbService.GetMovieActorsInOrderAsync(id);
        }

        public IEnumerable<T> GetMoviesBySearch<T>(int page, int itemsPerPage, SearchMovieInputModel searchModel, string order, out int moviesCount)
        {
            var query = this.moviesRepository.All().AsQueryable();

            if (searchModel?.Search != null)
            {
                query = query.Where(x => x.Title.Contains(searchModel.Search));
            }

            if (searchModel?.GenresIds != null)
            {
                foreach (var genreId in searchModel.GenresIds)
                {
                    query = query.Where(x => x.Genres.Any(i => i.GenreId == genreId));
                }
            }

            if (searchModel?.FromYear != null)
            {
                query = query.Where(x => x.ReleaseDate.Value.Year >= searchModel.FromYear);
            }

            if (searchModel?.ToYear != null)
            {
                query = query.Where(x => x.ReleaseDate.Value.Year <= searchModel.ToYear);
            }

            moviesCount = query.Count();

            if (order != null)
            {
                if (order == "popularityDesc")
                {
                    query = query.OrderByDescending(x => x.RatingsCount);
                }
                else if (order == "popularityAsc")
                {
                    query = query.OrderBy(x => x.RatingsCount);
                }
                else if (order == "ratingDesc")
                {
                    query = query.OrderByDescending(x => (x.RatingsCount + x.Ratings.Count) == 0
                        ? 0
                        : ((x.ImdbRating * x.RatingsCount)
                            + x.Ratings.Sum(x => x.Value)) / (x.RatingsCount + x.Ratings.Count));
                }
                else if (order == "ratingAsc")
                {
                    query = query.OrderBy(x => (x.RatingsCount + x.Ratings.Count) == 0
                        ? 0
                        : ((x.ImdbRating * x.RatingsCount)
                            + x.Ratings.Sum(x => x.Value)) / (x.RatingsCount + x.Ratings.Count));
                }
                else if (order == "dateDesc")
                {
                    query = query.OrderByDescending(x => x.ReleaseDate);
                }
                else if (order == "dateAsc")
                {
                    query = query.OrderBy(x => x.ReleaseDate);
                }
            }

            var movies = query
                .Skip((page - 1) * itemsPerPage).Take(itemsPerPage)
                .To<T>()
                .ToList();

            return movies;
        }

        public async Task<IEnumerable<T>> GetPopularMoviesAsync<T>(int page = 0)
        {
            var originalIds = await this.tmdbService.GetPopularMoviesOriginalIdAsync(page);

            var concurrentBag = new ConcurrentBag<int>(originalIds);

            await this.ImportMoviesIfNotExistAsync(concurrentBag);

            var popularMovies = new List<T>();
            foreach (var originalId in originalIds)
            {
                if (!this.moviesRepository.AllAsNoTracking().Any(x => x.OriginalId == originalId))
                {
                    continue;
                }

                var movie = await this.moviesRepository.AllAsNoTracking()
                    .Where(x => x.OriginalId == originalId)
                    .To<T>()
                    .FirstOrDefaultAsync();

                popularMovies.Add(movie);
            }

            return popularMovies;
        }

        public async Task<IEnumerable<T>> GetUpcomingMoviesAsync<T>()
        {
            var originalIds = await this.tmdbService.GetUpcomingMoviesOriginalIdAsync();

            var concurrentBag = new ConcurrentBag<int>(originalIds);

            await this.ImportMoviesIfNotExistAsync(concurrentBag);

            var upcomingMovies = new List<T>();
            foreach (var originalId in originalIds)
            {
                if (!this.moviesRepository.AllAsNoTracking().Any(x => x.OriginalId == originalId))
                {
                    continue;
                }

                var movie = await this.moviesRepository.AllAsNoTracking()
                    .Where(x => x.OriginalId == originalId)
                    .To<T>()
                    .FirstOrDefaultAsync();

                upcomingMovies.Add(movie);
            }

            return upcomingMovies;
        }

        public List<MovieVideoViewModel> GetMovieVideosForSingleMoviePage(int id)
        {
            if (id == 0)
            {
                return new List<MovieVideoViewModel>();
            }

            var movieVideoDtos = this.tmdbService.GetMovieVideos(id);

            var movieVideos = new List<MovieVideoViewModel>();

            foreach (var videoDto in movieVideoDtos)
            {
                var movieVideo = new MovieVideoViewModel
                {
                    Url = string.Format(GlobalConstants.YoutubeEmbedVideoUrl, videoDto.Key),
                    ThumbnailUrl = string.Format(GlobalConstants.YoutubeVideoThumbnailUrl, videoDto.Key),
                    Name = videoDto.Name,
                };

                movieVideos.Add(movieVideo);
            }

            return movieVideos;
        }

        public IEnumerable<SelectListItem> GetOrderOptions()
        {
            var options = new List<SelectListItem>()
            {
                new SelectListItem("ById (Default)", "byId"),
                new SelectListItem("Popularity Descending", "popularityDesc"),
                new SelectListItem("Popularity Ascending", "popularityAsc"),
                new SelectListItem("Rating Descending", "ratingDesc"),
                new SelectListItem("Rating Ascending", "ratingAsc"),
                new SelectListItem("Release Date Descending", "dateDesc"),
                new SelectListItem("Release Date Ascending", "dateAsc"),
            };

            return options;
        }

        public T PopulateMovieInputModelDropdownCollections<T>(T viewModel)
            where T : MovieInputModelDropdownItems
        {
            viewModel.GenresItems = this.genresService.GetAllAsKeyValuePairs()
                .Select(x => new SelectListItem(x.Value, x.Key));
            viewModel.ActorsItems = this.actorsService.GetAllAsKeyValuePairs()
                .Select(x => new SelectListItem(x.Value, x.Key));
            viewModel.DirectorsItems = this.directorsService.GetAllAsKeyValuePairs()
                .Select(x => new SelectListItem(x.Value, x.Key));
            viewModel.CompaniesItems = this.companiesService.GetAllAsKeyValuePairs()
                .Select(x => new SelectListItem(x.Value, x.Key));

            return viewModel;
        }

        public SearchMovieInputModel PopulateSearchInputModelWithGenres(SearchMovieInputModel viewModel)
        {
            viewModel.GenresItems = this.genresService.GetAllAsKeyValuePairs()
                .Select(x => new SelectListItem(x.Value, x.Key));

            return viewModel;
        }

        private async Task ImportMoviesIfNotExistAsync(ConcurrentBag<int> originalIds)
        {
            foreach (var originalId in originalIds)
            {
                if (this.moviesRepository.AllAsNoTrackingWithDeleted().Any(x => x.OriginalId == originalId))
                {
                    continue;
                }

                var movieDto = await this.tmdbService.GetMovieById(originalId);
                if (movieDto == null)
                {
                    continue;
                }

                await this.tmdbService.ImportMovieAsync(movieDto);
            }
        }

        public bool IsInWatchList(int movieId)
        {
            var movie = this.GetMovieByIdAsync<Movie>(movieId);

            return this.user.Watchlists.Select(x => x.Movies.Any(m => m.Id == movieId)).FirstOrDefault();
        }
    }
}

namespace MovieDeck.Services.TmdbApi
{
    using System;
using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Configuration;

    using MovieDeck.Data.Common.Repositories;
    using MovieDeck.Data.Models;
    using MovieDeck.Services.Models;

    using TMDbLib.Client;
    using TMDbLib.Objects.Configuration;
    using TMDbLib.Objects.Movies;
    using TMDbLib.Objects.People;

    public class TmdbService : ITmdbService
    {
        private const string BaseUrl = "https://www.themoviedb.org";
        private readonly string tmdbApiKey;

        private readonly IDeletableEntityRepository<Data.Models.Movie> moviesRepository;
        private readonly IDeletableEntityRepository<Actor> actorsRepository;
        private readonly IDeletableEntityRepository<Director> directorsRepository;
        private readonly IDeletableEntityRepository<ProductionCompany> companiesRepository;
        private readonly IDeletableEntityRepository<Genre> genresRepository;
        private readonly IDeletableEntityRepository<Image> imagesRepository;
        private readonly IRepository<MovieActor> movieActorsRepository;
        private readonly IRepository<MovieDirector> movieDirectorsRepository;
        private readonly IRepository<MovieCompany> movieCompaniesRepository;
        private readonly IRepository<MovieGenre> movieGenresRepository;

        private readonly IConfiguration configuration;

        private TMDbClient client;
        private APIConfiguration config;

        public TmdbService(
            IDeletableEntityRepository<Data.Models.Movie> moviesRepository,
            IDeletableEntityRepository<Actor> actorsRepository,
            IDeletableEntityRepository<Director> directorsRepository,
            IDeletableEntityRepository<ProductionCompany> companiesRepository,
            IDeletableEntityRepository<Genre> genresRepository,
            IDeletableEntityRepository<Image> imagesRepository,
            IRepository<MovieActor> movieActorsRepository,
            IRepository<MovieDirector> movieDirectorsRepository,
            IRepository<MovieCompany> movieCompaniesRepository,
            IRepository<MovieGenre> movieGenresRepository,
            IConfiguration configuration)
        {
            this.moviesRepository = moviesRepository;
            this.actorsRepository = actorsRepository;
            this.directorsRepository = directorsRepository;
            this.companiesRepository = companiesRepository;
            this.genresRepository = genresRepository;
            this.imagesRepository = imagesRepository;
            this.movieActorsRepository = movieActorsRepository;
            this.movieDirectorsRepository = movieDirectorsRepository;
            this.movieCompaniesRepository = movieCompaniesRepository;
            this.movieGenresRepository = movieGenresRepository;

            this.configuration = configuration;

            this.tmdbApiKey = this.configuration.GetSection("TmdbApi")["ApiKey"];

            this.client = new TMDbClient(this.tmdbApiKey);
            this.config = this.client.GetAPIConfiguration().Result;
        }

        public async Task ImportMovieAsync(MovieDto movieDto)
        {
            if (this.moviesRepository.AllAsNoTrackingWithDeleted().Any(x => x.OriginalId == movieDto.OriginalId))
            {
                return;
            }

            var movie = new Data.Models.Movie
            {
                Title = movieDto.Title,
                Plot = movieDto.Plot,
                ReleaseDate = movieDto.ReleaseDate,
                Runtime = movieDto.Runtime,
                ImdbRating = movieDto.ImdbRating,
                RatingsCount = movieDto.RatingsCount,
                PosterPath = movieDto.PosterPath,
                BackdropPath = movieDto.BackdropPath,
                OriginalId = movieDto.OriginalId,
                TrailerKey = movieDto.TrailerKey,
            };

            await this.moviesRepository.AddAsync(movie);

            foreach (var actorDto in movieDto.Actors)
            {
                var actorId = await this.GetOrCreateActorAsync(actorDto);
                var characterName = actorDto.Character;

                var movieActor = new MovieActor
                {
                    ActorId = actorId,
                    Movie = movie,
                    CharacterName = characterName,
                };
                await this.movieActorsRepository.AddAsync(movieActor);
            }

            foreach (var directorDto in movieDto.Directors)
            {
                var directorId = await this.GetOrCreateDirectorAsync(directorDto);

                var movieDirector = new MovieDirector
                {
                    DirectorId = directorId,
                    Movie = movie,
                };
                await this.movieDirectorsRepository.AddAsync(movieDirector);
            }

            foreach (var genreName in movieDto.Genres)
            {
                var genreId = await this.GetOrCreateGenreAsync(genreName);

                var movieGenre = new MovieGenre
                {
                    GenreId = genreId,
                    Movie = movie,
                };
                await this.movieGenresRepository.AddAsync(movieGenre);
            }

            foreach (var companyName in movieDto.Companies)
            {
                var companyId = await this.GetOrCreateCompanyAsync(companyName);

                var movieCompany = new MovieCompany
                {
                    CompanyId = companyId,
                    Movie = movie,
                };
                await this.movieCompaniesRepository.AddAsync(movieCompany);
            }

            foreach (var imageUrl in movieDto.Images)
            {
                var image = new Image
                {
                    RemoteImageUrl = imageUrl,
                    Movie = movie,
                };

                await this.imagesRepository.AddAsync(image);
            }

            Console.WriteLine($"{movie.Title} added with {movie.Actors.Count} actors");
            await this.moviesRepository.SaveChangesAsync();
        }

        public async Task ImportMoviesInRangeAsync(int fromId, int toId)
        {
            var movies = this.GetMoviesInRange(fromId, toId);

            foreach (var movieDto in movies)
            {
                if (this.moviesRepository.AllAsNoTracking().Any(x => x.OriginalId == movieDto.OriginalId))
                {
                    continue;
                }

                await this.ImportMovieAsync(movieDto);
            }
        }

        public async Task<IEnumerable<int>> GetPopularMoviesOriginalIdAsync(int page)
        {
            var topPopularMovies = await this.client.GetMoviePopularListAsync(page: page);

            return topPopularMovies.Results.Select(x => x.Id);
        }

        public async Task<IEnumerable<int>> GetUpcomingMoviesOriginalIdAsync()
        {
            var upcomingMovies = await this.client.GetMovieUpcomingListAsync();

            return upcomingMovies.Results.Select(x => x.Id);
        }

        public void GetAll()
        {
            throw new System.NotImplementedException();
        }

        public async Task<MovieDto> GetMovieById(int id)
        {
            TMDbLib.Objects.Movies.Movie movieInfo = this.client
                .GetMovieAsync(id, MovieMethods.Credits | MovieMethods.Images | MovieMethods.Videos).Result;

            if (movieInfo == null)
            {
                return null;
            }

            if (movieInfo.ImdbId == null || movieInfo.Adult)
            {
                return null;
            }

            var movie = new MovieDto
            {
                Title = movieInfo.Title,
                Plot = string.IsNullOrEmpty(movieInfo.Overview) ? null : movieInfo.Overview,
                ReleaseDate = movieInfo.ReleaseDate,
                Runtime = TimeSpan.FromMinutes((double)movieInfo.Runtime),
                ImdbRating = movieInfo.VoteAverage,
                RatingsCount = movieInfo.VoteCount,
                OriginalId = movieInfo.Id,
                PosterPath = movieInfo.PosterPath,
                BackdropPath = movieInfo.BackdropPath,
                TrailerKey = movieInfo.Videos.Results.LastOrDefault(x => x.Type == "Trailer")?.Key,
                Genres = movieInfo.Genres.Select(x => x.Name).ToList(),
                Companies = movieInfo.ProductionCompanies.Select(x => x.Name).ToList(),
                Images = this.GetMovieImages(movieInfo),
                Directors = await this.GetMovieDirectorsAsync(movieInfo),
                Actors = await this.GetMovieActorsAsync(movieInfo),
            };

            Console.WriteLine($"{movie.Title} got with {movie.Actors.Count} actors");
            return movie;
        }

        public async Task<List<int>> GetMovieActorsInOrderAsync(int originalId)
        {
            var credits = await this.client.GetMovieCreditsAsync(originalId);

            return credits.Cast.Select(x => x.Id).ToList();
        }

        public List<MovieVideoDto> GetMovieVideos(int originalId)
        {
            TMDbLib.Objects.Movies.Movie movieInfo = this.client
                .GetMovieAsync(originalId, MovieMethods.Videos).Result;

            var movieVideosDtos = new List<MovieVideoDto>();

            foreach (var video in movieInfo.Videos.Results.Where(x => x.Site == "YouTube"))
            {
                var movieVideoDto = new MovieVideoDto
                {
                    Key = video.Key,
                    Name = video.Name,
                };

                movieVideosDtos.Add(movieVideoDto);
            }

            return movieVideosDtos;
        }

        public List<PersonImageDto> GetPersonImages(int originalId)
        {
            Person personInfo = this.client
                .GetPersonAsync(originalId, PersonMethods.Images).Result;

            var personImagesDtos = new List<PersonImageDto>();

            foreach (var personImage in personInfo.Images.Profiles)
            {
                var personImageDto = new PersonImageDto
                {
                    PhotoPath = personImage.FilePath,
                };

                personImagesDtos.Add(personImageDto);
            }

            return personImagesDtos;
        }

        private async Task<List<ActorDto>> GetMovieActorsAsync(TMDbLib.Objects.Movies.Movie movieInfo)
        {
            var actors = new List<ActorDto>();

            foreach (var actor in movieInfo.Credits.Cast)
            {
                var actorInfo = await this.client.GetPersonAsync(actor.Id);

                actors.Add(new ActorDto
                {
                    FullName = actorInfo.Name,
                    Biography = string.IsNullOrEmpty(actorInfo.Biography) ? null : actorInfo.Biography,
                    BirthDate = actorInfo.Birthday,
                    PhotoPath = actorInfo.ProfilePath,
                    OriginalId = actorInfo.Id,
                    Character = string.IsNullOrEmpty(actor.Character) ? null : actor.Character,
                });
            }

            return actors;
        }

        private async Task<List<PersonDto>> GetMovieDirectorsAsync(TMDbLib.Objects.Movies.Movie movieInfo)
{
            var directors = new List<PersonDto>();

            foreach (var director in movieInfo.Credits.Crew.Where(x => x.Job == "Director"))
            {
                var directorInfo = await this.client.GetPersonAsync(director.Id);

                directors.Add(new PersonDto
                {
                    FullName = directorInfo.Name,
                    Biography = string.IsNullOrEmpty(directorInfo.Biography) ? null : directorInfo.Biography,
                    BirthDate = directorInfo.Birthday,
                    PhotoPath = directorInfo.ProfilePath,
                    OriginalId = directorInfo.Id,
                });
            }

            return directors;
        }

        private List<string> GetMovieImages(TMDbLib.Objects.Movies.Movie movieInfo)
        {
            return movieInfo.Images.Backdrops.Select(x => x.FilePath).ToList();
        }

        private ConcurrentBag<MovieDto> GetMoviesInRange(int fromId, int toId)
        {
            var movies = new ConcurrentBag<MovieDto>();
            Parallel.For(fromId, toId + 1, i =>
            {
                try
                {
                    var movie = this.GetMovieById(i).GetAwaiter().GetResult();
                    if (movie != null)
                    {
                        movies.Add(movie);
                    }
                }
                catch
                {
                }
            });

            return movies;
        }

        private async Task<int> GetOrCreateActorAsync(ActorDto actorDto)
        {
            var actor = this.actorsRepository
                .AllAsNoTracking()
                .FirstOrDefault(x => x.OriginalId == actorDto.OriginalId);

            if (actor != null)
            {
                return actor.Id;
            }

            actor = new Actor
            {
                FullName = actorDto.FullName,
                Biography = actorDto.Biography,
                BirthDate = actorDto.BirthDate,
                PhotoPath = actorDto.PhotoPath,
                OriginalId = actorDto.OriginalId,
            };

            await this.actorsRepository.AddAsync(actor);
            await this.actorsRepository.SaveChangesAsync();

            return actor.Id;
        }

        private async Task<int> GetOrCreateDirectorAsync(PersonDto directorDto)
        {
            var director = this.directorsRepository
                .AllAsNoTracking()
                .FirstOrDefault(x => x.OriginalId == directorDto.OriginalId);

            if (director != null)
            {
                return director.Id;
            }

            director = new Director
            {
                FullName = directorDto.FullName,
                Biography = directorDto.Biography,
                BirthDate = directorDto.BirthDate,
                PhotoPath = directorDto.PhotoPath,
                OriginalId = directorDto.OriginalId,
            };

            await this.directorsRepository.AddAsync(director);
            await this.directorsRepository.SaveChangesAsync();

            return director.Id;
        }

        private async Task<int> GetOrCreateGenreAsync(string name)
        {
            var genre = this.genresRepository
                .AllAsNoTracking()
                .FirstOrDefault(x => x.Name == name);

            if (genre != null)
            {
                return genre.Id;
            }

            genre = new Genre
            {
                Name = name,
            };

            await this.genresRepository.AddAsync(genre);
            await this.genresRepository.SaveChangesAsync();

            return genre.Id;
        }

        private async Task<int> GetOrCreateCompanyAsync(string name)
        {
            var company = this.companiesRepository
                .AllAsNoTracking()
                .FirstOrDefault(x => x.Name == name);

            if (company != null)
            {
                return company.Id;
            }

            company = new ProductionCompany
            {
                Name = name,
            };

            await this.companiesRepository.AddAsync(company);
            await this.companiesRepository.SaveChangesAsync();

            return company.Id;
        }
    }
}

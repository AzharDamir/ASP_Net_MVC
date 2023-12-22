using Microsoft.Extensions.Caching.Memory;
using MovieStore.Models.Domain;
using MovieStore.Models.DTO;
using MovieStore.Repositories.Abstract;

namespace MovieStore.Repositories.Implementation
{
    public class MovieService : IMovieService
    {
        private readonly DBContext ctx;
        private readonly IMemoryCache _cache;
        private readonly ILogger<IMovieService> _logger;
        private  List<string> _cacheParameters; // Store caching parameters here
        public MovieService(DBContext ctx, IMemoryCache cache, ILogger<IMovieService> logger)
        {
            this.ctx = ctx;
            _cache = cache;
            _logger = logger;
        }
        public bool Add(Movie model)
        {
            try
            {

                ctx.Movies.Add(model);
                ctx.SaveChanges();
                foreach (int genreId in model.Genres)
                {
                    var movieGenre = new MovieGenre
                    {
                        MovieId = model.Id,
                        GenreId = genreId
                    };
                    ctx.MoviesGenres.Add(movieGenre);
                }
                ctx.SaveChanges();
                _cache.Remove("MovieList");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                var data = this.GetById(id);
                if (data == null)
                    return false;
                var movieGenres = ctx.MoviesGenres.Where(a => a.MovieId == data.Id);
                foreach (var movieGenre in movieGenres)
                {
                    ctx.MoviesGenres.Remove(movieGenre);
                }
                ctx.Movies.Remove(data);
                ctx.SaveChanges();
                //var cacheKey = GetCacheKey(_cacheParameters); // Regenerate the cache key
                _cache.Remove("MovieList"); // Remove data from cache
                _logger.LogInformation("Movie deleted and removed from cache");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Movie GetById(int id)
        {
            return ctx.Movies.Find(id);
        }

        public MovieListVm List(string term = "", bool paging = false, int currentPage = 0)
        {
            // var cacheKey = $"MovieList_{term}_{paging}_{currentPage}";
            _cacheParameters = new List<string> { term, paging.ToString(), currentPage.ToString() };
            // var cacheKey = GetCacheKey(_cacheParameters);
            var cacheKey = "MovieList";
            // var cacheKey = GetCacheKey(new List<string> { term, paging.ToString(), currentPage.ToString() });

            if (_cache.TryGetValue(cacheKey, out MovieListVm cachedResult))
            {
                _logger.LogInformation("found in cache" );

                // Data found in cache, return the cached result
                return cachedResult;
            }
            else
            {
                var data = new MovieListVm();

                var list = ctx.Movies.ToList();


                if (!string.IsNullOrEmpty(term))
                {
                    term = term.ToLower();
                    list = list.Where(a => a.Title.ToLower().StartsWith(term)).ToList();
                }

                if (paging)
                {
                    // here we will apply paging
                    int pageSize = 5;
                    int count = list.Count;
                    int TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                    list = list.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                    data.PageSize = pageSize;
                    data.CurrentPage = currentPage;
                    data.TotalPages = TotalPages;
                }

                foreach (var movie in list)
                {
                    var genres = (from genre in ctx.Genres
                                  join mg in ctx.MoviesGenres
                                  on genre.Id equals mg.GenreId
                                  where mg.MovieId == movie.Id
                                  select genre.GenreName
                                  ).ToList();
                    var genreNames = string.Join(',', genres);
                    movie.GenreNames = genreNames;
                }
                data.MovieList = list.AsQueryable();
                // Store the data in cache for future use
                _cache.Set(cacheKey, data, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes, adjust as needed
                });
                _logger.LogInformation("saved in cache");

                return data;
            }
        }

        public bool Update(Movie model)
        {
            try
            {
                // these genreIds are not selected by users and still present is movieGenre table corresponding to
                // this movieId. So these ids should be removed.
                var genresToDeleted = ctx.MoviesGenres.Where(a => a.MovieId == model.Id && !model.Genres.Contains(a.GenreId)).ToList();
                foreach (var mGenre in genresToDeleted)
                {
                    ctx.MoviesGenres.Remove(mGenre);
                }
                foreach (int genId in model.Genres)
                {
                    var movieGenre = ctx.MoviesGenres.FirstOrDefault(a => a.MovieId == model.Id && a.GenreId == genId);
                    if (movieGenre == null)
                    {
                        movieGenre = new MovieGenre { GenreId = genId, MovieId = model.Id };
                        ctx.MoviesGenres.Add(movieGenre);
                    }
                }

                ctx.Movies.Update(model);
                // we have to add these genre ids in movieGenre table
                ctx.SaveChanges();
                _cache.Remove("MovieList");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<int> GetGenreByMovieId(int movieId)
        {
            var genreIds = ctx.MoviesGenres.Where(a => a.MovieId == movieId).Select(a => a.GenreId).ToList();
            return genreIds;
        }
        private static string GetCacheKey(List<string> ressources)
        {
            return $"{string.Join("-",ressources)}";
        }

    }
}

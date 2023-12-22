using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MovieStore.Models.Domain
{
    public class DBContext : IdentityDbContext<ApplicationUser>
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }
        public DbSet<Movie> Movies { get;set; }
        public DbSet<Genre> Genres { get;set; }
        public DbSet<MovieGenre> MoviesGenres { get;set;}
        public DbSet<ShoppingCart> ShoppingCarts { get;set;}
        public DbSet<CartDetail> CartDetails { get; set; }
    }
}

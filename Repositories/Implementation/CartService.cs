using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieStore.Models.Domain;
using MovieStore.Repositories.Abstract;
using System.Net;

namespace MovieStore.Repositories.Implementation
{
    public class CartService : ICartService
    {
      
        private readonly DBContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartService(DBContext db, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async void AddMovieToCart(int movieId, int quantity)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();
                // cart detail section
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.MovieId == movieId);
                if (cartItem is not null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    var movie = _db.Movies.Find(movieId);
                    if (movie != null) {
                        cartItem = new CartDetail
                        {
                            MovieId = movieId,
                            ShoppingCartId = cart.Id,
                            Quantity = quantity,
                            UnitPrice = movie.Price  // it is a new line after update
                        };
                        _db.CartDetails.Add(cartItem);
                    }
                    
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
            }
            
        }
        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new Exception("Invalid userid");
            var shoppingCart = await _db.ShoppingCarts
                                  .Include(a => a.CartDetails)
                                  .ThenInclude(a => a.Movie)
                                  .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;

        }
        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (!string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from cart in _db.ShoppingCarts
                              join cartDetail in _db.CartDetails
                              on cart.Id equals cartDetail.ShoppingCartId
                              select new { cartDetail.Id }
                        ).ToListAsync();
            return data.Count;
        }
        public async void RemoveItem(int movieId)
        {
            //using var transaction = _db.Database.BeginTransaction();
            string userId = GetUserId();
            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new Exception("Invalid cart");
                // cart detail section
                var cartItem = _db.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.MovieId == movieId);
                if (cartItem is null)
                    throw new Exception("Not items in cart");
                else if (cartItem.Quantity == 1)
                    _db.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity = cartItem.Quantity - 1;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            
        }
        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }

        public async Task<ShoppingCart> GetOrCreateCartForUser(string userId)
        {
            var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

        /*public CartService(DBContext dbContext)
        {
            _ctx = dbContext;
        }

        public ShoppingCart GetOrCreateCartForUser(string userId)
        {
            ShoppingCart userCart = _ctx.ShoppingCarts
                .FirstOrDefault(cart => cart.UserId == userId && !cart.IsDeleted);

            if (userCart == null)
            {
                userCart = new ShoppingCart
                {
                    UserId = userId
                };
                _ctx.ShoppingCarts.Add(userCart);
                _ctx.SaveChanges();
            }

            return userCart;
        }

        public void AddMovieToCart(int movieId, ShoppingCart cart, int quantity)
        {
            Movie movieToAdd = _ctx.Movies.FirstOrDefault(movie => movie.Id == movieId);
            if (movieToAdd != null && quantity > 0)
            {
                var cartDetail = new CartDetail
                {
                    MovieId = movieId,
                    Quantity = quantity,
                    UnitPrice = movieToAdd.Price,
                    ShoppingCartId = cart.Id
                };

                _ctx.ShoppingCarts.Add(cartDetail);
                _ctx.SaveChanges();
            }
        }*/
    }

}

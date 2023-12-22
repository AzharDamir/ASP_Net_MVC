using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieStore.Models.Domain;
using MovieStore.Repositories.Abstract;

namespace MovieStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartRepo;
        private readonly ILogger<ICartService> _logger;

        public CartController(ICartService cartRepo, ILogger<ICartService> logger)
        {
            _cartRepo = cartRepo;
            _logger = logger;
        }
        public Task<IActionResult> AddItem(int movieId, int qty = 1, int redirect = 0)
        {
            _logger.LogInformation("add item id" + movieId + " with quantity : " + qty);
             _cartRepo.AddMovieToCart(movieId, qty);
            /*if (redirect == 0)
                return Ok(cartCount);*/
            return Task.FromResult<IActionResult>(RedirectToAction("GetUserCart"));
        }

        public async Task<IActionResult> RemoveItem(int movieId)
        {
              _cartRepo.RemoveItem(movieId);
            _logger.LogInformation("remove item id" + movieId);
            return RedirectToAction("GetUserCart");
        }
        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepo.GetUserCart();
            if (cart != null)
            {
                _logger.LogInformation("cart:" + cart.ToString());
            }
            else
            {
                _logger.LogInformation("cart:" + null);
            }
            
            return View(cart);
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            int cartItem = await _cartRepo.GetCartItemCount();
            _logger.LogInformation("total item " + cartItem.ToString() );
            return Ok(cartItem);
        }

    }
}

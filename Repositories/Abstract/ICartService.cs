using MovieStore.Models.Domain;

namespace MovieStore.Repositories.Abstract
{
    public interface ICartService
    {
        /* Task<int> AddItem(int movieId, int qty);
         Task<int> RemoveItem(int bookId);

         Task<ShoppingCart> GetCart(string userId);*/
        Task<ShoppingCart> GetOrCreateCartForUser(string userId);
        void AddMovieToCart(int movieId, int quantity);
        Task<ShoppingCart> GetCart(string userId);
        void RemoveItem(int bookId);
        Task<ShoppingCart> GetUserCart();
        Task<int> GetCartItemCount(string userId = "");
    }
}

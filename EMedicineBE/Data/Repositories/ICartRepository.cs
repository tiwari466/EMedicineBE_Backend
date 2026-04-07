using EMedicineBE.Entities;

public interface ICartRepository
{
    Task<bool> AddToCart(Cart cart);
    Task<List<Cart>> GetCartItems(int userId);
    Task<bool> UpdateQty(int cartId, int qty);
    Task<bool> RemoveItem(int cartId);
}
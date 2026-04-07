using EMedicineBE.Common;
using EMedicineBE.Entities;

public class CartService : ICartService
{
    private readonly ICartRepository _repo;
    public CartService(ICartRepository repo) => _repo = repo;

    public async Task<ApiResponse<string>> Add(Cart cart)
        => await _repo.AddToCart(cart)
            ? ApiResponse<string>.Ok("Added to cart")
            : ApiResponse<string>.Fail("Failed");

    public async Task<ApiResponse<List<Cart>>> Items(int userId)
        => ApiResponse<List<Cart>>.Ok(await _repo.GetCartItems(userId));

    public async Task<ApiResponse<string>> UpdateQty(int cartId, int qty)
        => await _repo.UpdateQty(cartId, qty)
            ? ApiResponse<string>.Ok("Updated")
            : ApiResponse<string>.Fail("Failed");

    public async Task<ApiResponse<string>> Remove(int cartId)
        => await _repo.RemoveItem(cartId)
            ? ApiResponse<string>.Ok("Removed")
            : ApiResponse<string>.Fail("Failed");
}

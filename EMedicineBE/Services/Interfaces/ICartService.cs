using EMedicineBE.Common;
using EMedicineBE.Entities;

public interface ICartService
{
    Task<ApiResponse<string>> Add(Cart cart);
    Task<ApiResponse<List<Cart>>> Items(int userId);
    Task<ApiResponse<string>> UpdateQty(int cartId, int qty);
    Task<ApiResponse<string>> Remove(int cartId);
}

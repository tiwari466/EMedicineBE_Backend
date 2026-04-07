using EMedicineBE.Dto.Order;
using EMedicineBE.Entities;

namespace EMedicineBE.Data.Repositories
{
    public interface IOrderRepository
    {
        Task<bool> PlaceOrderAsync(PlaceOrderDto dto);
        Task<List<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderDetailsAsync(int userId, int orderId);
        Task<int> CancelOrderAsync(CancelOrderDto dto);
    }
}
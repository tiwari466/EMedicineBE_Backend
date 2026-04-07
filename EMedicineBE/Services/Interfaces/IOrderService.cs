using EMedicineBE.Common;
using EMedicineBE.Dto.Order;
using EMedicineBE.Entities;

namespace EMedicineBE.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<string>> PlaceOrder(PlaceOrderDto dto);
        Task<ApiResponse<List<Order>>> UserOrders(int userId);
        Task<ApiResponse<Order>> OrderDetails(int userId, int orderId);
        Task<ApiResponse<string>> CancelOrder(CancelOrderDto dto);
    }
}

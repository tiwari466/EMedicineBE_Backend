using EMedicineBE.Common;
using EMedicineBE.Data.Repositories;
using EMedicineBE.Dto.Order;
using EMedicineBE.Entities;
using EMedicineBE.Services.Interfaces;

namespace EMedicineBE.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;

        public OrderService(IOrderRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<string>> PlaceOrder(PlaceOrderDto dto)
        {
            bool ok = await _repo.PlaceOrderAsync(dto);
            return ok
                ? ApiResponse<string>.Ok("Order placed successfully")
                : ApiResponse<string>.Fail("Cart is empty");
        }

        public async Task<ApiResponse<List<Order>>> UserOrders(int userId)
            => ApiResponse<List<Order>>.Ok(await _repo.GetUserOrdersAsync(userId));

        public async Task<ApiResponse<Order>> OrderDetails(int userId, int orderId)
        {
            var order = await _repo.GetOrderDetailsAsync(userId, orderId);
            return order == null
                ? ApiResponse<Order>.Fail("Order not found", 404)
                : ApiResponse<Order>.Ok(order);
        }

        public async Task<ApiResponse<string>> CancelOrder(CancelOrderDto dto)
        {
            int r = await _repo.CancelOrderAsync(dto);
            return r switch
            {
                1 => ApiResponse<string>.Ok("Order cancelled"),
                -2 => ApiResponse<string>.Fail("Delivered order cannot be cancelled"),
                0 => ApiResponse<string>.Fail("Order not found"),
                _ => ApiResponse<string>.Fail("Failed to cancel order")
            };
        }
    }
}

using EMedicineBE.Dto.Order;
using EMedicineBE.Models;
using EMedicineBE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        // ✅ USER ORDER LIST
        [HttpPost("placeOrder")]
        public async Task<IActionResult> Place(PlaceOrderDto dto)
                 => Ok(await _service.PlaceOrder(dto));

        [HttpGet("userOrderList/{user_id}")]
        public async Task<IActionResult> UserOrders(int userId)
            => Ok(await _service.UserOrders(userId));

        [HttpGet("orderDetails/{userId}/{orderId}")]
        public async Task<IActionResult> Details(int userId, int orderId)
            => Ok(await _service.OrderDetails(userId, orderId));

        [HttpPost("cancelOrder")]
        public async Task<IActionResult> Cancel(CancelOrderDto dto)
            => Ok(await _service.CancelOrder(dto));

    }
}



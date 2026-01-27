using EMedicineBE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ✅ USER ORDER LIST
        [HttpGet("userOrderList/{user_id}")]
        public IActionResult UserOrderList(int user_id)
        {
            DAL dal = new DAL();

            string cs = _configuration.GetConnectionString("PostgresCS");
            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.userOrderList(user_id, connection);

            return Ok(response);
        }

        // ✅ PLACE ORDER
        [HttpPost("placeOrder")]
        public IActionResult PlaceOrder([FromBody] PlaceOrderDto dto)
        {
            if (dto == null || dto.user_id <= 0)
                return BadRequest("Invalid order request");

            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.placeOrder(dto, connection);

            return Ok(response);
        }

        // ✅ ORDER DETAILS
        [HttpGet("orderDetails/{userId}/{orderId}")]
        public IActionResult GetOrderDetails(int userId, int orderId)
        {
            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.GetOrderDetails(userId, orderId, connection);

            return Ok(response);
        }


        [HttpPost("cancelOrder")]
        public IActionResult CancelOrder([FromBody] CancelOrderDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request");

            DAL dal = new DAL();

            string cs = _configuration.GetConnectionString("PostgresCS");
            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.cancelOrder(dto, connection);

            return Ok(response);
        }

    }
}

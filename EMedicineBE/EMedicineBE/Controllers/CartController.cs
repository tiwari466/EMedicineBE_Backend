using EMedicineBE.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ✅ ADD TO CART
        [HttpPost("addToCart")]
        public IActionResult AddToCart([FromBody] Cart cart)
        {
            if (cart == null)
                return BadRequest("Invalid cart data");

            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.addToCart(cart, connection);

            return Ok(response);
        }

        // ✅ GET CART ITEMS
        [HttpGet("getCartItems/{user_id}")]
        public IActionResult GetCartItems(int user_id)
        {
            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.getCartItems(user_id, connection);

            return Ok(response);
        }

        // ✅ REMOVE CART ITEM
        [HttpDelete("removeCartItem/{cartId}")]
        public IActionResult RemoveCartItem(int cartId)
        {
            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.removeCartItem(cartId, connection);

            return Ok(response);
        }

        // ✅ UPDATE CART QTY
        [HttpPut("updateCartQty")]
        public IActionResult UpdateCartQty([FromBody] UpdateCartQtyRequest req)
        {
            if (req == null)
                return BadRequest("Invalid request");

            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.updateCartQty(req.id, req.qty, connection);

            return Ok(response);
        }
    }
}

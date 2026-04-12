using EMedicineBE.Dto.Cart;
using EMedicineBE.Entities;
using EMedicineBE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        public CartController(ICartService service)
        {
            _service = service;
        }

        [HttpPost("addToCart")]
        public async Task AddToCart([FromBody] Cart cart)
            => Ok(await _service.Add(cart));

        [HttpGet("getCartItems/{user_id}")]
        public async Task<IActionResult> GetCartItems(int user_id)
        => Ok(await _service.Items(user_id));

        [HttpDelete("removeCartItem/{cartId}")]
        public async Task<IActionResult> RemoveCartItem(int cartId)
            => Ok(await _service.Remove(cartId));

        [HttpPut("updateCartQty")]
        public async Task<IActionResult> UpdateCartQty(int id, int qty)
        => Ok(await _service.UpdateQty(id, qty));
    }
}

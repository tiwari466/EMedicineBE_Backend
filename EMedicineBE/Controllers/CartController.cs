using EMedicineBE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

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

        [HttpPost]
        [Route("addToCart")]
        public Response AddToCart(Cart cart)
        {
            DAL dal = new DAL();
            Response response;

            using (SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("EMedCS").ToString()))
            {
                response = dal.addToCart(cart, connection);
            }

            return response;
        }
        [HttpGet("getCartItems/{user_id}")]
        public IActionResult GetCartItems(int user_id)
        {
            Response response = new Response();
            DAL dal = new DAL();

            using (SqlConnection connection = new SqlConnection(
                 _configuration.GetConnectionString("EMedCS")))
            {
                response = dal.getCartItems(user_id, connection);
            }

            return Ok(response);
        }

        [HttpDelete("removeCartItem/{cartId}")]
        public Response removeCartItem(int cartId)
        {
            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("EMedCS"));
            DAL dal = new DAL();
            return dal.removeCartItem(cartId, connection);
        }

        [HttpPut("updateCartQty")]
        public Response updateCartQty([FromBody] UpdateCartQtyRequest req)
        {
            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("EMedCS"));
            DAL dal = new DAL();
            return dal.updateCartQty(req.id, req.qty, connection);
        }



    }
}

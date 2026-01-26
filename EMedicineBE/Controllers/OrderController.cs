using EMedicineBE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

        [HttpGet("userOrderList/{user_id}")]
        public IActionResult UserOrderList(int user_id)
        {
            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("EMedCS");

            using (SqlConnection connection = new SqlConnection(cs))
            {
                var response = dal.userOrderList(user_id, connection);
                return Ok(response);
            }
        }

        [HttpPost("placeOrder")]
        public IActionResult PlaceOrder([FromBody] PlaceOrderDto dto)
        {
            Response response = new Response();
            DAL dal = new DAL();

            string cs = _configuration.GetConnectionString("EMedCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Connection string 'EMedCS' is missing in appsettings.json");

            using (SqlConnection connection = new SqlConnection(cs))
            {
                response = dal.placeOrder(dto, connection);
            }

            return Ok(response);
        }
    }
}

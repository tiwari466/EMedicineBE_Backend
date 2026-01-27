using EMedicineBE.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ✅ ADD / UPDATE MEDICINE
        [HttpPost("addUpdateMedicine")]
        public IActionResult AddUpdateMedicine([FromBody] Medicine medicines)
        {
            if (medicines == null)
                return BadRequest("Invalid medicine data");

            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.addUpdateMedicine(medicines, connection);

            return Ok(response);
        }

        // ✅ USER LIST
        [HttpGet("userList")]
        public IActionResult UserList()
        {
            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.userList(connection);

            return Ok(response);
        }

        // ✅ GET MEDICINES
        [HttpGet("getMedicines")]
        public IActionResult GetMedicines()
        {
            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            if (string.IsNullOrEmpty(cs))
                return BadRequest("Postgres connection string missing");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.getMedicines(connection);

            return Ok(response);
        }
    }
}

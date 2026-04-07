using EMedicineBE.Entities;
using EMedicineBE.Models;
using EMedicineBE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IMedicineService _service;

        public AdminController(IMedicineService service)
        {
            _service = service;
        }

        // ✅ ADD / UPDATE MEDICINE
        [HttpPost("addUpdateMedicine")]
       public async Task<IActionResult> AddUpdateMedicine(Medicine medicine)
            => Ok(await _service.Save(medicine));

        // ✅ USER LIST
        //[HttpGet("userList")]
        //public IActionResult UserList()
        //{
        //    DAL dal = new DAL();
        //    string cs = _configuration.GetConnectionString("PostgresCS");

        //    if (string.IsNullOrEmpty(cs))
        //        return BadRequest("Postgres connection string missing");

        //    using var connection = new NpgsqlConnection(cs);
        //    var response = dal.userList(connection);

        //    return Ok(response);
        //}

        // ✅ GET MEDICINES
        [HttpGet("getMedicines")]
        public async Task<IActionResult> GetMedicines()
            => Ok(await _service.GetAll());
    }
}

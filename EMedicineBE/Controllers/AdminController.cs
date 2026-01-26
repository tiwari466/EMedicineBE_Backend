using EMedicineBE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

        [HttpPost]
        [Route("addUpdateMedicine")]
        public Response AddUpdateMedicine(Medicine medicines)
        {
            DAL dal = new DAL();
            Response response;

            using (SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("EMedCS").ToString()))
            {
                response = dal.addUpdateMedicine(medicines, connection);
            }

            return response;
        }
        [HttpGet]
        [Route("userList")]
        public Response UserList()
        {
            DAL dal = new DAL();
            Response response;

            using (SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("EMedCS").ToString()))
            {
                response = dal.userList(connection);
            }

            return response;
        }
        [HttpGet]
        [Route("getMedicines")]
        public Response GetMedicines()
        {
            DAL dal = new DAL();
            Response response;

            using (SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("EMedCS").ToString()))
            {
                response = dal.getMedicines(connection);
            }

            return response;
        }
    }
}

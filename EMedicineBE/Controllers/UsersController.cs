using EMedicineBE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;


namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpPost]
        [Route("registration")]
        public Response register([FromBody] RegisterRequest request)
        {
            Response response = new Response();
            DAL dal = new DAL();
            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("EMedCS").ToString());
            response = dal.register(request, connection);
            return response;

        }

        [HttpPost]
        [Route("login")]
        public IActionResult login([FromBody] LoginDto model)
        {
            DAL dal = new DAL();
            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("EMedCS"));

            User u = new User();
            u.email = model.email;
            u.password = model.password;

            var response = dal.login(u, connection);
            return Ok(response);
        }

        [HttpPost]
        [Route("viewUser")]
        public Response viewUser(User users)
        {
            DAL dal = new DAL();
            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("EMedCS").ToString());
            Response response = dal.viewUser(users, connection);
            return response;
        }

        [HttpPost]
        [Route("updateProfile")]
        public Response UpdateProfile([FromBody] UpdateProfileRequest users)
        {
            DAL dal = new DAL();
            Response response;

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("EMedCS").ToString()))
            {
                response = dal.updateProfile(users, connection);
            }

            return response;
        }
        [HttpPost]
        [Route("uploadProfilePic")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePic([FromForm] UploadProfilePicRequest model)
        {
            if (model.file == null || model.file.Length == 0)
                return BadRequest("No file uploaded");

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profilepics");
            Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.file.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.file.CopyToAsync(stream);
            }

            string fileUrl = $"{Request.Scheme}://{Request.Host}/profilepics/{fileName}";

            DAL dal = new DAL();
            Response response;

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("EMedCS")))
            {
                response = dal.updateProfilePicture(model.user_id, fileUrl, connection);
            }

            response.picture = fileUrl;
            return Ok(response);
        }



    }
}

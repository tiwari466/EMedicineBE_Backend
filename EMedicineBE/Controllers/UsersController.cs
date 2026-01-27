using EMedicineBE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.IO;

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

        // ✅ REGISTER
        [HttpPost("registration")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.register(request, connection);

            return Ok(response);
        }

        // ✅ LOGIN
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto model)
        {
            if (model == null)
                return BadRequest("Invalid login request");

            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            User user = new User
            {
                email = model.email,
                password = model.password
            };

            using var connection = new NpgsqlConnection(cs);
            var response = dal.login(user, connection);

            return Ok(response);
        }

        // ✅ VIEW USER
        [HttpPost("viewUser")]
        public IActionResult ViewUser([FromBody] User users)
        {
            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.viewUser(users, connection);

            return Ok(response);
        }

        // ✅ UPDATE PROFILE
        [HttpPost("updateProfile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest users)
        {
            if (users == null)
                return BadRequest("Invalid request");

            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.updateProfile(users, connection);

            return Ok(response);
        }

        // ✅ UPLOAD PROFILE PICTURE
        [HttpPost("uploadProfilePic")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePic([FromForm] UploadProfilePicRequest model)
        {
            if (model.file == null || model.file.Length == 0)
                return BadRequest("No file uploaded");

            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "profilepics"
            );

            Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid() + Path.GetExtension(model.file.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.file.CopyToAsync(stream);
            }

            string fileUrl = $"{Request.Scheme}://{Request.Host}/profilepics/{fileName}";

            DAL dal = new DAL();
            string cs = _configuration.GetConnectionString("PostgresCS");

            using var connection = new NpgsqlConnection(cs);
            var response = dal.updateProfilePicture(model.user_id, fileUrl, connection);

            response.picture = fileUrl;
            return Ok(response);
        }
    }
}

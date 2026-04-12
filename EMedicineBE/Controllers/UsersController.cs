using EMedicineBE.Dto.Auth;
using EMedicineBE.Dto.User;
using EMedicineBE.Entities;
using EMedicineBE.Models;
using EMedicineBE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.IO;

namespace EMedicineBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
     => Ok(await _service.Register(dto));

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
            => Ok(await _service.Login(dto));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        => Ok(await _service.GetUser(id));

        [HttpPut("updateProfile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequestDto dto)
            => Ok(await _service.UpdateProfile(dto));
        [HttpPost("uploadProfilePic")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePicture(
        [FromForm] int user_id,
        [FromForm] IFormFile picture)
        {
            var result = await _service.UpdateProfilePicture(user_id, picture);

            return StatusCode(result.StatusCode, result);
        }


        //// ✅ VIEW USER
        //[HttpPost("viewUser")]
        //public IActionResult ViewUser([FromBody] User users)
        //{
        //    DAL dal = new DAL();
        //    string cs = _configuration.GetConnectionString("PostgresCS");

        //    using var connection = new NpgsqlConnection(cs);
        //    var response = dal.viewUser(users, connection);

        //    return Ok(response);
        //}

        //// ✅ UPDATE PROFILE
        //[HttpPost("updateProfile")]
        //public IActionResult UpdateProfile([FromBody] UpdateProfileRequestDto users)
        //{
        //    if (users == null)
        //        return BadRequest("Invalid request");

        //    DAL dal = new DAL();
        //    string cs = _configuration.GetConnectionString("PostgresCS");

        //    using var connection = new NpgsqlConnection(cs);
        //    var response = dal.updateProfile(users, connection);

        //    return Ok(response);
        //}

        //// ✅ UPLOAD PROFILE PICTURE
        //[HttpPost("uploadProfilePic")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> UploadProfilePic([FromForm] UploadProfilePicRequestDto model)
        //{
        //    if (model.file == null || model.file.Length == 0)
        //        return BadRequest("No file uploaded");

        //    string folderPath = Path.Combine(
        //        Directory.GetCurrentDirectory(),
        //        "wwwroot",
        //        "profilepics"
        //    );

        //    Directory.CreateDirectory(folderPath);

        //    string fileName = Guid.NewGuid() + Path.GetExtension(model.file.FileName);
        //    string filePath = Path.Combine(folderPath, fileName);

        //    using (var stream = new FileStream(filePath, FileMode.Create))
        //    {
        //        await model.file.CopyToAsync(stream);
        //    }

        //    string fileUrl = $"{Request.Scheme}://{Request.Host}/profilepics/{fileName}";

        //    DAL dal = new DAL();
        //    string cs = _configuration.GetConnectionString("PostgresCS");

        //    using var connection = new NpgsqlConnection(cs);
        //    var response = dal.updateProfilePicture(model.user_id, fileUrl, connection);

        //    response.picture = fileUrl;
        //    return Ok(response);
        //}
    }
}

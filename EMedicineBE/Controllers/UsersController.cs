using EMedicineBE.Dto.Auth;
using EMedicineBE.Dto.User;
using EMedicineBE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequestDto dto)
        {
            return Ok(await _service.Register(dto));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginDto dto)
        {
            return Ok(await _service.Login(dto));
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            return Ok(await _service.GetUser(id));
        }

        [Authorize]
        [HttpPut("updateProfile")]
        public async Task<IActionResult> UpdateProfile(
            UpdateProfileRequestDto dto)
        {
            return Ok(await _service.UpdateProfile(dto));
        }

        [Authorize]
        [HttpPost("uploadProfilePic")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePicture(
            [FromForm] int user_id,
            [FromForm] IFormFile picture)
        {
            var result =
                await _service.UpdateProfilePicture(
                    user_id,
                    picture);

            return StatusCode(
                result.StatusCode,
                result);
        }
    }
}
using EMedicineBE.Common;
using EMedicineBE.Data.Repositories;
using EMedicineBE.Dto.Auth;
using EMedicineBE.Dto.User;
using EMedicineBE.Entities;
using EMedicineBE.Services.Interfaces;
using EMedicineBE.Services.Security;

namespace EMedicineBE.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly JwtService _jwtService;

        public UserService(
            IUserRepository repo,
            JwtService jwtService)
        {
            _repo = repo;
            _jwtService = jwtService;
        }

        public async Task<ApiResponse<string>> Register(RegisterRequestDto dto)
        {
            int rows = await _repo.Register(dto);

            return rows > 0
                ? ApiResponse<string>.Ok("User registered successfully")
                : ApiResponse<string>.Fail("Registration failed");
        }

        public async Task<ApiResponse<LoginResponseDto>> Login(LoginDto dto)
        {
            var user = await _repo.Login(dto.email);

            if (user == null)
            {
                return ApiResponse<LoginResponseDto>
                    .Fail("Invalid email or password");
            }

            bool valid = BCrypt.Net.BCrypt.Verify(
                dto.password,
                user.password_hash);

            if (!valid)
            {
                return ApiResponse<LoginResponseDto>
                    .Fail("Invalid email or password");
            }

            if (!user.is_active)
            {
                return ApiResponse<LoginResponseDto>
                    .Fail("Account is inactive");
            }

            string token = _jwtService.GenerateToken(user);

            return ApiResponse<LoginResponseDto>.Ok(
                new LoginResponseDto
                {
                    user_id = user.user_id,
                    email = user.email,
                    role = user.role,
                    picture = user.picture,
                    token = token
                });
        }

        public async Task<ApiResponse<User>> GetUser(int userId)
        {
            var user = await _repo.GetUser(userId);

            return user == null
                ? ApiResponse<User>.Fail("User not found", 404)
                : ApiResponse<User>.Ok(user);
        }

        public async Task<ApiResponse<string>> UpdateProfile(UpdateProfileRequestDto dto)
        {
            bool ok = await _repo.UpdateProfile(dto);

            return ok
                ? ApiResponse<string>.Ok("Profile updated")
                : ApiResponse<string>.Fail("Update failed");
        }

        public async Task<ApiResponse<string>> UpdateProfilePicture(
            int userId,
            IFormFile picture)
        {
            try
            {
                if (picture == null || picture.Length == 0)
                    return ApiResponse<string>.Fail("No file uploaded");

                if (picture.Length > 2 * 1024 * 1024)
                    return ApiResponse<string>.Fail("File size must be less than 2MB");

                var allowedTypes =
                    new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var ext =
                    Path.GetExtension(picture.FileName).ToLower();

                if (!allowedTypes.Contains(ext))
                    return ApiResponse<string>.Fail(
                        "Only JPG, PNG, WEBP allowed");

                var uploadFolder =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "profilepics");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileName =
                    $"{Guid.NewGuid()}{ext}";

                var filePath =
                    Path.Combine(uploadFolder, fileName);

                using (var stream =
                    new FileStream(filePath, FileMode.Create))
                {
                    await picture.CopyToAsync(stream);
                }

                var imagePath =
                    $"/profilepics/{fileName}";

                bool saved =
                    await _repo.UpdateProfilePicture(
                        userId,
                        imagePath);

                if (!saved)
                    return ApiResponse<string>.Fail(
                        "Database update failed");

                return ApiResponse<string>.Ok(fileName);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(
                    $"Upload failed: {ex.Message}");
            }
        }
    }
}
using EMedicineBE.Common;
using EMedicineBE.Data.Repositories;
using EMedicineBE.Dto.Auth;
using EMedicineBE.Dto.User;
using EMedicineBE.Entities;
using EMedicineBE.Services.Interfaces;

namespace EMedicineBE.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<string>> Register(RegisterRequestDto dto)
        {
            int rows = await _repo.Register(dto);

            return rows > 0
                ? ApiResponse<string>.Ok("User registered successfully")
                : ApiResponse<string>.Fail("Registration failed");
        }

        public async Task<ApiResponse<User>> Login(LoginDto dto)
        {
            var user = await _repo.Login(dto.email, dto.password);

            return user == null
                ? ApiResponse<User>.Fail("Invalid credentials", 401)
                : ApiResponse<User>.Ok(user);
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

        public async Task<ApiResponse<string>> UpdateProfilePicture(int userId, IFormFile picture)
        {
            try
            {
                // -----------------------------
                // Validation
                // -----------------------------

                if (picture == null || picture.Length == 0)
                    return ApiResponse<string>.Fail("No file uploaded");

                // Max 2MB
                if (picture.Length > 2 * 1024 * 1024)
                    return ApiResponse<string>.Fail("File size must be less than 2MB");

                var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var ext = Path.GetExtension(picture.FileName).ToLower();

                if (!allowedTypes.Contains(ext))
                    return ApiResponse<string>.Fail("Only JPG, PNG, WEBP allowed");


                // -----------------------------
                // Create Folder If Not Exists
                // -----------------------------

                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profilepics");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }


                // -----------------------------
                // Generate File Name
                // -----------------------------

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadFolder, fileName);


                // -----------------------------
                // Save File
                // -----------------------------

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await picture.CopyToAsync(stream);
                }


                // -----------------------------
                // Save In Database
                // -----------------------------

                var imagePath = $"/profilepics/{fileName}";
                bool saved = await _repo.UpdateProfilePicture(userId, imagePath);

                if (!saved)
                    return ApiResponse<string>.Fail("Database update failed");


                // -----------------------------
                // Success
                // -----------------------------

                return ApiResponse<string>.Ok(fileName);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Upload failed: {ex.Message}");
            }
        }



    }
}


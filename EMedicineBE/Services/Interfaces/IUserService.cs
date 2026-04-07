using EMedicineBE.Common;
using EMedicineBE.Dto.Auth;
using EMedicineBE.Dto.User;
using EMedicineBE.Entities;

namespace EMedicineBE.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<string>> Register(RegisterRequestDto dto);
        Task<ApiResponse<User>> Login(LoginDto dto);

        Task<ApiResponse<User>> GetUser(int userId);
        Task<ApiResponse<string>> UpdateProfile(UpdateProfileRequestDto dto);
        Task<ApiResponse<string>> UpdateProfilePicture(int userId, IFormFile picture);
    }
}

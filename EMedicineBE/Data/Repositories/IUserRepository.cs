using EMedicineBE.Dto.Auth;
using EMedicineBE.Dto.User;
using EMedicineBE.Entities;

namespace EMedicineBE.Data.Repositories
{
    public interface IUserRepository
    {
        Task<int> Register(RegisterRequestDto dto);
        Task<User?> Login(string email, string password);
        Task<User?> GetUser(int userId);
        Task<bool> UpdateProfile(UpdateProfileRequestDto dto);
        Task<bool> UpdateProfilePicture(int userId, string pictureUrl);
        Task<List<User>> GetUsers();
    }
}

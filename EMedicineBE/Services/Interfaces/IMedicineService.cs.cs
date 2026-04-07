using EMedicineBE.Common;
using EMedicineBE.Entities;

namespace EMedicineBE.Services.Interfaces
{
    public interface IMedicineService
    {
        Task<ApiResponse<string>> Save(Medicine medicine);
        Task<ApiResponse<List<Medicine>>> GetAll();
    }
}

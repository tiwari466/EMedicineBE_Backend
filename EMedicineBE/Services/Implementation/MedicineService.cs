using EMedicineBE.Common;
using EMedicineBE.Data.Repositories;
using EMedicineBE.Entities;
using EMedicineBE.Services.Interfaces;

namespace EMedicineBE.Services.Implementations
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _repo;

        public MedicineService(IMedicineRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<string>> Save(Medicine medicine)
        {
            bool ok = await _repo.SaveAsync(medicine);
            return ok
                ? ApiResponse<string>.Ok(medicine.id == 0 ? "Medicine added" : "Medicine updated")
                : ApiResponse<string>.Fail("Save failed");
        }

        public async Task<ApiResponse<List<Medicine>>> GetAll()
            => ApiResponse<List<Medicine>>.Ok(await _repo.GetAllAsync());
    }
}

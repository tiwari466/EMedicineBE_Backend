using EMedicineBE.Entities;

namespace EMedicineBE.Data.Repositories
{
    public interface IMedicineRepository
    {
        Task<bool> SaveAsync(Medicine medicine);
        Task<List<Medicine>> GetAllAsync();
    }
}

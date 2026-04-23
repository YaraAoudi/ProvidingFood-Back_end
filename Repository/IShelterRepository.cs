using ProvidingFood2.DTO;
using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface IShelterRepository
    {
        Task<int> CreateAsync(Shelter shelter);
        Task<IEnumerable<Shelter>> GetPendingAsync();
        Task<Shelter> GetByIdAsync(int id);

        Task ApproveAsync(int id);
        Task RejectAsync(int id);
        Task<IEnumerable<Shelter>> GetAllAsync();
      
    }
}

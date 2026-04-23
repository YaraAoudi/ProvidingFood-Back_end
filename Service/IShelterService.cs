using ProvidingFood2.DTO;
using ProvidingFood2.Model;

namespace ProvidingFood2.Service
{
    public interface IShelterService
    {
        Task<int> RegisterAsync(RegisterShelterDto dto, int userId);
        Task<IEnumerable<Shelter>> GetPendingAsync();
        Task ApproveAsync(int id);
        Task RejectAsync(int id);
        Task<IEnumerable<Shelter>> GetAllrequestAsync();
        Task<Shelter> GetByIdAsync(int id);
       
    }
}

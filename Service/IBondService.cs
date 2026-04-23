using ProvidingFood2.DTO;

namespace ProvidingFood2.Service
{
    public interface IBondService
    {
        Task<string> CreateBondSession(CreateBondDto dto, int userId);
       
    }
}

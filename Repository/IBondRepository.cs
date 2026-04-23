using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
    public interface IBondRepository
    {
        Task<decimal> GetActiveBondPriceAsync();
        Task SetNewBondPriceAsync(decimal newPrice);
        Task<IEnumerable<GiftDonation>> GetAllAsync();
    }
}

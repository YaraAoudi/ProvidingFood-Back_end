using ProvidingFood2.DTO;

namespace ProvidingFood2.Service
{
    public interface IGiftService
    {
        Task<string> CreateGiftSession(CreateGiftDonationDto dto, int userId);
        Task<string> CreateGiftChallengeSession(CreateGiftDonationDto dto, int userId);
    }
}

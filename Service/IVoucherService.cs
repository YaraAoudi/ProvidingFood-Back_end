using ProvidingFood2.DTOs;
using ProvidingFood2.Model;

public interface IVoucherService
{
	Task<Voucher> GenerateVoucherAsync(CreateVoucherDto dto);
	Task<Voucher> GetVoucherAsync(string code);
	Task RedeemVoucherAsync(string code);
	byte[] GenerateQrImage(string code);
	Task<IEnumerable<Voucher>> GetAllVouchersAsync();
   
}
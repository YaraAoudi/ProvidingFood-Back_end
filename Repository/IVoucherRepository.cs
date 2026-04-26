using ProvidingFood2.Model;

public interface IVoucherRepository
{
	Task<int> CreateAsync(Voucher voucher);
	Task<Voucher?> GetByCodeAsync(string code);
	Task<Voucher?> GetByIdAsync(int id);
	Task MarkAsUsedAsync(int voucherId);
    Task<IEnumerable<Voucher>> GetVoucherAsync();
    Task<int?> GetUserIdByBeneficiaryId(int beneficiaryId);


}
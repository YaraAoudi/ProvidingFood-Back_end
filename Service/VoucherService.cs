using ProvidingFood2.DTOs;
using ProvidingFood2.Model;
using ProvidingFood2.Service;

public class VoucherService : IVoucherService
{
	private readonly IVoucherRepository _repository;
	private readonly IQrService _qrService;
	private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;

    public VoucherService(
		IVoucherRepository repository,
		IQrService qrService,
		IConfiguration configuration, INotificationService notificationService)
	{
		_repository = repository;
		_qrService = qrService;
		_configuration = configuration;
        _notificationService = notificationService;
	}

    public async Task<Voucher> GenerateVoucherAsync(CreateVoucherDto dto)
    {
        var voucher = new Voucher
        {
            BeneficiaryId = dto.BeneficiaryId,
            BeneficiaryName = dto.BeneficiaryName,
            StoreName = dto.StoreName,
            StoreLocation = dto.StoreLocation,
            BasketCount = dto.BasketCount,
            ExpiryDate = dto.ExpiryDate,
            QRCode = $"VCH-{Guid.NewGuid()}",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        var id = await _repository.CreateAsync(voucher);
        voucher.VoucherId = id;

        // 🔥 جيب UserId
        var userId = await _repository.GetUserIdByBeneficiaryId(dto.BeneficiaryId);

        Console.WriteLine("UserId = " + userId); // مهم للتجربة

        // 🔥 ابعت إشعار
        if (userId != null)
        {
            await _notificationService.SendNotificationAsync(
                userId.Value,
                "🧾 تم استلام قسيمة",
                $"تم إضافة قسيمة من {dto.StoreName}"
            );
        }

        return voucher;
    }

    public async Task<Voucher> GetVoucherAsync(string code)
	{
		var voucher = await _repository.GetByCodeAsync(code);

		if (voucher == null)
			throw new Exception("القسيمة غير موجودة");

		return voucher;
	}

	public async Task RedeemVoucherAsync(string code)
	{
		var voucher = await _repository.GetByCodeAsync(code);

		if (voucher == null)
			throw new Exception("القسيمة غير موجودة");

		if (voucher.Status == "Used")
			throw new Exception("تم استخدامها مسبقاً");

		if (voucher.ExpiryDate < DateTime.UtcNow)
			throw new Exception("القسيمة منتهية");

		await _repository.MarkAsUsedAsync(voucher.VoucherId);
	}

	public byte[] GenerateQrImage(string code)
	{

		return _qrService.GenerateQr(code);
	}

    public Task<IEnumerable<Voucher>> GetAllVouchersAsync()
             => _repository.GetVoucherAsync();


   
}
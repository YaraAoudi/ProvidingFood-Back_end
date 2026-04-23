using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTOs;
using System.Security.Claims;

[Route("api/voucher")]
[ApiController]
public class VoucherController : ControllerBase
{
	private readonly IVoucherService _service;

	public VoucherController(IVoucherService service)
	{
		_service = service;
	}
	
	[HttpPost("generate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Generate(CreateVoucherDto dto)
	{
		var voucher = await _service.GenerateVoucherAsync(dto);

		return Ok(new
		{
			voucher.VoucherId,
			voucher.QRCode,
			voucher.Status
		});
	}

	[HttpGet("{code}")]
	public async Task<IActionResult> Get(string code)
	{
		var voucher = await _service.GetVoucherAsync(code);
		return Ok(voucher);
	}

	[HttpPost("redeem/{code}")]
	public async Task<IActionResult> Redeem(string code)
	{
		await _service.RedeemVoucherAsync(code);
		return Ok("تم تأكيد الاستلام");
	}

	[HttpGet("qr/{code}")]
	public IActionResult GetQr(string code)
	{
		var image = _service.GenerateQrImage(code);
		return File(image, "image/png");
	}

    
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllVoucher()
    {
        var requests = await _service.GetAllVouchersAsync();
        return Ok(requests);
    }


}
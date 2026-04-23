
using QRCoder;

namespace ProvidingFood2.Service
{
	public class QrService : IQrService
	{
		public byte[] GenerateQr(string url)
		{
			using var generator = new QRCodeGenerator();
			var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
			using var qr = new PngByteQRCode(data);
			return qr.GetGraphic(20);

		}
	}
}
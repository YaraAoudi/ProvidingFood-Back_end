namespace ProvidingFood2.Service
{
    public interface IQrService
    {
		byte[] GenerateQr(string url);
	}
}

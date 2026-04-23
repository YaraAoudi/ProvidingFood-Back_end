using ProvidingFood2.DTO;
using ProvidingFood2.Model;

namespace ProvidingFood2.Service
{
	public interface IUserService
	{
		Task<RegisterResult> RegisterAsync(UserDto request);
		Task<LoginResult> LoginAsync(Login login);
	}
}

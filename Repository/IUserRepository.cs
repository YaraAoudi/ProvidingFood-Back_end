using Microsoft.AspNetCore.Mvc;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;

namespace ProvidingFood2.Repository
{
	public interface IUserRepository
	{
		Task<int?> GetUserTypeIdAsync(string typeName);
		Task<int> InsertUserAsync(User user, int userTypeId);
		Task<bool> InsertAdminAsync(int userId, string position);
		Task<UserWithType?> GetUserByEmailAsync(string email);
		Task<bool> ExistsInTableAsync(string tableName, int userId);


	}
}

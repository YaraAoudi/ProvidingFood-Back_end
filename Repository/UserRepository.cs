using Dapper;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using System.Data.SqlClient;

public class UserRepository : IUserRepository
{
	private readonly string _connectionString;

	public UserRepository(IConfiguration configuration)
	{
		_connectionString = configuration.GetConnectionString("DefaultConnection");
	}

	public async Task<int?> GetUserTypeIdAsync(string typeName)
	{
		using var connection = new SqlConnection(_connectionString);
		return await connection.ExecuteScalarAsync<int?>(
			"SELECT UserTypeId FROM UserType WHERE TypeName = @TypeName",
			new { TypeName = typeName });
	}

	public async Task<int> InsertUserAsync(User user, int userTypeId)
	{
		using var connection = new SqlConnection(_connectionString);

		var query = @"
INSERT INTO [User]
(FullName, Email, Password, PhoneNumber, UserTypeId)
VALUES
(@FullName, @Email, @Password, @PhoneNumber, @UserTypeId);
SELECT CAST(SCOPE_IDENTITY() as int);";

		return await connection.ExecuteScalarAsync<int>(query, new
		{
			user.FullName,
			user.Email,
			user.Password,
			user.PhoneNumber,
			UserTypeId = userTypeId
		});
	}

	public async Task<bool> InsertAdminAsync(int userId, string position)
	{
		using var connection = new SqlConnection(_connectionString);

		var rows = await connection.ExecuteAsync(@"
INSERT INTO [Admin]
(UserId, Position, CreatedAt)
VALUES
(@UserId, @Position, @CreatedAt);",
			new
			{
				UserId = userId,
				Position = position,
				CreatedAt = DateTime.UtcNow
			});

		return rows > 0;
	}

	public async Task<UserWithType?> GetUserByEmailAsync(string email)
	{
		using var connection = new SqlConnection(_connectionString);

		return await connection.QueryFirstOrDefaultAsync<UserWithType>(@"
SELECT u.UserId, u.Password, u.FullName, u.UserTypeId,u.Email, ut.TypeName
FROM [User] u
JOIN UserType ut ON u.UserTypeId = ut.UserTypeId
WHERE u.Email = @Email",
		new { Email = email });
	}

	public async Task<bool> ExistsInTableAsync(string tableName, int userId)
	{
		using var connection = new SqlConnection(_connectionString);

		var result = await connection.QueryFirstOrDefaultAsync(
			$"SELECT 1 FROM [{tableName}] WHERE UserId = @UserId",
			new { UserId = userId });

		return result != null;
	}
}
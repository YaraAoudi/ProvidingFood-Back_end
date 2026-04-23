using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;
using BCrypt.Net;

namespace ProvidingFood2.Repository
{
	public class RestaurantRepository:IRestaurantRepository
	{
		private readonly string _connectionString;

		public RestaurantRepository (IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection");
		}
		
		/////////////////function to return all reasturant in Restaurant Table///////////////////////////
		public async Task<IEnumerable<RestaurantGetInfo>> GetRestaurantAsync()
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();
				string query = @"SELECT r.RestaurantId,
                r.UserId,
				r.RestaurantName,
                r.RestaurantEmail,
				r.Address,
				r.RestaurantPhone,
				c.Name AS CategoryName 
                FROM 
				Restaurant r
			INNER JOIN 
				Category c ON r.CategoryId = c.CategoryId";
				return await connection.QueryAsync<RestaurantGetInfo>(query);
			}
		}

		///////////////////function to Add all User in User Table/////////////////////////////
		private async Task<int> AddBaseUserAsync(User user, string userTypeName)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				// جلب UserTypeId بناءً على اسم النوع
				string getUserTypeIdQuery = "SELECT UserTypeId FROM UserType WHERE TypeName = @UserTypeName";
				var userTypeId = await connection.ExecuteScalarAsync<int?>(getUserTypeIdQuery, new { UserTypeName = userTypeName });

				if (userTypeId == null)
				{
					throw new ArgumentException("UserTypeName غير موجود في جدول UserType");
				}

				// تشفير كلمة المرور
				string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

				// إدخال المستخدم
				string query = @"
			INSERT INTO [User] 
				(fullName, email, password, phoneNumber, UserTypeId)
			VALUES 
				(@FullName, @Email, @Password, @PhoneNumber, @UserTypeId);
			SELECT CAST(SCOPE_IDENTITY() as int);";

				var parameters = new
				{
					user.FullName,
					user.Email,
					Password = hashedPassword,
					user.PhoneNumber,
					UserTypeId = userTypeId
				};

				return await connection.ExecuteScalarAsync<int>(query, parameters);
			}
		}

		///////////////////function to Add all restaurant in Restaurant Table/////////////////////////////
		public async Task<bool> AddRestaurantUserAsync(User user, Restaurant restaurant, string userTypeName, string categoryName)
		{
			// جلب CategoryId من CategoryName
			int? categoryId;

			await using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string getCategoryIdQuery = "SELECT CategoryId FROM Category WHERE Name = @CategoryName";
				categoryId = await connection.ExecuteScalarAsync<int?>(getCategoryIdQuery, new { CategoryName = categoryName });

				if (categoryId == null)
				{
					throw new ArgumentException("اسم التصنيف غير موجود في جدول Category");
				}
			}

			// إضافة المستخدم بناءً على UserTypeName
			int userId = await AddBaseUserAsync(user, userTypeName);

			await using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string insertRestaurantQuery = @"
			INSERT INTO [Restaurant] 
				(UserId, RestaurantName, RestaurantEmail, RestaurantPhone, Address, CategoryId)
			VALUES 
				(@UserId, @RestaurantName, @RestaurantEmail, @RestaurantPhone, @Address, @CategoryId);";

				var parameters = new
				{
					UserId = userId,
					RestaurantName = restaurant.RestaurantName,
					RestaurantEmail = restaurant.RestaurantEmail,
					RestaurantPhone = restaurant.RestaurantPhone,
					Address = restaurant.RestaurantAddress,
					CategoryId = categoryId
				};

				int rows = await connection.ExecuteAsync(insertRestaurantQuery, parameters);
				return rows > 0;
			}
		}

	


		///////////////////function Delet restaurant from Restaurant Table/////////////////////////////
		public async Task<bool> DeleteRestaurantUserAsync(int userId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				
				using (var transaction = connection.BeginTransaction())
				{
					try
					{
						
						string deleteRestaurantQuery = @"DELETE FROM [Restaurant] 
                                              WHERE UserId = @UserId";

						int restaurantRows = await connection.ExecuteAsync(
							deleteRestaurantQuery,
							new { UserId = userId },
							transaction);

						
						string deleteUserQuery = @"DELETE FROM [User] 
                                         WHERE UserId = @UserId";

						int userRows = await connection.ExecuteAsync(
							deleteUserQuery,
							new { UserId = userId },
							transaction);

						
						transaction.Commit();

						
						return (restaurantRows > 0 || userRows > 0);
					}
					catch
					{
						
						transaction.Rollback();
						throw;
					}
				}
			}
		}

		///////////////////function for Update field in Restaurant and UserTable/////////////////////////////

		private string KeepOldIfEmpty(string newValue, string oldValue)
		{
			return string.IsNullOrWhiteSpace(newValue) ? oldValue : newValue;
		}

		public async Task<bool> UpdateRestaurantUserAsync(User newUser, Restaurant newRestaurant)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				using (var transaction = connection.BeginTransaction())
				{
					try
					{
					
						var existingUser = await connection.QuerySingleOrDefaultAsync<User>(
							"SELECT * FROM [User] WHERE UserId = @UserId",
							new { UserId = newUser.UserId }, transaction);

						var existingRestaurant = await connection.QuerySingleOrDefaultAsync<Restaurant>(
							"SELECT * FROM [Restaurant] WHERE UserId = @UserId",
							new { UserId = newUser.UserId }, transaction);

						if (existingUser == null || existingRestaurant == null)
							return false;

						
						newUser.FullName = KeepOldIfEmpty(newUser.FullName, existingUser.FullName);
						newUser.Email = KeepOldIfEmpty(newUser.Email, existingUser.Email);
						newUser.PhoneNumber = KeepOldIfEmpty(newUser.PhoneNumber, existingUser.PhoneNumber);
						newUser.Password = KeepOldIfEmpty(newUser.Password, existingUser.Password);

						newRestaurant.RestaurantName = KeepOldIfEmpty(newRestaurant.RestaurantName, existingRestaurant.RestaurantName);
						newRestaurant.RestaurantEmail = KeepOldIfEmpty(newRestaurant.RestaurantEmail, existingRestaurant.RestaurantEmail);
						newRestaurant.RestaurantPhone = KeepOldIfEmpty(newRestaurant.RestaurantPhone, existingRestaurant.RestaurantPhone);
						newRestaurant.RestaurantAddress = KeepOldIfEmpty(newRestaurant.RestaurantAddress, existingRestaurant.RestaurantAddress);

						
						string updateUserQuery = @"UPDATE [User] SET 
						FullName = @FullName, 
						Email = @Email, 
						PhoneNumber = @PhoneNumber, 
						Password = @Password
					WHERE UserId = @UserId";

						int userRows = await connection.ExecuteAsync(updateUserQuery, newUser, transaction);

						string updateRestaurantQuery = @"UPDATE [Restaurant] SET 
						RestaurantName = @RestaurantName, 
						RestaurantEmail = @RestaurantEmail, 
						RestaurantPhone = @RestaurantPhone, 
						Address = @RestaurantAddress
					WHERE UserId = @UserId";

						newRestaurant.UserId = newUser.UserId;
						int restRows = await connection.ExecuteAsync(updateRestaurantQuery, newRestaurant, transaction);

						transaction.Commit();
						return userRows > 0 || restRows > 0;
					}
					catch
					{
						transaction.Rollback();
						throw;

					}
				}
			}
		}


	}
}

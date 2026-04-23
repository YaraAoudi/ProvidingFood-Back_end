using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{


    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task CreateAsync(int userId, string title, string message)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
            INSERT INTO Notifications (UserId, Title, Message)
            VALUES (@UserId, @Title, @Message)
        ", new { UserId = userId, Title = title, Message = message });
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Notification>(@"
            SELECT * FROM Notifications
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC
        ", new { UserId = userId });
        }

        public async Task MarkAsReadAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
            UPDATE Notifications
            SET IsRead = 1
            WHERE Id = @Id
        ", new { Id = id });
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*)
            FROM Notifications
            WHERE UserId = @UserId AND IsRead = 0
        ", new { UserId = userId });
        }

    }
}

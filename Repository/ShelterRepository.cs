using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
    public class ShelterRepository : IShelterRepository
    {
        private readonly string _connectionString;

        public ShelterRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // 🏠 إنشاء طلب توثيق
        public async Task<int> CreateAsync(Shelter shelter)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(@"
                INSERT INTO Shelters
                (Name, Description, ProofImageUrl, Status , UserId)
                VALUES
                (@Name, @Description, @ProofImageUrl, @Status , @UserId);

                SELECT CAST(SCOPE_IDENTITY() as int);
            ", shelter);
        }

        // 🛡️ جلب الطلبات المعلقة
        public async Task<IEnumerable<Shelter>> GetPendingAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Shelter>(@"
                SELECT * FROM Shelters
                WHERE Status = 'Pending'
                ORDER BY CreatedAt DESC
            ");
        }

        public async Task<IEnumerable<Shelter>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Shelter>(@"
                SELECT * FROM Shelters
                ORDER BY CreatedAt DESC
            ");
        }

        // 🔍 جلب حسب ID
        public async Task<Shelter> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Shelter>(@"
                SELECT * FROM Shelters
                WHERE Id = @Id
            ", new { Id = id });
        }

        // ✅ موافقة
        public async Task ApproveAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
UPDATE Shelters
SET Status = 'Approved',
    IsVerified = 1
WHERE Id = @Id
", new { Id = id });
        }

        // ❌ رفض
        public async Task RejectAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
UPDATE Shelters
SET Status = 'Rejected',
    IsVerified = 0
WHERE Id = @Id
", new { Id = id });
        }

    }
}
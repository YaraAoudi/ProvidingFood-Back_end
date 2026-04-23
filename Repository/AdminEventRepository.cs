using Dapper;
using ProvidingFood2.Model;
using System.Data.SqlClient;

namespace ProvidingFood2.Repository
{
    public class AdminEventRepository :IAdminEventRepository
    {
        private readonly string _conn;

        public AdminEventRepository(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateEventAsync(Event ev)
        {
            using var db = new SqlConnection(_conn);

            return await db.ExecuteScalarAsync<int>(@"
            INSERT INTO Events (Name, Description, StartDate, EndDate, IsActive)
            VALUES (@Name, @Description, @StartDate, @EndDate, 1);
            SELECT CAST(SCOPE_IDENTITY() as int);", ev);
        }

        public async Task AddItemAsync(int eventId, EventItem item)
        {
            using var db = new SqlConnection(_conn);

            await db.ExecuteAsync(@"
            INSERT INTO EventItems (EventId, Name, Price)
            VALUES (@EventId, @Name, @Price);",
                new { EventId = eventId, item.Name, item.Price });
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            using var db = new SqlConnection(_conn);

            return await db.QueryAsync<Event>("SELECT * FROM Events ORDER BY Id DESC");
        }

        public async Task<Event> GetByIdAsync(int id)
        {
            using var db = new SqlConnection(_conn);

            return await db.QueryFirstOrDefaultAsync<Event>(
                "SELECT * FROM Events WHERE Id = @Id", new { Id = id });
        }

        public async Task UpdateAsync(int id, Event ev)
        {
            using var db = new SqlConnection(_conn);

            await db.ExecuteAsync(@"
            UPDATE Events
            SET Name = @Name,
                Description = @Description,
                StartDate = @StartDate,
                EndDate = @EndDate,
                IsActive = @IsActive
            WHERE Id = @Id",
                new
                {
                    ev.Name,
                    ev.Description,
                    ev.StartDate,
                    ev.EndDate,
                    ev.IsActive,
                    Id = id
                });
        }

        public async Task DeleteAsync(int id)
        {
            using var db = new SqlConnection(_conn);

            await db.ExecuteAsync("DELETE FROM Events WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<EventItem>> GetItemsAsync(int eventId)
        {
            using var db = new SqlConnection(_conn);

            return await db.QueryAsync<EventItem>(
                "SELECT * FROM EventItems WHERE EventId = @eventId",
                new { eventId });
        }

        public async Task<EventItem> GetItemByIdAsync(int itemId)
        {
            using var connection = new SqlConnection(_conn);

            return await connection.QueryFirstOrDefaultAsync<EventItem>(@"
        SELECT * FROM EventItems
        WHERE Id = @Id",
                new { Id = itemId });
        }

        public async Task UpdateItemAsync(int itemId, EventItem item)
        {
            using var connection = new SqlConnection(_conn);

            await connection.ExecuteAsync(@"
        UPDATE EventItems
        SET Name = @Name,
            Price = @Price
        WHERE Id = @Id",
                new
                {
                    Id = itemId,
                    item.Name,
                    item.Price
                });
        }

        public async Task DeleteItemAsync(int itemId)
        {
            using var connection = new SqlConnection(_conn);

            await connection.ExecuteAsync(@"
        DELETE FROM EventItems
        WHERE Id = @Id",
                new { Id = itemId });
        }
        public async Task<IEnumerable<EventItem>> GetItemsByEventIdAsync(int eventId)
        {
            using var connection = new SqlConnection(_conn);

            return await connection.QueryAsync<EventItem>(@"
        SELECT * FROM EventItems
        WHERE EventId = @EventId",
                new { EventId = eventId });
        }
        public async Task ActivateAsync(int id)
        {
            using var connection = new SqlConnection(_conn);

            await connection.ExecuteAsync(@"
        UPDATE Events
        SET IsActive = 1
        WHERE Id = @Id",
                new { Id = id });
        }
        public async Task DeactivateAsync(int id)
        {
            using var connection = new SqlConnection(_conn);

            await connection.ExecuteAsync(@"
        UPDATE Events
        SET IsActive = 0
        WHERE Id = @Id",
                new { Id = id });
        }
    }
}

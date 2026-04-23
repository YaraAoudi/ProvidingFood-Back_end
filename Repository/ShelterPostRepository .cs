using Dapper;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using System.Data.SqlClient;

public class ShelterPostRepository : IShelterPostRepository
{
    private readonly string _connectionString;

    public ShelterPostRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public async Task<int> CreateAsync(ShelterPost post)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(@"
INSERT INTO ShelterPosts
(ShelterId, Title, Description, RequiredMeals, CollectedMeals, DisplayImageUrl, Status)
VALUES
(@ShelterId, @Title, @Description, @RequiredMeals, 0, @DisplayImageUrl, 'Active');

SELECT CAST(SCOPE_IDENTITY() as int);
", post);
    }

    public async Task UpdateAsync(ShelterPost post)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(@"
UPDATE ShelterPosts
SET Title = @Title,
    Description = @Description,
    RequiredMeals = @RequiredMeals,
    CollectedMeals = @CollectedMeals,
    DisplayImageUrl = @DisplayImageUrl,
    Status = @Status
WHERE Id = @Id
", post);
    }

    public async Task<ShelterPost> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<ShelterPost>(@"
SELECT * FROM ShelterPosts WHERE Id = @Id
", new { Id = id });
    }

    public async Task<IEnumerable<ShelterPost>> GetAllAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<ShelterPost>(@"
SELECT * FROM ShelterPosts ORDER BY CreatedAt DESC
");
    }
    public async Task<Shelter> GetShelterByUserId(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Shelter>(@"
        SELECT *
        FROM Shelters
        WHERE UserId = @UserId
    ", new { UserId = userId });
    }


    public async Task<IEnumerable<ShelterPostWithDonationsDto>> GetPostsWithDonations()
    {
        using var connection = new SqlConnection(_connectionString);

        var query = @"
SELECT 
    p.Id,
    p.Title,
    p.Description,
    p.RequiredMeals,
    p.CollectedMeals,
    p.Status,
    p.DisplayImageUrl,

    s.Name AS ShelterName, -- 🏠 اسم الملجأ

    r.RestaurantName AS RestaurantName,
    d.MealsCount

FROM ShelterPosts p
LEFT JOIN Shelters s ON p.ShelterId = s.Id
LEFT JOIN ShelterPostDonations d ON p.Id = d.PostId
LEFT JOIN Restaurant r ON d.RestaurantId = r.RestaurantId
ORDER BY p.Id DESC
";

        var result = await connection.QueryAsync(query);

        // 🔥 grouping
        var posts = result
     .GroupBy(p => p.Id)
     .Select(g => new ShelterPostWithDonationsDto
     {
         Id = g.Key,
         Title = g.First().Title,
         Description = g.First().Description,
         RequiredMeals = g.First().RequiredMeals,
         CollectedMeals = g.First().CollectedMeals,
         Status = g.First().Status,
         DisplayImageUrl = g.First().DisplayImageUrl,

         ShelterName = g.First().ShelterName, // 🏠

         Donations = g
             .Where(x => x.RestaurantName != null)
             .Select(x => new DonationDto
             {
                 RestaurantName = x.RestaurantName,
                 MealsCount = x.MealsCount
             }).ToList()
     });

        return posts;
    }

    public async Task AddDonationAsync(int postId, int restaurantId, int meals)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(@"
        INSERT INTO ShelterPostDonations (PostId, RestaurantId, MealsCount)
        VALUES (@PostId, @RestaurantId, @MealsCount)
    ", new
        {
            PostId = postId,
            RestaurantId = restaurantId,
            MealsCount = meals
        });
    }
    public async Task<int?> GetRestaurantIdByUserId(int userId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int?>(@"
        SELECT RestaurantId
        FROM Restaurant
        WHERE UserId = @UserId
    ", new { UserId = userId });
    }

    public async Task<int> GetShelterOwnerUserIdByPostId(int postId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<int>(@"
        SELECT s.UserId
        FROM ShelterPosts p
        JOIN Shelters s ON p.ShelterId = s.Id
        WHERE p.Id = @PostId
    ", new { PostId = postId });
    }

    public async Task<string> GetRestaurantNameById(int restaurantId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.ExecuteScalarAsync<string>(@"
        SELECT RestaurantName
        FROM Restaurant
        WHERE RestaurantId = @Id
    ", new { Id = restaurantId });
    }
}
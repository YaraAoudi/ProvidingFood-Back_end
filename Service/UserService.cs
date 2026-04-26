using Microsoft.IdentityModel.Tokens;
using ProvidingFood2.DTO;
using ProvidingFood2.Model;
using ProvidingFood2.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProvidingFood2.Service
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _repository;
		private readonly IConfiguration _configuration;

		public UserService(IUserRepository repository, IConfiguration configuration)
		{
			_repository = repository;
			_configuration = configuration;
		}

		// =========================
		// Register
		// =========================
		public async Task<RegisterResult> RegisterAsync(UserDto request)
		{
            var allowedTypes = new List<string>
{
    "Beneficiary",
    "Shelter Owner",
    "Store Owner",
    "Donor"
};

            var role = request.Role?.Trim();

            if (string.IsNullOrEmpty(role) ||
                !allowedTypes.Any(x => x.Equals(role, StringComparison.OrdinalIgnoreCase)))
            {
                return new RegisterResult
                {
                    Success = false,
                    Message = "نوع المستخدم غير صالح"
                };
            }

            var userTypeId = await _repository.GetUserTypeIdAsync(role);

            if (userTypeId == null)
			{
				return new RegisterResult
				{
					Success = false,
					Message = "نوع المستخدم غير موجود"
				};
			}

			string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

			var user = new User
			{
				FullName = request.FullName,
				Email = request.Email,
				Password = hashedPassword,
				PhoneNumber = request.PhoneNumber
			};

			int userId = await _repository.InsertUserAsync(user, userTypeId.Value);

			return new RegisterResult
			{
				Success = true,
				UserId = userId,
				Message = $"تم إنشاء حساب {request.Role} بنجاح"
			};
		}

        // =========================
        // Login
        // =========================
        public async Task<LoginResult> LoginAsync(Login login)
        {
            var user = await _repository.GetUserByEmailAsync(login.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
                };
            }

            string role = await DetermineRole(user);

            string token = GenerateJwtToken(
                user.UserId,
                user.FullName,
                user.Email,        // 👈 إضافة الإيميل
                user.UserTypeId,
                role
            );

            return new LoginResult
            {
                Success = true,
                UserId = user.UserId,
                FullName = user.FullName,
                UserTypeId = user.UserTypeId,
                Token = token,
                Message = $"تم تسجيل الدخول بنجاح كـ {role}"
            };
        }

        private async Task<string> DetermineRole(UserWithType user)
		{
			if (await _repository.ExistsInTableAsync("Admin", user.UserId))
				return "Admin";

			if (await _repository.ExistsInTableAsync("Restaurant", user.UserId))
				return "Restaurant";

			if (await _repository.ExistsInTableAsync("Shelter", user.UserId))
				return "Shelter Owner";

			if (await _repository.ExistsInTableAsync("Store", user.UserId))
				return "Store Owner";

			if (await _repository.ExistsInTableAsync("Beneficiaries", user.UserId))
				return "Beneficiary";

			return user.TypeName;
		}

		// =========================
		// JWT Generation (sub claim)
		// =========================
		private string GenerateJwtToken(int userId, string fullName,string email, int userTypeId, string role)
		{
			var jwtSettings = _configuration.GetSection("JwtSettings");
			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])
			);

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
			new Claim("fullName", fullName),
            new Claim(ClaimTypes.Email, email),
            new Claim("userTypeId", userTypeId.ToString()),
			new Claim(ClaimTypes.Role, role),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

			var token = new JwtSecurityToken(
				issuer: jwtSettings["Issuer"],
				audience: jwtSettings["Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddHours(2),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}

using ProvidingFood2.Repository;
using ProvidingFood2.Service;
using ProvidingFood2.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";


builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:8080") // 👈 لازم تحدد المصدر
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // 👈 مهم جداً
    });
});


// 🟢 Controllers
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();


// 🟢 Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])
        ),

        // ⭐ مهم جداً
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };

    // ⭐⭐ أهم سطر ناقص عندك
    options.MapInboundClaims = true;

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/notificationHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();


// 🟢 Repositories + Services

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();
builder.Services.AddScoped<IDonationRestaurantRepository, DonationRestaurantRepository>();

builder.Services.AddScoped<IBeneficiaryRepository>(provider =>
    new BeneficiaryRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();

builder.Services.AddScoped<IDonationIndividalRepository>(provider =>
    new DonationIndividalRepository(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFoodBondRepository>(provider =>
    new FoodBondRepository(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        provider.GetRequiredService<INotificationService>() 
    )
);
builder.Services.AddHostedService<BondStatusBackgroundService>();

builder.Services.AddScoped<IStoreRepository, StoreRepository>();
builder.Services.AddScoped<IStoreService, StoreService>();

builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IVoucherService, VoucherService>();

builder.Services.AddScoped<IQrService, QrService>();
builder.Services.AddScoped<ICashDonationRepository, CashDonationRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<IAdminDonationRepository, AdminDonationRepository>();
builder.Services.AddScoped<IAdminDonationService, AdminDonationService>();

builder.Services.AddScoped<IBondRepository, BondRepository>();
builder.Services.AddScoped<IBondDonationRepository, BondDonationRepository>();
builder.Services.AddScoped<IBondService, BondService>();

builder.Services.AddScoped<IGiftDonationRepository, GiftDonationRepository>();
builder.Services.AddScoped<IGiftService, GiftService>();

builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<ChallengeDisplayService>();

builder.Services.AddScoped<RestaurantPaymentService>();

builder.Services.AddScoped<AdminEventService>();
builder.Services.AddScoped<EventDonationService>();
builder.Services.AddScoped<IAdminEventRepository, AdminEventRepository>();
builder.Services.AddScoped<IEventDonationRepository, EventDonationRepository>();


// 🟢 Shelter
builder.Services.AddScoped<IShelterRepository, ShelterRepository>();
builder.Services.AddScoped<IShelterService, ShelterService>();

builder.Services.AddScoped<IShelterPostRepository, ShelterPostRepository>();
builder.Services.AddScoped<ShelterPostService>();
// هذا غير موجود عندك
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();


// 🟢 SignalR
builder.Services.AddSignalR();


var app = builder.Build();


// 🟢 Middleware pipeline (IMPORTANT ORDER)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();


app.UseCors(MyAllowSpecificOrigins);

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 🟢 SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
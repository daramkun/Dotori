using Dotori.Registry.Auth;
using Dotori.Registry.Database;
using Dotori.Registry.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB — provider는 Database:Provider 설정으로 선택 (기본: sqlite)
var dbProvider = builder.Configuration["Database:Provider"]?.ToLowerInvariant() ?? "sqlite";
var connStr = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<RegistryDbContext>(opt =>
{
    switch (dbProvider)
    {
        case "postgres":
        case "postgresql":
            opt.UseNpgsql(connStr ?? "Host=localhost;Database=dotori;Username=dotori;Password=dotori");
            break;
        case "mysql":
        case "mariadb":
            opt.UseMySql(connStr ?? "Server=localhost;Database=dotori;User=dotori;Password=dotori;",
                ServerVersion.AutoDetect(connStr ?? "Server=localhost;Database=dotori;User=dotori;Password=dotori;"));
            break;
        case "oracle":
            opt.UseOracle(connStr ?? "User Id=dotori;Password=dotori;Data Source=localhost/XEPDB1");
            break;
        default: // sqlite
            opt.UseSqlite(connStr ?? "Data Source=registry.db");
            break;
    }
});

// Storage (filesystem default; S3 when S3_BUCKET env var is set)
if (Environment.GetEnvironmentVariable("S3_BUCKET") is not null)
    builder.Services.AddSingleton<IPackageStorage, S3Storage>();
else
    builder.Services.AddSingleton<IPackageStorage, FileSystemStorage>();

// Proxy upstream HttpClient (used when Registry:Mode = "proxy")
builder.Services.AddHttpClient("proxy");

// Auth
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<IOAuthProvider, GitHubOAuthProvider>();
builder.Services.AddSingleton<OAuthProviderRegistry>();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<TokenService>((opt, tokenService) =>
    {
        opt.TokenValidationParameters = tokenService.GetValidationParameters();
    });
builder.Services.AddAuthorization();

// API
builder.Services.AddControllers();

var app = builder.Build();

// DB 자동 마이그레이션
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RegistryDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

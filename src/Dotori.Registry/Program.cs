using Dotori.Registry.Auth;
using Dotori.Registry.Database;
using Dotori.Registry.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<RegistryDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=registry.db"));

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

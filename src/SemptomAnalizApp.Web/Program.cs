using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Core.Interfaces;
using SemptomAnalizApp.Data;
using SemptomAnalizApp.Service.Interfaces;
using SemptomAnalizApp.Service.Options;
using SemptomAnalizApp.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Rate limiting: analiz endpoint kötüye kullanımını önler
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("analiz", context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit         = 10,
                Window              = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                QueueLimit          = 0
            }));
    options.RejectionStatusCode = 429;
});

// Railway PostgreSQL plugin DATABASE_URL'sini Npgsql formatına çevir
string conn;
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var configConn  = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(configConn))
{
    conn = configConn;
}
else if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    conn = BuildNpgsqlConnectionString(databaseUrl);
}
else
{
    throw new InvalidOperationException(
        "Veritabanı bağlantısı bulunamadı. " +
        "Lütfen 'ConnectionStrings__DefaultConnection' veya 'DATABASE_URL' environment variable'ını ayarlayın.");
}

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(conn));
builder.Services.AddScoped<IAnalizDbContext>(sp => sp.GetRequiredService<AppDbContext>());

builder.Services.AddIdentity<Kullanici, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 8;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireNonAlphanumeric = true;
    opt.Password.RequireUppercase = true;
    opt.SignIn.RequireConfirmedEmail = false;
    opt.Lockout.AllowedForNewUsers = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    opt.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Hesap/Giris";
    opt.AccessDeniedPath = "/Hesap/Giris";
    opt.Cookie.Name = "SemptomAnaliz.Auth";
    opt.ExpireTimeSpan = TimeSpan.FromDays(14);
    opt.SlidingExpiration = true;
});

builder.Services.AddScoped<IAnalizService, AnalizMotoru>();
builder.Services.AddSingleton<IBmiService, BmiService>();
builder.Services.AddSingleton<ISemptomImzaService, SemptomImzaService>();
builder.Services.AddSingleton<IAciliyetService, AciliyetService>();
builder.Services.AddScoped<IBayesianAnalizService, BayesianAnalizService>();
builder.Services.AddScoped<ITekrarAnalizService, TekrarAnalizService>();
builder.Services.AddSingleton<IAnalizMetinService, AnalizMetinService>();
builder.Services.AddSingleton<IGunlukOneriService, GunlukOneriService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.Configure<KritikSemptomOptions>(
    builder.Configuration.GetSection("KritikSemptomlar"));
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// Migrations + Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var um = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var adminPwd = app.Configuration["Seed:AdminPassword"];
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, um, rm, adminPwd, seedDemoUsers: app.Environment.IsDevelopment());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Güvenlik başlıkları — clickjacking, MIME sniff, referrer sızıntısı önler
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Frame-Options", "DENY");
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    ctx.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
    ctx.Response.Headers.Append("Permissions-Policy",
        "camera=(), microphone=(), geolocation=(), payment=()");
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string BuildNpgsqlConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2)
        throw new InvalidOperationException("DATABASE_URL kullanici adi ve parola icermelidir.");

    var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
    if (string.IsNullOrWhiteSpace(database))
        throw new InvalidOperationException("DATABASE_URL veritabani adini icermelidir.");

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = database,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require,
        TrustServerCertificate = true,
        Pooling = true
    };

    return builder.ConnectionString;
}

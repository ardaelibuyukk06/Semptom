using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SemptomAnalizApp.Core.Entities;
using SemptomAnalizApp.Data;
using SemptomAnalizApp.Service.Interfaces;
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
    // postgresql://user:pass@host:port/dbname → Npgsql formatına çevir
    var uri  = new Uri(databaseUrl);
    var user = uri.UserInfo.Split(':');
    conn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={user[0]};Password={user[1]};SSL Mode=Require;Trust Server Certificate=true;";
}
else
{
    // Localhost çalışması için varsayılan olarak SQLite kullan
    conn = "Data Source=SemptomAnalizLocal.db";
}

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(conn));

builder.Services.AddIdentity<Kullanici, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.SignIn.RequireConfirmedAccount = false;
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
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migrations + Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var um = scope.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var adminPwd = app.Configuration["Seed:AdminPassword"];
    await DbSeeder.SeedAsync(db, um, rm, adminPwd);
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

app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

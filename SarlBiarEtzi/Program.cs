using Microsoft.EntityFrameworkCore;
using SarlBiarEtzi.Hubs;
using SarlBiarEtzi.Models;
using SarlBiarEtzi.Services;

var builder = WebApplication.CreateBuilder(args);

//
// ================= SERVICES =================
//
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddSignalR();
builder.Services.AddScoped<GroqService>();

//
// ================= DATABASE =================
//
string connectionString;

try
{
    string? rawUrl =
        Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    connectionString = ParseDatabaseUrl(rawUrl);
}
catch (Exception ex)
{
    Console.WriteLine("DB connection parsing error: " + ex.Message);

    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("No database connection string found");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

//
// ================= BUILD APP =================
//
var app = builder.Build();

//
// ================= AUTO MIGRATIONS (SAFE) =================
//
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    Console.WriteLine("Migration error: " + ex.Message);
}

//
// ================= ERROR HANDLING =================
//
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//
// ================= MIDDLEWARE ORDER (FIXED) =================
//
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

//
// ================= SIGNALR HUBS =================
//
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chatHub");

//
// ================= ROUTES =================
//
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//
// ================= RUN =================
//
app.Run();

//
// ================= HELPER =================
//
string ParseDatabaseUrl(string? url)
{
    if (string.IsNullOrEmpty(url))
    {
        throw new Exception("DATABASE_URL is missing");
    }

    // If already normal connection string
    if (!url.StartsWith("postgresql://"))
        return url;

    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');

    var username = userInfo[0];
    var password = userInfo[1];

    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={username};Password={password};Ssl Mode=Require;Trust Server Certificate=true";
}

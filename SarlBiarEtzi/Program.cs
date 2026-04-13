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
string? rawUrl =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString = ParseDatabaseUrl(rawUrl);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

//
// ================= APP =================
//
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//
// ================= HUBS =================
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
// ================= PORT =================
//
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();

//
// ================= HELPER =================
//
string ParseDatabaseUrl(string? url)
{
    if (string.IsNullOrEmpty(url))
        throw new Exception("DATABASE URL NOT FOUND");

    if (!url.StartsWith("postgresql://"))
        return url;

    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');

    var username = userInfo[0];
    var password = userInfo[1];

    return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={username};Password={password};Ssl Mode=Require;Trust Server Certificate=true";
}

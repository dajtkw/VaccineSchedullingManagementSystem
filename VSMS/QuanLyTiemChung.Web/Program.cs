using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyTiemChung.Web.Data;
using QuanLyTiemChung.Web.Interfaces;
using QuanLyTiemChung.Web.Models;
using QuanLyTiemChung.Web.Repositories;
using QuanLyTiemChung.Web.Hubs;
using QuanLyTiemChung.Web.Services;
using QuanLyTiemChung;
using QuanLyTiemChung.Web;
using System.Security.Claims; // 🔥 THÊM DÒNG NÀY

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// Cấu hình ASP.NET Core Identity
builder.Services.AddIdentity<User, Role>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 🔥 CONFIGURE SIGNALR TO USE USER ID
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Đăng ký Repositories để sử dụng với Dependency Injection
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IVaccineRepository, VaccineRepository>();
builder.Services.AddScoped<IVaccinationSiteRepository, VaccinationSiteRepository>();
builder.Services.AddScoped<IVaccinationRecordRepository, VaccinationRecordRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IVaccineCategoryRepository, VaccineCategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHostedService<AppointmentReminderService>();
builder.Services.AddHostedService<NextDoseReminderService>();


var app = builder.Build();

// Seed dữ liệu ban đầu (Roles và Admin user)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
// app.UseMiddleware<ClaimsValidationMiddleware>(); 
app.UseAuthorization();

// 🔥 MAP SIGNALR HUBS
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<AdminHub>("/adminHub"); // 🔥 THÊM DÒNG NÀY

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

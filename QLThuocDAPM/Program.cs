using Microsoft.EntityFrameworkCore;
using QLThuocDAPM.Data;
using QLThuocDAPM.Common;
using Common = QLThuocDAPM.Common.Common;
using QLThuocDAPM.Services;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using QLThuocDAPM.Services.VnPay;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<Common>();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IVnPayService, VnPayService>();


builder.Services.AddHttpClient();

// Add database context
builder.Services.AddDbContext<QlthuocDapm6Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Hshop"));
});

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Timeout after 10 minutes
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Paypal
builder.Services.AddScoped<PayPalService>();

//Facade
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();



var app = builder.Build();

// Cấu hình routing hỗ trợ Areas
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Sử dụng session trước khi dùng authentication và authorization
app.UseRouting();

app.UseAuthentication(); // Đảm bảo authentication được dùng trước khi authorization
app.UseAuthorization();

// Cấu hình endpoints cho Areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Cấu hình routing mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

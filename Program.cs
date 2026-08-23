using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Linq;
using VitraX.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ===== منع مشروع MVC من تحميل كنترولرات الـ API =====
builder.Services.AddControllersWithViews()
    .ConfigureApplicationPartManager(apm =>
    {
        var apiPart = apm.ApplicationParts.FirstOrDefault(p => p.Name == "VitraX.Api");
        if (apiPart != null)
            apm.ApplicationParts.Remove(apiPart);
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== إضافة المصادقة بالكوكيز =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
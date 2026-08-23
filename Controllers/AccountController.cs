using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VitraX.Infrastructure.Data;
using VitraX.Domain.Entities;

namespace VitraX.MVC.Controllers
{
    [AllowAnonymous] // صفحات الدخول والتسجيل مفتوحة للجميع
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context) => _context = context;

        // ================= تسجيل الدخول =================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false, string? returnUrl = null)
        {
            // ابحث عن المستخدم بالاسم
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            // تحقق من وجوده ومن تطابق كلمة المرور المشفّرة
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties { IsPersistent = rememberMe });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "اسم المستخدم أو كلمة المرور غير صحيحة";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ================= تسجيل حساب جديد =================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string? role = "User")
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "الرجاء إدخال اسم المستخدم وكلمة المرور";
                return View();
            }

            // تأكد أن الاسم غير مستخدم مسبقاً
            bool exists = await _context.Users.AnyAsync(u => u.Username == username);
            if (exists)
            {
                ViewBag.Error = "اسم المستخدم موجود مسبقاً، اختر اسماً آخر";
                return View();
            }

            // أنشئ المستخدم مع تشفير كلمة المرور
            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = string.IsNullOrWhiteSpace(role) ? "User" : role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // بعد التسجيل، وجّهه لصفحة الدخول
            TempData["Success"] = "تم إنشاء الحساب بنجاح، يمكنك تسجيل الدخول الآن";
            return RedirectToAction("Login");
        }

        // ================= تسجيل الخروج =================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}

using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitraX.MVC.Services;

namespace VitraX.MVC.Controllers
{
    [AllowAnonymous] // صفحات الدخول والتسجيل مفتوحة للجميع
    public class AccountController : Controller
    {
        private readonly IApiClient _apiClient;
        public AccountController(IApiClient apiClient) => _apiClient = apiClient;

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
            // اطلب رمز الدخول (JWT) من VitraX.Api
            var token = await _apiClient.LoginAsync(username, password);

            if (token != null)
            {
                var tokenClaims = DecodeJwtClaims(token);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, tokenClaims.GetValueOrDefault(ClaimTypes.Name, username)),
                    new Claim(ClaimTypes.Role, tokenClaims.GetValueOrDefault(ClaimTypes.Role, "User")),
                    new Claim("access_token", token) // يُرفق مع كل طلب لاحق إلى الـ API
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

            var response = await _apiClient.RegisterAsync(username, password);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "اسم المستخدم موجود مسبقاً، اختر اسماً آخر";
                return View();
            }

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

        // فك ترميز حمولة الـ JWT القادم من VitraX.Api لاستخراج الاسم والدور
        // بدون التحقق من التوقيع (غير ضروري: التوكن قادم مباشرة من استدعاء موثوق للـ API)
        private static Dictionary<string, string> DecodeJwtClaims(string jwt)
        {
            var payload = jwt.Split('.')[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                ?? new Dictionary<string, JsonElement>();

            return raw.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        }
    }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace VitraX.MVC.Services
{
    // Talks to VitraX.Api over HTTP only. VitraX.MVC never touches a database directly.
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        // The JWT issued by VitraX.Api is carried inside the MVC auth cookie as a claim.
        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;

            _http.DefaultRequestHeaders.Authorization = token is null
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return result?.Token;
        }

        public Task<HttpResponseMessage> RegisterAsync(string username, string password)
            => _http.PostAsJsonAsync("api/auth/register", new { username, password });

        public async Task<List<T>> GetAllAsync<T>(string resource)
        {
            AttachToken();
            return await _http.GetFromJsonAsync<List<T>>(resource) ?? new List<T>();
        }

        public async Task<T?> GetByIdAsync<T>(string resource, int id)
        {
            AttachToken();
            var response = await _http.GetAsync($"{resource}/{id}");
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public Task<HttpResponseMessage> CreateAsync<T>(string resource, T item)
        {
            AttachToken();
            return _http.PostAsJsonAsync(resource, item);
        }

        public Task<HttpResponseMessage> UpdateAsync<T>(string resource, int id, T item)
        {
            AttachToken();
            return _http.PutAsJsonAsync($"{resource}/{id}", item);
        }

        public Task<HttpResponseMessage> DeleteAsync(string resource, int id)
        {
            AttachToken();
            return _http.DeleteAsync($"{resource}/{id}");
        }

        private class TokenResponse
        {
            public string? Token { get; set; }
        }
    }
}

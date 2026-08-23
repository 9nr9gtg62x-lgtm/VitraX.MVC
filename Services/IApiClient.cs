namespace VitraX.MVC.Services
{
    public interface IApiClient
    {
        Task<string?> LoginAsync(string username, string password);
        Task<HttpResponseMessage> RegisterAsync(string username, string password);

        Task<List<T>> GetAllAsync<T>(string resource);
        Task<T?> GetByIdAsync<T>(string resource, int id);
        Task<HttpResponseMessage> CreateAsync<T>(string resource, T item);
        Task<HttpResponseMessage> UpdateAsync<T>(string resource, int id, T item);
        Task<HttpResponseMessage> DeleteAsync(string resource, int id);
    }
}

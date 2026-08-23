using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VitraX.Domain.Entities;
using VitraX.MVC.Services;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private const string Resource = "api/products";
        private readonly IApiClient _apiClient;
        public ProductsController(IApiClient apiClient) => _apiClient = apiClient;

        // Index
        public async Task<IActionResult> Index()
            => View(await _apiClient.GetAllAsync<Product>(Resource));

        // Details
        public async Task<IActionResult> Details(int id)
        {
            var product = await _apiClient.GetByIdAsync<Product>(Resource, id);
            if (product == null) return NotFound();
            return View(product);
        }

        // Create (GET)
        public IActionResult Create() => View();

        // Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                var response = await _apiClient.CreateAsync(Resource, product);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            }
            return View(product);
        }

        // Edit (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _apiClient.GetByIdAsync<Product>(Resource, id);
            if (product == null) return NotFound();
            return View(product);
        }

        // Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId) return NotFound();
            if (ModelState.IsValid)
            {
                var response = await _apiClient.UpdateAsync(Resource, id, product);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            }
            return View(product);
        }

        // Delete (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _apiClient.GetByIdAsync<Product>(Resource, id);
            if (product == null) return NotFound();
            return View(product);
        }

        // Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiClient.DeleteAsync(Resource, id);
            return RedirectToAction(nameof(Index));
        }
    }
}

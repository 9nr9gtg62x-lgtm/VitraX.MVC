using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using VitraX.Domain.Entities;
using VitraX.MVC.Services;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class ProductionOrdersController : Controller
    {
        private const string OrdersResource = "api/productionorders";
        private const string ProductsResource = "api/products";

        private readonly IApiClient _apiClient;
        public ProductionOrdersController(IApiClient apiClient) => _apiClient = apiClient;

        private static readonly string[] StatusOptions = { "قيد التنفيذ", "مكتمل", "متوقف" };

        private async Task LoadListsAsync(object? selectedProduct = null, object? selectedStatus = null)
        {
            var products = await _apiClient.GetAllAsync<Product>(ProductsResource);
            ViewBag.Products = new SelectList(products, "ProductId", "ProductName", selectedProduct);
            ViewBag.Statuses = new SelectList(StatusOptions, selectedStatus);
        }

        // الـ API لا يُرجع الكيانات المرتبطة (Product) ضمن أوامر الإنتاج، لذا نربطها هنا للعرض فقط
        private async Task AttachProductsAsync(IEnumerable<ProductionOrder> orders)
        {
            var products = (await _apiClient.GetAllAsync<Product>(ProductsResource))
                .ToDictionary(p => p.ProductId);

            foreach (var order in orders)
                if (products.TryGetValue(order.ProductId, out var product))
                    order.Product = product;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _apiClient.GetAllAsync<ProductionOrder>(OrdersResource);
            await AttachProductsAsync(orders);
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _apiClient.GetByIdAsync<ProductionOrder>(OrdersResource, id);
            if (order == null) return NotFound();
            await AttachProductsAsync(new[] { order });
            return View(order);
        }

        public async Task<IActionResult> Create()
        {
            await LoadListsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductionOrder order)
        {
            // 1) عالج التواريخ الفارغة (بسبب صيغة المتصفح) قبل التحقق
            if (order.StartDate == default) order.StartDate = DateTime.Today;
            if (order.EndDate == default) order.EndDate = DateTime.Today;

            // 2) شِل أخطاء التحقق الخاصة بالتاريخ + العلاقة (تُعبّأ تلقائياً)
            ModelState.Remove(nameof(order.StartDate));
            ModelState.Remove(nameof(order.EndDate));
            ModelState.Remove(nameof(order.Product));
            order.Notes ??= "";

            // 3) الآن التحقق يعمل على باقي الحقول (المنتج، الكمية، الحالة...)
            if (ModelState.IsValid)
            {
                var response = await _apiClient.CreateAsync(OrdersResource, order);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            }
            await LoadListsAsync(order.ProductId, order.Status);
            return View(order);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var order = await _apiClient.GetByIdAsync<ProductionOrder>(OrdersResource, id);
            if (order == null) return NotFound();
            await LoadListsAsync(order.ProductId, order.Status);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductionOrder order)
        {
            if (id != order.OrderId) return NotFound();

            if (order.StartDate == default) order.StartDate = DateTime.Today;
            if (order.EndDate == default) order.EndDate = DateTime.Today;

            ModelState.Remove(nameof(order.StartDate));
            ModelState.Remove(nameof(order.EndDate));
            ModelState.Remove(nameof(order.Product));
            order.Notes ??= "";

            if (ModelState.IsValid)
            {
                var response = await _apiClient.UpdateAsync(OrdersResource, id, order);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            }
            await LoadListsAsync(order.ProductId, order.Status);
            return View(order);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _apiClient.GetByIdAsync<ProductionOrder>(OrdersResource, id);
            if (order == null) return NotFound();
            await AttachProductsAsync(new[] { order });
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiClient.DeleteAsync(OrdersResource, id);
            return RedirectToAction(nameof(Index));
        }
    }
}

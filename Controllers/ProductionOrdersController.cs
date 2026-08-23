using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitraX.Infrastructure.Data;
using VitraX.Domain.Entities;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class ProductionOrdersController : Controller
    {
        private readonly AppDbContext _context;
        public ProductionOrdersController(AppDbContext context) => _context = context;

        private static readonly string[] StatusOptions = { "قيد التنفيذ", "مكتمل", "متوقف" };

        private void LoadLists(object? selectedProduct = null, object? selectedStatus = null)
        {
            ViewBag.Products = new SelectList(_context.Products, "ProductId", "ProductName", selectedProduct);
            ViewBag.Statuses = new SelectList(StatusOptions, selectedStatus);
        }

        public async Task<IActionResult> Index()
            => View(await _context.ProductionOrders.Include(o => o.Product).ToListAsync());

        public async Task<IActionResult> Details(int id)
        {
            var o = await _context.ProductionOrders.Include(x => x.Product)
                        .FirstOrDefaultAsync(x => x.OrderId == id);
            if (o == null) return NotFound();
            return View(o);
        }

        public IActionResult Create()
        {
            LoadLists();
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
                _context.ProductionOrders.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            LoadLists(order.ProductId, order.Status);
            return View(order);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var o = await _context.ProductionOrders.FindAsync(id);
            if (o == null) return NotFound();
            LoadLists(o.ProductId, o.Status);
            return View(o);
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
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            LoadLists(order.ProductId, order.Status);
            return View(order);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var o = await _context.ProductionOrders.Include(x => x.Product)
                        .FirstOrDefaultAsync(x => x.OrderId == id);
            if (o == null) return NotFound();
            return View(o);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var o = await _context.ProductionOrders.FindAsync(id);
            if (o != null)
            {
                _context.ProductionOrders.Remove(o);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

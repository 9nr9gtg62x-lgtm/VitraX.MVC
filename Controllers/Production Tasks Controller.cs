using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VitraX.Infrastructure.Data;
using VitraX.Domain.Entities;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class ProductionTasksController : Controller
    {
        private readonly AppDbContext _context;
        public ProductionTasksController(AppDbContext context) => _context = context;

        private static readonly string[] StageOptions = { "القص", "التقسية", "الفحص", "التغليف" };
        private static readonly string[] StatusOptions = { "قيد التنفيذ", "مكتمل", "متوقف" };

        private void LoadLists(object? selOrder = null, object? selWorker = null,
                               object? selStage = null, object? selStatus = null)
        {
            ViewBag.Orders = new SelectList(_context.ProductionOrders, "OrderId", "OrderId", selOrder);
            ViewBag.Workers = new SelectList(_context.Workers, "WorkerId", "WorkerName", selWorker);
            ViewBag.Stages = new SelectList(StageOptions, selStage);
            ViewBag.Statuses = new SelectList(StatusOptions, selStatus);
        }

        public async Task<IActionResult> Index()
            => View(await _context.ProductionTasks
                        .Include(t => t.ProductionOrder)
                        .Include(t => t.Worker)
                        .ToListAsync());

        public async Task<IActionResult> Details(int id)
        {
            var t = await _context.ProductionTasks
                        .Include(x => x.ProductionOrder)
                        .Include(x => x.Worker)
                        .FirstOrDefaultAsync(x => x.TaskId == id);
            if (t == null) return NotFound();
            return View(t);
        }

        public IActionResult Create()
        {
            LoadLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductionTask task)
        {
            // 1) عالج الأوقات الفارغة قبل التحقق
            if (task.StartTime == default) task.StartTime = DateTime.Now;
            if (task.EndTime == default) task.EndTime = DateTime.Now;

            // 2) شِل أخطاء التحقق الخاصة بالوقت + العلاقات (تُعبّأ تلقائياً)
            ModelState.Remove(nameof(task.StartTime));
            ModelState.Remove(nameof(task.EndTime));
            ModelState.Remove(nameof(task.ProductionOrder));
            ModelState.Remove(nameof(task.Worker));

            // 3) التحقق يعمل على باقي الحقول (أمر الإنتاج، العامل، المرحلة...)
            if (ModelState.IsValid)
            {
                _context.ProductionTasks.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            LoadLists(task.OrderId, task.WorkerId, task.Stage, task.Status);
            return View(task);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var t = await _context.ProductionTasks.FindAsync(id);
            if (t == null) return NotFound();
            LoadLists(t.OrderId, t.WorkerId, t.Stage, t.Status);
            return View(t);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductionTask task)
        {
            if (id != task.TaskId) return NotFound();

            if (task.StartTime == default) task.StartTime = DateTime.Now;
            if (task.EndTime == default) task.EndTime = DateTime.Now;

            ModelState.Remove(nameof(task.StartTime));
            ModelState.Remove(nameof(task.EndTime));
            ModelState.Remove(nameof(task.ProductionOrder));
            ModelState.Remove(nameof(task.Worker));

            if (ModelState.IsValid)
            {
                _context.Update(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            LoadLists(task.OrderId, task.WorkerId, task.Stage, task.Status);
            return View(task);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var t = await _context.ProductionTasks
                        .Include(x => x.ProductionOrder)
                        .Include(x => x.Worker)
                        .FirstOrDefaultAsync(x => x.TaskId == id);
            if (t == null) return NotFound();
            return View(t);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t = await _context.ProductionTasks.FindAsync(id);
            if (t != null)
            {
                _context.ProductionTasks.Remove(t);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

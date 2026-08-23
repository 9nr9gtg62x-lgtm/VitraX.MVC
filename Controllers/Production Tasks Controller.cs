using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using VitraX.Domain.Entities;
using VitraX.MVC.Services;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class ProductionTasksController : Controller
    {
        private const string TasksResource = "api/productiontasks";
        private const string OrdersResource = "api/productionorders";
        private const string WorkersResource = "api/workers";

        private readonly IApiClient _apiClient;
        public ProductionTasksController(IApiClient apiClient) => _apiClient = apiClient;

        private static readonly string[] StageOptions = { "القص", "التقسية", "الفحص", "التغليف" };
        private static readonly string[] StatusOptions = { "قيد التنفيذ", "مكتمل", "متوقف" };

        private async Task LoadListsAsync(object? selOrder = null, object? selWorker = null,
                               object? selStage = null, object? selStatus = null)
        {
            var orders = await _apiClient.GetAllAsync<ProductionOrder>(OrdersResource);
            var workers = await _apiClient.GetAllAsync<Worker>(WorkersResource);
            ViewBag.Orders = new SelectList(orders, "OrderId", "OrderId", selOrder);
            ViewBag.Workers = new SelectList(workers, "WorkerId", "WorkerName", selWorker);
            ViewBag.Stages = new SelectList(StageOptions, selStage);
            ViewBag.Statuses = new SelectList(StatusOptions, selStatus);
        }

        // الـ API لا يُرجع الكيانات المرتبطة (الأمر والعامل) ضمن مهام الإنتاج، لذا نربطها هنا للعرض فقط
        private async Task AttachRelatedAsync(IEnumerable<ProductionTask> tasks)
        {
            var orders = (await _apiClient.GetAllAsync<ProductionOrder>(OrdersResource))
                .ToDictionary(o => o.OrderId);
            var workers = (await _apiClient.GetAllAsync<Worker>(WorkersResource))
                .ToDictionary(w => w.WorkerId);

            foreach (var task in tasks)
            {
                if (orders.TryGetValue(task.OrderId, out var order)) task.ProductionOrder = order;
                if (workers.TryGetValue(task.WorkerId, out var worker)) task.Worker = worker;
            }
        }

        public async Task<IActionResult> Index()
        {
            var tasks = await _apiClient.GetAllAsync<ProductionTask>(TasksResource);
            await AttachRelatedAsync(tasks);
            return View(tasks);
        }

        public async Task<IActionResult> Details(int id)
        {
            var t = await _apiClient.GetByIdAsync<ProductionTask>(TasksResource, id);
            if (t == null) return NotFound();
            await AttachRelatedAsync(new[] { t });
            return View(t);
        }

        public async Task<IActionResult> Create()
        {
            await LoadListsAsync();
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
                var response = await _apiClient.CreateAsync(TasksResource, task);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            }
            await LoadListsAsync(task.OrderId, task.WorkerId, task.Stage, task.Status);
            return View(task);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var t = await _apiClient.GetByIdAsync<ProductionTask>(TasksResource, id);
            if (t == null) return NotFound();
            await LoadListsAsync(t.OrderId, t.WorkerId, t.Stage, t.Status);
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
                var response = await _apiClient.UpdateAsync(TasksResource, id, task);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            }
            await LoadListsAsync(task.OrderId, task.WorkerId, task.Stage, task.Status);
            return View(task);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var t = await _apiClient.GetByIdAsync<ProductionTask>(TasksResource, id);
            if (t == null) return NotFound();
            await AttachRelatedAsync(new[] { t });
            return View(t);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiClient.DeleteAsync(TasksResource, id);
            return RedirectToAction(nameof(Index));
        }
    }
}

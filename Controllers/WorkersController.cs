using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VitraX.Domain.Entities;
using VitraX.MVC.Services;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class WorkersController : Controller
    {
        private const string Resource = "api/workers";
        private readonly IApiClient _apiClient;
        public WorkersController(IApiClient apiClient) => _apiClient = apiClient;

        public async Task<IActionResult> Index()
            => View(await _apiClient.GetAllAsync<Worker>(Resource));

        public async Task<IActionResult> Details(int id)
        {
            var worker = await _apiClient.GetByIdAsync<Worker>(Resource, id);
            if (worker == null) return NotFound();
            return View(worker);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Worker worker)
        {
            if (ModelState.IsValid)
            {
                var response = await _apiClient.CreateAsync(Resource, worker);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            }
            return View(worker);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var worker = await _apiClient.GetByIdAsync<Worker>(Resource, id);
            if (worker == null) return NotFound();
            return View(worker);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Worker worker)
        {
            if (id != worker.WorkerId) return NotFound();
            if (ModelState.IsValid)
            {
                var response = await _apiClient.UpdateAsync(Resource, id, worker);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Index));
                ModelState.AddModelError(string.Empty, await response.Content.ReadAsStringAsync());
            }
            return View(worker);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var worker = await _apiClient.GetByIdAsync<Worker>(Resource, id);
            if (worker == null) return NotFound();
            return View(worker);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _apiClient.DeleteAsync(Resource, id);
            return RedirectToAction(nameof(Index));
        }
    }
}

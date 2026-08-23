using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VitraX.Infrastructure.Data;
using VitraX.Domain.Entities;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class WorkersController : Controller
    {
        private readonly AppDbContext _context;
        public WorkersController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index()
            => View(await _context.Workers.ToListAsync());

        public async Task<IActionResult> Details(int id)
        {
            var w = await _context.Workers.FirstOrDefaultAsync(x => x.WorkerId == id);
            if (w == null) return NotFound();
            return View(w);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Worker worker)
        {
            if (ModelState.IsValid)
            {
                _context.Workers.Add(worker);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(worker);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var w = await _context.Workers.FindAsync(id);
            if (w == null) return NotFound();
            return View(w);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Worker worker)
        {
            if (id != worker.WorkerId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(worker);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(worker);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var w = await _context.Workers.FirstOrDefaultAsync(x => x.WorkerId == id);
            if (w == null) return NotFound();
            return View(w);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var w = await _context.Workers.FindAsync(id);
            if (w != null)
            {
                _context.Workers.Remove(w);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VitraX.MVC.Models;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly VitraX.Api.Data.AppDbContext _context;
        public HomeController(ILogger<HomeController> logger, VitraX.Api.Data.AppDbContext context)
        {
           
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalOrders = _context.ProductionOrders.Count();
            ViewBag.TotalTasks = _context.ProductionTasks.Count();
            ViewBag.TotalWorkers = _context.Workers.Count();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

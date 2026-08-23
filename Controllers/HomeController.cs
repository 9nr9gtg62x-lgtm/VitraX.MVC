using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using VitraX.Domain.Entities;
using VitraX.MVC.Models;
using VitraX.MVC.Services;

namespace VitraX.MVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IApiClient _apiClient;
        public HomeController(IApiClient apiClient) => _apiClient = apiClient;

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = (await _apiClient.GetAllAsync<Product>("api/products")).Count;
            ViewBag.TotalOrders = (await _apiClient.GetAllAsync<ProductionOrder>("api/productionorders")).Count;
            ViewBag.TotalTasks = (await _apiClient.GetAllAsync<ProductionTask>("api/productiontasks")).Count;
            ViewBag.TotalWorkers = (await _apiClient.GetAllAsync<Worker>("api/workers")).Count;
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using System.Diagnostics;
using SadStore.Models; // Ensure ErrorViewModel is accessible if defined, otherwise use dynamic/ViewBag

namespace SadStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly StoreContext _context;

        public HomeController(StoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Products
                .Where(p => p.IsFeatured)
                .Take(4)
                .ToListAsync();

            return View(featuredProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult ReturnPolicy()
        {
            return View();
        }

        public IActionResult ReturnMethod()
        {
            return View();
        }

        public IActionResult Delivery()
        {
            return View();
        }

        public IActionResult GiftPolicy()
        {
            return View();
        }

        public IActionResult VipPoints()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
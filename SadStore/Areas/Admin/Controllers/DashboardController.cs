using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")] // قم بتفعيل هذا السطر بعد إنشاء الأدوار والمستخدمين
    public class DashboardController : Controller
    {
        private readonly StoreContext _context;

        public DashboardController(StoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // إحصائيات عامة
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();

            // حساب الإيرادات (تأكد من وجود بيانات وإلا ستعود بـ 0)
            ViewBag.TotalRevenue = await _context.Orders.AnyAsync()
                ? await _context.Orders.SumAsync(o => o.TotalAmount)
                : 0;

            ViewBag.NewOrders = await _context.Orders.CountAsync(o => o.Status == "جديد");

            // آخر 5 طلبات
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Globalization;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class DashboardController : Controller
    {
        private readonly StoreContext _context;
        private readonly LanguageService _lang;

        public DashboardController(StoreContext context, LanguageService lang)
        {
            _context = context;
            _lang = lang;
        }

        public async Task<IActionResult> Index()
        {
            // 1. إحصائيات البطاقات
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders.AnyAsync()
                ? await _context.Orders.SumAsync(o => o.TotalAmount)
                : 0;
            ViewBag.NewOrders = await _context.Orders.CountAsync(o => o.Status == "جديد");

            // 2. بيانات الجدول (آخر الطلبات)
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // 3. بيانات الرسم البياني (حالات الطلبات)
            // نستخدم الأسماء الانجليزية للمفاتيح، وسيتم ترجمتها في الفيو
            var statusData = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.StatusLabels = statusData.Select(s => s.Status).ToList(); // سنترجمها في الفيو
            ViewBag.StatusCounts = statusData.Select(s => s.Count).ToList();

            // 4. بيانات الرسم البياني (المبيعات الشهرية - آخر 6 أشهر)
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var salesData = await _context.Orders
                .Where(o => o.OrderDate >= sixMonthsAgo)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // تحويل أرقام الشهور إلى أسماء مترجمة
            var monthNames = salesData.Select(d => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(d.Month)).ToList();
            var salesAmounts = salesData.Select(d => d.Total).ToList();

            ViewBag.SalesLabels = monthNames;
            ViewBag.SalesData = salesAmounts;

            return View(recentOrders);
        }
    }
}
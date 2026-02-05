using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly StoreContext _context;
        private readonly LanguageService _lang;

        public OrdersController(StoreContext context, LanguageService lang)
        {
            _context = context;
            _lang = lang;
        }

        // عرض قائمة الطلبات (بحث + فلترة + تقسيم صفحات)
        public async Task<IActionResult> Index(string status, string search, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Orders.AsQueryable();

            // 1. الفلترة بالحالة
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // 2. البحث (رقم الطلب، اسم العميل، رقم الهاتف)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                query = query.Where(o => o.Id.ToString().Contains(search) ||
                                         o.CustomerName.Contains(search) ||
                                         o.PhoneNumber.Contains(search));
            }

            // 3. الترتيب (الأحدث أولاً)
            query = query.OrderByDescending(o => o.OrderDate);

            // 4. تقسيم الصفحات
            int totalItems = await query.CountAsync();
            var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            ViewBag.TotalCount = totalItems;

            return View(orders);
        }

        // تفاصيل الطلب
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images) // لجلب الصور في التفاصيل
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // تحديث حالة الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Saved successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Error occurred";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // صفحة الفاتورة للطباعة
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // تصدير الطلبات إلى Excel (CSV)
        public async Task<IActionResult> Export()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            builder.Append('\uFEFF'); // BOM لدعم العربية

            if (isRtl)
                builder.AppendLine("رقم الطلب,العميل,الهاتف,العنوان,التاريخ,الإجمالي,الحالة");
            else
                builder.AppendLine("Order ID,Customer,Phone,Address,Date,Total,Status");

            foreach (var o in orders)
            {
                var name = o.CustomerName?.Replace(",", " ") ?? "";
                var phone = o.PhoneNumber?.Replace(",", " ") ?? "";
                var address = o.Address?.Replace(",", " ") ?? "";
                var status = _lang.Get(o.Status).Replace(",", " ");

                builder.AppendLine($"{o.Id},{name},{phone},{address},{o.OrderDate:yyyy-MM-dd},{o.TotalAmount},{status}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"orders_{DateTime.Now:yyyyMMdd}.csv");
        }

        // حذف الطلب
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
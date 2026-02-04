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

        // عرض الطلبات مع الفلترة
        public async Task<IActionResult> Index(string status, string search)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.Id.ToString().Contains(search) || o.CustomerName.Contains(search) || o.PhoneNumber.Contains(search));
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;

            return View(orders);
        }

        // تصدير الطلبات إلى Excel (CSV)
        public async Task<IActionResult> Export()
        {
            // جلب كل الطلبات (يمكن تعديلها لتصدير الطلبات المفلترة فقط إذا أردت)
            var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            // إضافة BOM لدعم الحروف العربية في Excel
            var preamble = Encoding.UTF8.GetPreamble();

            // كتابة العناوين حسب اللغة
            if (isRtl)
            {
                builder.AppendLine("رقم الطلب,العميل,الهاتف,العنوان,التاريخ,الإجمالي,الحالة");
            }
            else
            {
                builder.AppendLine("Order ID,Customer,Phone,Address,Date,Total,Status");
            }

            foreach (var o in orders)
            {
                // تنظيف البيانات من الفواصل لتجنب كسر ملف CSV
                var name = o.CustomerName?.Replace(",", " ") ?? "";
                var phone = o.PhoneNumber?.Replace(",", " ") ?? "";
                var address = o.Address?.Replace(",", " ") ?? "";
                var status = _lang.Get(o.Status).Replace(",", " "); // ترجمة الحالة

                builder.AppendLine($"{o.Id},{name},{phone},{address},{o.OrderDate:yyyy-MM-dd},{o.TotalAmount},{status}");
            }

            // دمج BOM مع المحتوى النصي
            var contentBytes = Encoding.UTF8.GetBytes(builder.ToString());
            var resultBytes = new byte[preamble.Length + contentBytes.Length];
            Array.Copy(preamble, 0, resultBytes, 0, preamble.Length);
            Array.Copy(contentBytes, 0, resultBytes, preamble.Length, contentBytes.Length);

            string fileName = isRtl ? "orders_report_ar.csv" : "orders_report_en.csv";
            return File(resultBytes, "text/csv", fileName);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

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
    }
}
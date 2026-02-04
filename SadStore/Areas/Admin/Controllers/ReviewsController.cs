using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReviewsController : Controller
    {
        private readonly StoreContext _context;
        private readonly LanguageService _lang;

        public ReviewsController(StoreContext context, LanguageService lang)
        {
            _context = context;
            _lang = lang;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CustomerReviews.ToListAsync());
        }

        // تصدير التقييمات إلى Excel (CSV)
        public async Task<IActionResult> Export()
        {
            var reviews = await _context.CustomerReviews.ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            var preamble = Encoding.UTF8.GetPreamble();

            if (isRtl)
            {
                builder.AppendLine("العميل,التقييم,التعليق,الحالة");
            }
            else
            {
                builder.AppendLine("Customer,Rating,Comment,Status");
            }

            foreach (var r in reviews)
            {
                var name = r.CustomerName?.Replace(",", " ") ?? "";
                var text = r.ReviewText?.Replace(",", " ") ?? "";
                var status = r.IsApproved ? (isRtl ? "منشور" : "Published") : (isRtl ? "قيد المراجعة" : "Pending");

                builder.AppendLine($"{name},{r.Rating},{text},{status}");
            }

            var contentBytes = Encoding.UTF8.GetBytes(builder.ToString());
            var resultBytes = new byte[preamble.Length + contentBytes.Length];
            Array.Copy(preamble, 0, resultBytes, 0, preamble.Length);
            Array.Copy(contentBytes, 0, resultBytes, preamble.Length, contentBytes.Length);

            string fileName = isRtl ? "reviews_report_ar.csv" : "reviews_report_en.csv";
            return File(resultBytes, "text/csv", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var review = await _context.CustomerReviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = !review.IsApproved;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Saved successfully";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.CustomerReviews.FindAsync(id);
            if (review != null)
            {
                _context.CustomerReviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReviewsController : Controller
    {
        private readonly StoreContext _context;

        public ReviewsController(StoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CustomerReviews.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var review = await _context.CustomerReviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = !review.IsApproved;
                await _context.SaveChangesAsync();
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
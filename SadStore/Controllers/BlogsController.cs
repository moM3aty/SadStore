using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;

namespace SadStore.Controllers
{
    public class BlogsController : Controller
    {
        private readonly StoreContext _context;

        public BlogsController(StoreContext context)
        {
            _context = context;
        }

        // عرض قائمة المقالات للمستخدم
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.BlogPosts
                .OrderByDescending(b => b.PublishedDate)
                .ToListAsync();
            return View(blogs);
        }

        // عرض تفاصيل المقال
        public async Task<IActionResult> Details(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            // مقالات ذات صلة (آخر 3 مقالات غير الحالية)
            ViewBag.RelatedPosts = await _context.BlogPosts
                .Where(b => b.Id != id)
                .OrderByDescending(b => b.PublishedDate)
                .Take(3)
                .ToListAsync();

            return View(blogPost);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;

namespace SadStore.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly StoreContext _context;

        public ProfileController(UserManager<IdentityUser> userManager, StoreContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Auth");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerName == user.UserName) 
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.UserEmail = user.Email;
            ViewBag.UserName = user.UserName;

            return View(orders);
        }
    }
}
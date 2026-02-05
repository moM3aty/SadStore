using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly StoreContext _context;
        private readonly LanguageService _lang;

        public UsersController(UserManager<IdentityUser> userManager, StoreContext context, LanguageService lang)
        {
            _userManager = userManager;
            _context = context;
            _lang = lang;
        }

        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 10;
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
            }

            int totalItems = await query.CountAsync();
            var users = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Search = search;
            ViewBag.TotalCount = totalItems;

            return View(users);
        }

        // صفحة التعديل (محفظة، نقاط، وإرسال إشعار)
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var detail = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserId == user.Id);
            if (detail == null)
            {
                detail = new UserDetail { UserId = user.Id, WalletBalance = 0, LoyaltyPoints = 0 };
                _context.UserDetails.Add(detail);
                await _context.SaveChangesAsync();
            }

            ViewBag.UserDetail = detail;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, decimal walletBalance, int loyaltyPoints)
        {
            var detail = await _context.UserDetails.FirstOrDefaultAsync(u => u.UserId == id);
            if (detail != null)
            {
                detail.WalletBalance = walletBalance;
                detail.LoyaltyPoints = loyaltyPoints;
                _context.UserDetails.Update(detail);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Saved successfully";
            }
            return RedirectToAction(nameof(Index));
        }

        // دالة إرسال الإشعار
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(string userId, string title, string message)
        {
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(message))
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Date = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Notification Sent";
            }
            return RedirectToAction(nameof(Edit), new { id = userId });
        }

        public async Task<IActionResult> Export()
        {
            var users = await _userManager.Users.ToListAsync();
            var builder = new StringBuilder();
            builder.AppendLine("Id,Email,UserName");
            foreach (var u in users) builder.AppendLine($"{u.Id},{u.Email},{u.UserName}");
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "users.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Services;
using System.Text;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly LanguageService _lang;

        public UsersController(UserManager<IdentityUser> userManager, LanguageService lang)
        {
            _userManager = userManager;
            _lang = lang;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // تصدير المستخدمين إلى Excel (CSV)
        public async Task<IActionResult> Export()
        {
            var users = await _userManager.Users.ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            var preamble = Encoding.UTF8.GetPreamble();

            if (isRtl)
            {
                builder.AppendLine("معرف المستخدم,البريد الإلكتروني,رقم الهاتف,اسم المستخدم");
            }
            else
            {
                builder.AppendLine("User ID,Email,Phone,Username");
            }

            foreach (var u in users)
            {
                var email = u.Email?.Replace(",", " ") ?? "";
                var phone = u.PhoneNumber?.Replace(",", " ") ?? "-";

                builder.AppendLine($"{u.Id},{email},{phone},{u.UserName}");
            }

            var contentBytes = Encoding.UTF8.GetBytes(builder.ToString());
            var resultBytes = new byte[preamble.Length + contentBytes.Length];
            Array.Copy(preamble, 0, resultBytes, 0, preamble.Length);
            Array.Copy(contentBytes, 0, resultBytes, preamble.Length, contentBytes.Length);

            string fileName = isRtl ? "users_report_ar.csv" : "users_report_en.csv";
            return File(resultBytes, "text/csv", fileName);
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
            else
            {
                TempData["ErrorMessage"] = "Error occurred";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
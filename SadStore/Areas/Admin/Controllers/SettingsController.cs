using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly StoreContext _context;
        private readonly LanguageService _lang;

        public SettingsController(StoreContext context, LanguageService lang)
        {
            _context = context;
            _lang = lang;
        }

        public async Task<IActionResult> Index()
        {
            var promoSetting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "PromoBar");

            if (promoSetting == null)
            {
                promoSetting = new SiteSetting
                {
                    Key = "PromoBar",
                    ValueAr = "نص الإعلان بالعربية",
                    ValueEn = "Promo Text in English"
                };
                _context.SiteSettings.Add(promoSetting);
                await _context.SaveChangesAsync();
            }

            return View(promoSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePromo(int id, string ValueAr, string ValueEn)
        {
            var setting = await _context.SiteSettings.FindAsync(id);
            if (setting != null)
            {
                setting.ValueAr = ValueAr;
                setting.ValueEn = ValueEn;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Settings updated successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
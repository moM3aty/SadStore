using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;
using SadStore.Services;
using System.Text;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShippingController : Controller
    {
        private readonly StoreContext _context;
        private readonly LanguageService _lang;

        public ShippingController(StoreContext context, LanguageService lang)
        {
            _context = context;
            _lang = lang;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.ShippingLocations.ToListAsync());
        }

        public async Task<IActionResult> Export()
        {
            var locations = await _context.ShippingLocations.ToListAsync();
            var isRtl = _lang.IsRtl();
            var builder = new StringBuilder();

            builder.Append('\uFEFF');

            if (isRtl)
                builder.AppendLine("المعرف,المدينة / المنطقة (عربي),المدينة / المنطقة (إنجليزي),تكلفة الشحن");
            else
                builder.AppendLine("ID,City (Ar),City (En),Shipping Cost");

            foreach (var item in locations)
            {
                var cityAr = item.CityNameAr?.Replace(",", " ") ?? "";
                var cityEn = item.CityNameEn?.Replace(",", " ") ?? "";
                builder.AppendLine($"{item.Id},{cityAr},{cityEn},{item.ShippingCost}");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"shipping_{DateTime.Now:yyyyMMdd}.csv");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShippingLocation shippingLocation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shippingLocation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Saved successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(shippingLocation);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var shippingLocation = await _context.ShippingLocations.FindAsync(id);
            if (shippingLocation == null) return NotFound();
            return View(shippingLocation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShippingLocation shippingLocation)
        {
            if (id != shippingLocation.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shippingLocation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Saved successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ShippingLocations.Any(e => e.Id == shippingLocation.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(shippingLocation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _context.ShippingLocations.FindAsync(id);
            if (location != null)
            {
                _context.ShippingLocations.Remove(location);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Deleted successfully";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;

namespace SadStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ShippingController : Controller
    {
        private readonly StoreContext _context;

        public ShippingController(StoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.ShippingLocations.ToListAsync());
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
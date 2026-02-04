using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadStore.Data;

namespace SadStore.ViewComponents
{
    public class NavbarCategoriesViewComponent : ViewComponent
    {
        private readonly StoreContext _context;

        public NavbarCategoriesViewComponent(StoreContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
    }
}
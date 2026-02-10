using SadStore.Data;
using System.Globalization;

namespace SadStore.Services
{
    public class AppSettingService
    {
        private readonly StoreContext _context;

        public AppSettingService(StoreContext context)
        {
            _context = context;
        }

        public string GetPromoText()
        {
            var isRtl = CultureInfo.CurrentCulture.Name.StartsWith("ar");

            var setting = _context.SiteSettings.FirstOrDefault(s => s.Key == "PromoBar");

            if (setting != null)
            {
                return isRtl ? setting.ValueAr : setting.ValueEn;
            }

            return isRtl
                ? "تمتعي بخصم مميز 10% كود [صاد] - شحن مجاني للطلبات فوق 499SR"
                : "Enjoy a special 10% discount with code [SAD] - Free shipping on orders over 499 SR";
        }
    }
}
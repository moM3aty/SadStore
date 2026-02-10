using Microsoft.AspNetCore.Http;
using System;

namespace SadStore.Services
{
    public class CurrencyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CurrencyCookieKey = "SadStore_Currency";

        public CurrencyService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentCurrency()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context.Request.Cookies.ContainsKey(CurrencyCookieKey))
            {
                return context.Request.Cookies[CurrencyCookieKey];
            }
            return "SAR"; 
        }

        public decimal Convert(decimal amountInSar)
        {
            var currency = GetCurrentCurrency();
            if (currency == "AED")
            {
                return amountInSar * 0.98m; 
            }
            return amountInSar;
        }

        public string GetSymbol()
        {
            var currency = GetCurrentCurrency();
            return currency == "AED" ? "AED" : "SAR";
        }

        public bool IsSar()
        {
            return GetCurrentCurrency() == "SAR";
        }
    }
}
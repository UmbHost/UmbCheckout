using System.Globalization;

namespace UmbCheckout.Shared.Extensions
{
    public static class CultureInfoExtensions
    {
        public static string? GetISOCurrencySymbol(this CultureInfo culture)
        {
            RegionInfo regionInfo = new RegionInfo(culture.LCID);
            return regionInfo.ISOCurrencySymbol;
        }
    }
}

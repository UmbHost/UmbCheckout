using UmbHost.Licensing.Models;

namespace UmbCheckout.Testing;

/// <summary>Sets the static licence flag for one test and restores it. Tests using it must be in <c>DatabaseCollection</c> (not run in parallel).</summary>
public static class LicenseState
{
    public static IDisposable Set(bool licensed)
    {
        var property = typeof(UmbCheckoutSettings).GetProperty(nameof(UmbCheckoutSettings.IsLicensed))!;
        var was = (bool)property.GetValue(null)!;
        property.SetValue(null, licensed);
        return new Restore(() => property.SetValue(null, was));
    }

    private sealed class Restore(Action restore) : IDisposable
    {
        public void Dispose() => restore();
    }
}

using UmbHost.Licensing.Services;

namespace UmbCheckout.Testing;

/// <summary>
/// For the open packages' constructors that still take a <see cref="LicenseService"/>: none of them need it in tests, so there is nothing to build: they accept no licence service.
/// </summary>
public static class TestLicenseService
{
    public static LicenseService? None => null;
}

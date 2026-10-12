using UmbHost.Licensing.Services;

namespace UmbCheckout.Testing;

/// <summary>
/// For the open packages' constructors that still take a <see cref="LicenseService"/>: none of them call it in tests (and the closed
/// licensing library's internals stay out of the open repo), so there is nothing to build.
/// </summary>
public static class TestLicenseService
{
    public static LicenseService Create() => null!;
}

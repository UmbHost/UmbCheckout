using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using UmbHost.Licensing.Models;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace UmbCheckout.Testing;

/// <summary>A LicenseService whose RunLicenseCheck() returns at once (runtime level Boot), leaving the licence flag to <see cref="LicenseState"/>.</summary>
public static class TestLicenseService
{
    public static LicenseService Create() => new(
        Mock.Of<IHttpClientFactory>(),
        NullLogger<LicenseService>.Instance,
        Mock.Of<IWebHostEnvironment>(),
        Options.Create(new UmbCheckoutAppSettings()),
        Mock.Of<ICoreScopeProvider>(),
        Mock.Of<IRuntimeState>(r => r.Level == RuntimeLevel.Boot),
        Options.Create(new WebRoutingSettings()),
        null!);
}

using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using UmbHost.Licensing.Models;
using UmbHost.Licensing.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

namespace UmbCheckout.Testing;

/// <summary>A LicenseService that does nothing (runtime level Boot), leaving the licence flag to <see cref="LicenseState"/>.</summary>
public static class TestLicenseService
{
    public static LicenseService Create()
    {
        // The constructor is internal to UmbHost.Licensing (its cache store is not public API).
        var constructor = typeof(LicenseService).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
        return (LicenseService)constructor.Invoke(
        [
            Mock.Of<IHttpClientFactory>(),
            NullLogger<LicenseService>.Instance,
            Mock.Of<IWebHostEnvironment>(),
            Options.Create(new UmbCheckoutAppSettings()),
            Mock.Of<IEventAggregator>(),
            Mock.Of<IRuntimeState>(r => r.Level == RuntimeLevel.Boot),
            Options.Create(new WebRoutingSettings()),
            null,
            TimeProvider.System,
            null,
            null
        ]);
    }
}

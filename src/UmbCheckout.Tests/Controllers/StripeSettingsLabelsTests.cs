using System.Globalization;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UmbCheckout.Stripe.Controllers.BackOffice.Api;
using UmbCheckout.Stripe.Interfaces;
using UmbCheckout.Tests.Helpers;
using Umbraco.Cms.Core.Services;
using Xunit;

namespace UmbCheckout.Tests.Controllers;

/// <summary>
/// Every label and description on the Stripe settings screen is a key that exists, in the area it is looked up in (the labels were
/// looked up in "umbcheckout" but defined in "umbcheckoutstripe", so the screen showed "[collect_phone_number_label]" placeholders).
/// </summary>
public sealed class StripeSettingsLabelsTests
{
    private static string SrcDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "UmbCheckout.Stripe", "wwwroot")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("The src directory was not found.");
    }

    /// <summary>The keys the backoffice has, by area, from the open packages' lang files.</summary>
    internal static Dictionary<(string Area, string Key), string> LangKeys()
    {
        var src = SrcDirectory();
        var files = new[]
        {
            Path.Combine(src, "UmbCheckout.Backoffice", "wwwroot", "UmbCheckout", "lang", "en-US.xml"),
            Path.Combine(src, "UmbCheckout.Stripe", "wwwroot", "UmbCheckout", "Stripe", "lang", "en-US.xml"),
        };

        var keys = new Dictionary<(string, string), string>();
        foreach (var file in files)
        {
            foreach (var area in XDocument.Load(file).Root.Required().Elements("area"))
            {
                foreach (var key in area.Elements("key"))
                {
                    keys[(area.Attribute("alias").Required().Value, key.Attribute("alias").Required().Value)] = key.Value;
                }
            }
        }

        return keys;
    }

    [Fact]
    public async Task Every_Stripe_settings_label_and_description_exists_in_its_area()
    {
        var keys = LangKeys();
        var text = new Mock<ILocalizedTextService>();
        // As Umbraco: a missing key comes back as "[key]"
        text.Setup(t => t.Localize(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CultureInfo?>(), It.IsAny<IDictionary<string, string?>?>()))
            .Returns<string?, string?, CultureInfo?, IDictionary<string, string?>?>((area, key, _, _) =>
                keys.TryGetValue((area ?? string.Empty, key ?? string.Empty), out var value) ? value : "[" + key + "]");
        var controller = new StripeSettingsApiController(NullLogger<StripeSettingsApiController>.Instance, null, text.Object, Mock.Of<IStripeSettingsService>());

        var properties = JsonResultAssert.Body(await controller.GetStripeSettings()).AsArray();

        Assert.NotEmpty(properties);
        Assert.All(properties, property =>
        {
            var node = property.Required();
            Assert.DoesNotContain("[", node["label"].Required().GetValue<string>());
            Assert.DoesNotContain("[", node["description"].Required().GetValue<string>());
        });
    }
}

using System.Reflection;
using Xunit;

namespace UmbCheckout.Tests;

/// <summary>Every shipped open-source assembly carries the UmbHost banner, readable in a decompiler.</summary>
public sealed class BannerTests
{
    public static TheoryData<string> OpenAssemblies => new()
    {
        "UmbCheckout.Core",
        "UmbCheckout.Shared",
        "UmbCheckout.Backoffice",
        "UmbCheckout.Stripe",
    };

    [Theory]
    [MemberData(nameof(OpenAssemblies))]
    public void An_open_assembly_carries_the_banner_with_the_open_source_line(string assemblyName)
    {
        var text = Banner(Assembly.Load(assemblyName));

        Assert.Contains(@"| | | | _ __ ___  | |__  | | | |  ___   ___  | |_", text);
        Assert.Contains("UmbCheckout by UmbHost Limited - https://umbhost.net", text);
        Assert.Contains("Open source - contributions welcome: https://github.com/UmbHost/UmbCheckout", text);
        Assert.DoesNotContain("buy a licence", text);
    }

    internal static string Banner(Assembly assembly)
    {
        var type = assembly.GetType("UmbHost.UmbHostBanner", throwOnError: true)!;
        var field = type.GetField("Text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
        return (string)field.GetRawConstantValue()!;
    }
}

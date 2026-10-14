using System.Runtime.CompilerServices;
using UmbCheckout.Testing;
using Xunit;

namespace UmbCheckout.Tests.Architecture;

/// <summary>
/// Nullable problems are fixed, not silenced: no production file uses <c>null!</c>, <c>default!</c> or any other null-forgiving <c>!</c>.
/// Test projects are left out of the scan (they must still compile without nullable warnings).
/// </summary>
public sealed class NoNullForgivingTests
{
    [Fact]
    public void No_production_file_uses_the_null_forgiving_operator()
    {
        var root = ProductionSource.FindRoot(ThisFile(), Path.Combine("src", "UmbCheckout.Core"));

        Assert.True(ProductionSource.Files(root).Count > 50, "The scan found too few production files: is the root right?");
        var offenders = ProductionSource.NullForgiving(root);

        Assert.True(offenders.Count == 0, "Null-forgiving operators (fix the null instead):" + Environment.NewLine + string.Join(Environment.NewLine, offenders));
    }

    [Theory]
    [InlineData("var x = null!;")]
    [InlineData("private string _name = default!;")]
    [InlineData("var name = user!.Name;")]
    [InlineData("var count = Find()!;")]
    [InlineData("var item = list[0]!;")]
    [InlineData("var item = value! ?? other;")]
    [InlineData("var item = value!")]
    public void The_scan_finds_a_null_forgiving_operator(string line)
        => Assert.Single(ProductionSource.NullForgivingIn(line));

    [Theory]
    [InlineData("if (a != b) { }")]
    [InlineData("if (!ready) { }")]
    [InlineData("var x = a?.B ?? \"null!\";")]
    [InlineData("// var x = null!;")]
    [InlineData("/* var x = default!; */")]
    [InlineData("var path = @\"C:\\a!\";")]
    [InlineData("var message = $\"Hello {name}!\";")]
    [InlineData("var c = '!';")]
    public void The_scan_ignores_comments_strings_and_other_uses_of_the_exclamation_mark(string line)
        => Assert.Empty(ProductionSource.NullForgivingIn(line));

    private static string ThisFile([CallerFilePath] string path = "") => path;
}

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
    [InlineData("var result = callback!(argument);")]
    [InlineData("var result = factory!.Create()!(argument);")]
    [InlineData("var same = value! == other;")]
    [InlineData("var chosen = ready ? value! : other;")]
    [InlineData("var sum = left! + right;")]
    [InlineData("var first = values![0];")]
    [InlineData("var length = name!.Length;")]
    [InlineData("var item = null !;")]
    [InlineData("return value!;")]
    [InlineData("var f = x => y!.Z;")]
    [InlineData("var s = $\"{x!.Y}\";")]
    [InlineData("var s = $\"Hello {name!}\";")]
    [InlineData("var s = $@\"C:\\{folder!.Name}\";")]
    [InlineData("var s = @$\"C:\\{folder!.Name}\";")]
    [InlineData("var s = $\"{a} and {b!.C:N2} and {d}\";")]
    [InlineData("var s = $\"{Format(y!)}\";")]
    [InlineData("var s = $\"{(ready ? value! : \"none\")}\";")]
    [InlineData("var s = $\"\"\"{x!.Y}\"\"\";")]
    [InlineData("var s = $$\"\"\"{{x!.Y}}\"\"\";")]
    public void The_scan_finds_a_null_forgiving_operator(string line)
        => Assert.Single(ProductionSource.NullForgivingIn(line));

    [Theory]
    [InlineData("if (a != b) { }")]
    [InlineData("if (!ready) { }")]
    [InlineData("if (a!=b) { }")]
    [InlineData("return !(a && b);")]
    [InlineData("var x = a && !(b);")]
    [InlineData("var y = !(a);")]
    [InlineData("var z = a is not null && !b;")]
    [InlineData("var x = a?.B ?? \"null!\";")]
    [InlineData("// var x = null!;")]
    [InlineData("/* var x = default!; */")]
    [InlineData("var path = @\"C:\\a!\";")]
    [InlineData("var message = $\"Hello {name}!\";")]
    [InlineData("var c = '!';")]
    [InlineData("var f = x=>!y;")]
    [InlineData("var f = x => !y;")]
    [InlineData("items.Where(x=>!x.Ready);")]
    [InlineData("items.Any(x=>!string.IsNullOrEmpty(x));")]
    [InlineData("var s = $\"Hello {name}!\";")]
    [InlineData("var s = $\"{a}!{b}!\";")]
    [InlineData("var s = $\"{{x!}}\";")]
    [InlineData("var s = $\"{value:N2}!\";")]
    [InlineData("var s = $\"{(ready ? \"yes!\" : \"no!\")}\";")]
    [InlineData("var s = $@\"C:\\{folder}!\";")]
    [InlineData("var s = $\"\"\"Hello!\"\"\";")]
    [InlineData("var s = $\"{(!ready ? 1 : 2)}\";")]
    public void The_scan_ignores_comments_strings_and_other_uses_of_the_exclamation_mark(string line)
        => Assert.Empty(ProductionSource.NullForgivingIn(line));

    private static string ThisFile([CallerFilePath] string path = "") => path;
}

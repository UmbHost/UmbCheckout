using System.Text.RegularExpressions;
using Xunit;

namespace UmbCheckout.Tests.Architecture;

/// <summary>
/// Quality Phase 6: log calls use a constant template that says what failed, with named values. An exception's message as the
/// template gives a different "template" per failure (log search and grouping can't see them as one event) and can carry braces.
/// Scans the source of the nearest solution above the test (the open repo here; the whole checkout from the closed copy that links this file).
/// </summary>
public sealed class LoggingTests
{
    private static readonly Regex MessageAsTemplateRegex = new(@"\.Log(?:Trace|Debug|Information|Warning|Error|Critical)\(\s*(?:\w+\s*,\s*)?\w+\.Message\s*[,)]", RegexOptions.Compiled);

    private static Regex MessageAsTemplate() => MessageAsTemplateRegex;

    [Fact]
    public void No_log_call_uses_an_exception_message_as_its_template()
    {
        var root = SolutionRoot();
        var offenders = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsExcluded(Path.GetRelativePath(root, path)))
            .SelectMany(path => File.ReadLines(path).Select((line, index) => (path, line, index)))
            .Where(x => MessageAsTemplate().IsMatch(x.line))
            .Select(x => $"{Path.GetRelativePath(root, x.path)}:{x.index + 1}")
            .ToList();

        Assert.True(offenders.Count == 0, "Exception message used as a log template: " + string.Join(", ", offenders));
    }

    [Theory]
    [InlineData("_logger.LogError(ex, ex.Message);", true)]
    [InlineData("_logger.LogCritical(exception, exception.Message);", true)]
    [InlineData("_logger.LogWarning(ex.Message);", true)]
    [InlineData("_logger.LogError(ex, \"Checkout failed for basket {BasketId}\", basketId);", false)]
    [InlineData("Logger.LogWarning(\"Stripe request failed in {Controller}: {StripeError}\", GetType().Name, ex.Message);", false)]
    public void The_scan_tells_a_message_template_from_a_named_value(string line, bool flagged)
        => Assert.Equal(flagged, MessageAsTemplate().IsMatch(line));

    private static bool IsExcluded(string relative)
    {
        var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(p => p is "bin" or "obj" or "node_modules" || p.EndsWith(".Tests", StringComparison.Ordinal));
    }

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !directory.EnumerateFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("No solution above " + AppContext.BaseDirectory);
    }
}

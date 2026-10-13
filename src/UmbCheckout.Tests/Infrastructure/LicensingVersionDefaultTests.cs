using System.Text.RegularExpressions;
using Xunit;

namespace UmbCheckout.Tests.Infrastructure
{
    /// <summary>The UmbHost.Licensing version a package build falls back to is the 2.x line UmbCheckout 17 uses, not the 1.x line of v13.</summary>
    public class LicensingVersionDefaultTests
    {
        private static string? Find(string fileName)
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, fileName);
                if (File.Exists(candidate) && File.ReadAllText(candidate).Contains("UmbHostLicensingVersion", StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        [Fact]
        public void The_default_licensing_version_is_a_2x_version()
        {
            var props = Find("Directory.Build.props");
            Assert.NotNull(props);

            var match = Regex.Match(File.ReadAllText(props!), @"<UmbHostLicensingVersion Condition=""[^""]*"">([^<]+)</UmbHostLicensingVersion>");

            Assert.True(match.Success);
            Assert.StartsWith("2.", match.Groups[1].Value, StringComparison.Ordinal);
        }
    }
}

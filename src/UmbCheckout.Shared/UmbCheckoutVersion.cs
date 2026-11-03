using System.Reflection;
using Umbraco.Cms.Core.Semver;

namespace UmbCheckout.Shared
{
    public static class UmbCheckoutVersion
    {
        static UmbCheckoutVersion()
        {
            var assembly = typeof(UmbCheckoutVersion).Assembly;

            AssemblyVersion = assembly.GetName().Version;

            var fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
                ?? throw new InvalidOperationException($"{assembly.GetName().Name} has no assembly file version.");
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? throw new InvalidOperationException($"{assembly.GetName().Name} has no informational version.");

            AssemblyFileVersion = Version.Parse(fileVersion);

            SemanticVersion = SemVersion.Parse(informationalVersion);

            Version = new Version(SemanticVersion.Major, SemanticVersion.Minor, SemanticVersion.Patch);
        }

        public static Version? AssemblyVersion { get; }

        public static Version AssemblyFileVersion { get; }

        public static string Comment => SemanticVersion.Prerelease;

        public static SemVersion SemanticVersion { get; }

        public static Version Version { get; }
    }
}

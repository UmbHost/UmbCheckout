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

            AssemblyFileVersion = Version.Parse(assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version);

            SemanticVersion = SemVersion.Parse(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);

            Version = new Version(SemanticVersion.Major, SemanticVersion.Minor, SemanticVersion.Patch);
        }

        public static Version? AssemblyVersion { get; }

        public static Version AssemblyFileVersion { get; }

        public static string Comment => SemanticVersion.Prerelease;

        public static SemVersion SemanticVersion { get; }

        public static Version Version { get; }
    }
}

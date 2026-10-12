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

            PackageVersion = ToPackageVersion(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);
        }

        /// <summary>
        /// The package version including any pre-release suffix (e.g. 2.0.0-beta.3), without build metadata.
        /// Used as the package manifest version, which Umbraco 17.6+ uses to cache-bust /App_Plugins asset URLs.
        /// </summary>
        public static string PackageVersion { get; }

        public static string ToPackageVersion(string informationalVersion) => informationalVersion.Split('+')[0];

        public static Version? AssemblyVersion { get; }

        public static Version AssemblyFileVersion { get; }

        public static string Comment => SemanticVersion.Prerelease;

        public static SemVersion SemanticVersion { get; }

        public static Version Version { get; }
    }
}

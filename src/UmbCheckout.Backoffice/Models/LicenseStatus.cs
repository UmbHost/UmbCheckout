using UmbHost.Licensing.Models;

namespace UmbCheckout.Backoffice.Models
{
    internal class LicenseStatus
    {
        public string ExpiryDateTime { get; internal set; } = string.Empty;
        public string RegDate { get; set; } = string.Empty;
        public string Status { get; set; } = "Invalid";
        public string ValidDomains { get; internal set; } = string.Empty;
        public string ValidPaths { get; internal set; } = string.Empty;
        public IEnumerable<LicenseAddon> LicenseAddons { get; set; } = Enumerable.Empty<LicenseAddon>();
    }
}

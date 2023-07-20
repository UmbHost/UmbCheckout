namespace UmbCheckout.Backoffice.Models
{
    internal class LicenseStatus
    {
        public string ExpiryDateTime { get; internal set; } = string.Empty;
        public string RegDate { get; set; } = string.Empty;
        public string Status { get; set; } = "Invalid";
    }
}

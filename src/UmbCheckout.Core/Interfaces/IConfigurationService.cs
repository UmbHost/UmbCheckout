namespace UmbCheckout.Core.Interfaces
{
    /// <summary>
    /// A service which Gets or Updates the configuration
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the configuration
        /// </summary>
        /// <returns>The configuration</returns>
        Task<Shared.Models.UmbCheckoutConfiguration?> GetConfiguration();

        /// <summary>
        /// Updates the configuration
        /// </summary>
        /// <param name="configuration">The configuration</param>
        /// <returns>true if update was successful false if not</returns>
        Task<bool> UpdateConfiguration(Shared.Models.UmbCheckoutConfiguration configuration);
    }
}

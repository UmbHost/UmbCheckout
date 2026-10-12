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
        /// <returns>The configuration, or null before it is first saved</returns>
        Task<Shared.Models.UmbCheckoutConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts or updates the configuration
        /// </summary>
        /// <param name="configuration">The configuration</param>
        /// <returns>true when saved</returns>
        Task<bool> UpdateConfigurationAsync(Shared.Models.UmbCheckoutConfiguration configuration, CancellationToken cancellationToken = default);
    }
}

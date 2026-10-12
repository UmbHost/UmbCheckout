# UmbCheckout

UmbCheckout is an eCommerce solution for Umbraco which integrates with Stripe Checkout

## Getting started

Only long term support (LTS) versions for Umbraco are officially supported.

Community contributions for short term support versions (STS) are welcome.

Currently it has been officially tested against Umbraco v10 and v13.

## Documentation

The documentation can be found on the docs site:

[https://docs.umbcheckout.net](https://docs.umbcheckout.net)

## Stripe webhook

Add a webhook endpoint in Stripe pointing at `https://<your site>/umbraco/api/stripewebhookapi/checkoutevents`, and put its signing secret in configuration as `UmbCheckout:Stripe:Test:WebHookSecret` (and `UmbCheckout:Stripe:Live:WebHookSecret` for live mode), in user secrets or environment variables, never in a committed settings file. Without the secret for the active mode, UmbCheckout logs an error and answers 500 so Stripe keeps retrying.

UmbCheckout uses Stripe.net 50.x, which is pinned to Stripe API version `2026-01-28.clover`. Create the endpoint on that version where you can (the Stripe API and CLI accept an `api_version`; the Dashboard may only offer your account's default). Events on another version are still handled, with a warning in the log once per version.

Stripe delivers each event at least once and retries for up to 3 days. Once your notification handlers have completed for an event, a redelivery is acknowledged without running them again. If a handler throws, or the server stops while handlers are running, Stripe's retry runs the handlers again (all of them), so write handlers that are safe to repeat.

## Extending UmbCheckout

UmbCheckout publishes Umbraco notifications around its data, so a site can change or refuse data going in and change data coming out, without changing UmbCheckout. Register handlers as usual, e.g. `builder.AddNotificationAsyncHandler<OnShippingRateSavingNotification, MyHandler>()` in a composer.

| Data | Before it is saved (change it, or cancel) | After it is read (change what is returned) |
|---|---|---|
| Configuration | `OnConfigurationSavingNotification` | `OnConfigurationLoadedNotification` |
| Stripe settings | `OnStripeSettingsSavingNotification` | `OnStripeSettingsLoadedNotification` |
| Shipping rates | `OnShippingRateSavingNotification` (create and update), `OnShippingRateDeletingNotification` | `OnShippingRatesLoadedNotification` |
| Tax rates (licensed add-on) | `OnStripeTaxRateSavingNotification`, `OnStripeTaxRateDeletingNotification` | `OnStripeTaxRatesLoadedNotification` |
| Basket | `OnSessionSavingNotification` | `OnSessionLoadedNotification` |

Saving notifications carry the object about to be saved (`Target`) and, for an update, the stored one (`Existing`). Loaded notifications carry the object (`Target`) or, for shipping and tax rates, the list about to be returned (`Items`). Remove a rate from `Items` to hide it from lists and single reads alike (checkout and shop code); editors still see and manage it in the backoffice. The existing `…Saved`, `…Deleted` and session notifications are unchanged.

Refuse a save or delete with a message for the person who tried:

```csharp
public sealed class StripeShippingRateIds : INotificationAsyncHandler<OnShippingRateSavingNotification>
{
    public Task HandleAsync(OnShippingRateSavingNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.Target.Value.StartsWith("shr_", StringComparison.Ordinal))
        {
            notification.CancelOperation(new EventMessage("Shipping", "Stripe shipping rate IDs start with shr_", EventMessageType.Error));
        }

        return Task.CompletedTask;
    }
}
```

Nothing is saved, and the message is shown in the backoffice error toast. On the shop, a cancelled basket change leaves the basket as it was and puts the message in `TempData[UmbCheckout.Shared.Consts.TempData.UmbCheckoutBasketChangeCancelled]` for your views to show.

Change what is read:

```csharp
public sealed class HideInternalShippingRates : INotificationAsyncHandler<OnShippingRatesLoadedNotification>
{
    public Task HandleAsync(OnShippingRatesLoadedNotification notification, CancellationToken cancellationToken)
    {
        notification.Items.RemoveAll(rate => rate.Name.StartsWith("Internal", StringComparison.Ordinal));
        return Task.CompletedTask;
    }
}
```

Rules:

- Cancel with `CancelOperation(...)` and an `Error` or `Warning` event message; that message is what the person sees. `notification.Cancel = true` also cancels, with a generic message.
- Loaded changes are for what is shown and handed to code; they are never stored. UmbCheckout reads stored data, without Loaded, wherever it changes or charges data:
  - basket changes and checkout (checkout charges the stored basket, with every unit price re-priced from published content);
  - the backoffice screens that edit configuration, Stripe settings, shipping rates and tax rates;
  - uSync exports and imports.
- Your own code can do the same with `using (UmbCheckoutHooks.SuppressLoaded()) { ... }`.
- Checkout records its Stripe session on the basket inside `UmbCheckoutHooks.SuppressSaving()`, so a basket Saving handler can't stop the paid basket being cleared.
- Loaded handlers run on every other read, including frequent internal ones, so keep them cheap.
- A handler that throws fails the operation; UmbCheckout doesn't hide handler errors.

## Pricing comparison

UmbCheckout can be used for FREE forever with the basket stored in .NET sessions, for a small fee of £50 you can purchase a license which unlocks cookie and database storing of the database, as well as all existing / future addon features and lifetime updates.

Technical support is included in the first year of a paid licence and can be renewed for £50 per year thereafter.

| FREE | Paid | Hosted |
|------|--------|--------|
| [Free forever](https://github.com/UmbHost/UmbCheckout) | [One off payment of £50](https://umbcheckout.net) | [Included with hosting from £10 per month](https://umbhost.net/hosting/cloud-umbraco-hosting) |
| Stripe & community payment providers | All official payment providers | All official payment providers | 
| Unlimited updates | Install on one Umbraco instance | Install on one Umbraco instance |
| Basket stored in session | Basket stored in session | Basket stored in session |
| Community supported | Basket stored in a cookie | Basket stored in a cookie |
|        | Basket stored in a database | Basket stored in a database |
|        | All addon features | All addon features |
|        | 12 months support desk access, £50 per year thereafter | Ongoing support desk access |

[Licenses can be purchased via UmbHost Limited here](https://umbcheckout.net) alternatively the license is included with all hosting packages purchased via UmbHost limited, you can [compare the packages here](https://umbhost.net/hosting/cloud-umbraco-hosting), licenses which with hosting include ongoing technical support for the lifetime of the hosting subscription.

## Addon features

There are a number of addon features available with a paid license.

* Store basket in the database
* Store basket in a cookie
* Database and cookie notifications
* Removed expired baskets from database scheduled tasks
* Technical support via a support desk from Umbraco Certified Master developers

### Payment provider addon features

* Stripe tax rate support
* All future official payment providers including PayPal and Klarna

## Getting started with the TestSite

`src/UmbCheckout.TestSite` is an Umbraco 17 site (The Starter Kit) with the open-source UmbCheckout packages, on SQLite at https://localhost:44381.

On first run it installs itself and imports its content with uSync. Set the backoffice login it creates with user secrets first:

```bash
cd src/UmbCheckout.TestSite
dotnet user-secrets set "Umbraco:CMS:Unattended:UnattendedUserName" "<name>"
dotnet user-secrets set "Umbraco:CMS:Unattended:UnattendedUserEmail" "<email>"
dotnet user-secrets set "Umbraco:CMS:Unattended:UnattendedUserPassword" "<password>"
dotnet run
```

The install writes an `Imaging:HMACSecretKey` into `appsettings.json`; discard that change. Stripe keys go in user secrets too, never in the settings files.

UmbHost.Licensing is closed source. Inside the full UmbCheckout checkout the build uses the project next to this repository; cloned on its own it uses the `UmbHost.Licensing` NuGet package at the version in `Directory.Build.props` (`-p:UseUmbLicensingProject=false` forces the package). The lock files are written in project mode.

### Contribution guidelines

You will need to sign the Contribution License Agreement which can be found at the link below:

[https://cla-assistant.io/UmbHost/UmbCheckout](https://cla-assistant.io/UmbHost/UmbCheckout)

To raise a new bug, create an issue on the GitHub repository. To fix a bug or add new features, fork the repository and send a pull request with your changes. Feel free to add ideas to the repository's issues list if you would to discuss anything related to the package.

### Who do I talk to?
This project is maintained by [UmbHost Limited](https://umbhost.net) and contributors. If you have any questions about the project please raise an issue on GitHub.

## License

Copyright &copy; 2023 [UmbHost Limited](https://umbhost.net), and other contributors

Licensed under the Apache License 2.0 license

# UmbCheckout

UmbCheckout is an eCommerce solution for Umbraco which integrates with Stripe Checkout

## Getting started

Only long term support (LTS) versions for Umbraco are officially supported.

Community contributions for short term support versions (STS) are welcome.

This version targets Umbraco v17 (LTS) on .NET 10. Umbraco v10 - v13 are supported by the previous major version of UmbCheckout.

## Documentation

The documentation can be found on the docs site:

[https://docs.umbcheckout.net](https://docs.umbcheckout.net)

## Pricing comparison

UmbCheckout can be used for FREE forever with the basket stored in .NET sessions, for a small fee of £50 you can purchase a license which unlocks cookie and database storing of the database, as well as all existing / future addon features and lifetime updates.

Technical support is included in the first year of a paid licence and can be renewed for £50 per year thereafter.

| FREE | Paid | Hosted |
|------|--------|--------|
| [Free forever](https://github.com/UmbHost/UmbCheckout) | [One off payment of £50](https://my.umbhost.net/store/umbraco-packages) | [Included with hosting from £10 per month](https://umbhost.net/hosting/cloud-umbraco-hosting) |
| Stripe & community payment providers | All official payment providers | All official payment providers | 
| Unlimited updates | Install on one Umbraco instance | Install on one Umbraco instance |
| Basket stored in session | Basket stored in session | Basket stored in session |
| Community supported | Basket stored in a cookie | Basket stored in a cookie |
|        | Basket stored in a database | Basket stored in a database |
|        | All addon features | All addon features |
|        | 12 months support desk access, £50 per year thereafter | Ongoing support desk access |

[Licenses can be purchased via UmbHost Limited here](https://my.umbhost.net/store/umbraco-packages) alternatively the license is included with all hosting packages purchased via UmbHost limited, you can [compare the packages here](https://umbhost.net/hosting/cloud-umbraco-hosting), licenses which with hosting include ongoing technical support for the lifetime of the hosting subscription.

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

The test site is the [UmbCheckout Starter Kit for Stripe](https://github.com/UmbHost/UmbCheckout.StarterKit.Stripe), included as a git submodule in `src/UmbCheckout.StarterKit.Stripe`. When checked out inside this repository it references the UmbCheckout projects directly instead of the NuGet packages.

```bash
git submodule update --init --recursive
```

Run `src/UmbCheckout.StarterKit.Stripe/UmbCheckout.StarterKit.Web` (uses SQLite). The starter kit content is imported by uSync on first boot. The UmbCheckout configuration can't be imported until the UmbCheckout tables exist, so run Import Everything using uSync once the site has been installed.

Stripe API keys for testing can be added to `appsettings.Local.json` (git ignored) under `UmbCheckout:Stripe`.

## Acceptance tests

Playwright tests covering the backoffice pages, the basket and Stripe Checkout against the starter kit live in `tests/UmbCheckout.AcceptanceTests`, see its [README](tests/UmbCheckout.AcceptanceTests/README.md).

## Building the backoffice

The backoffice UI is built with Lit, TypeScript and Vite against `@umbraco-cms/backoffice`. The compiled output is committed to `wwwroot`, so you only need to rebuild it when changing the client source:

```bash
cd src/UmbCheckout.Backoffice/Client
npm install
npm run build
```

The Stripe payment provider has its own client in `src/UmbCheckout.Stripe/Client`, which is built the same way. Use `npm run watch` to rebuild on changes.

### Contribution guidelines

You will need to sign the Contribution License Agreement which can be found at the link below:

[https://cla-assistant.io/UmbHost/UmbCheckout](https://cla-assistant.io/UmbHost/UmbCheckout)

To raise a new bug, create an issue on the GitHub repository. To fix a bug or add new features, fork the repository and send a pull request with your changes. Feel free to add ideas to the repository's issues list if you would to discuss anything related to the package.

### Who do I talk to?
This project is maintained by [UmbHost Limited](https://umbhost.net) and contributors. If you have any questions about the project please raise an issue on GitHub.

## License

Copyright &copy; 2023 [UmbHost Limited](https://umbhost.net), and other contributors

Licensed under the Apache License 2.0 license

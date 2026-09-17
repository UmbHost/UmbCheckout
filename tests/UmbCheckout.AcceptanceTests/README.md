# UmbCheckout acceptance tests

Playwright browser tests that run against a **real, running** [UmbCheckout Starter Kit for Stripe](../../src/UmbCheckout.StarterKit.Stripe)
site, which references the UmbCheckout projects in this repository directly.

## Running locally

**1. Start the starter kit** (see the root README), and make sure the UmbCheckout configuration has been imported
(uSync > Import Everything, or restart with `uSync__Settings__ImportAtStartup=All`).

**2. For the Stripe Checkout tests,** add a Stripe **test** secret key to `appsettings.Local.json` under
`UmbCheckout:Stripe:Test:ApiKey`. The Stripe account must contain the shipping rate from
`uSync/v17/UmbCheckoutStripeShippingRates/test.config`, otherwise Stripe refuses to create the session.

**3. Configure and run the tests.** Copy `.env.example` to `.env` and fill in the HTTPS URL and a superadmin login.

```bash
cd tests/UmbCheckout.AcceptanceTests && npm ci && npx playwright install chromium && npx playwright test
```

Use `npx playwright test --ui` while writing or debugging selectors, and `npx playwright show-report` for the last report.

## In CI

`.github/workflows/acceptance-tests.yml` publishes the starter kit, boots it with an unattended install on SQLite
and runs this suite. The Stripe Checkout tests run when the `STRIPE_TEST_API_KEY` repository secret is set; secrets
are not passed to pull requests from forks, so they are skipped there.

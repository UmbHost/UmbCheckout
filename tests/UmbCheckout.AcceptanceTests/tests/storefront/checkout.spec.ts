import { test, expect, type Page } from '@playwright/test';
import { Storefront } from '../../helpers/storefront';
import { Page as SitePage, Product } from '../../helpers/umbCheckoutConstants';

const { banjo, pingPongBall } = Product;

test.describe('Stripe Checkout', () => {
  test.skip(
    process.env.STRIPE_CONFIGURED !== 'true',
    'Set STRIPE_CONFIGURED=true once the site has a Stripe test API key (UmbCheckout:Stripe:Test:ApiKey)',
  );

  test.setTimeout(3 * 60 * 1000);

  test('paying with a test card returns to the success page with an empty basket', async ({ page }) => {
    const storefront = new Storefront(page);
    await storefront.addToBasket(banjo.url);
    await storefront.addToBasket(pingPongBall.url, { quantity: 2 });
    await storefront.gotoBasket();

    await storefront.checkout();

    await expect(page).toHaveURL(/checkout\.stripe\.com/, { timeout: 30_000 });
    await expect(page.getByText(banjo.name).first()).toBeVisible({ timeout: 30_000 });
    await expect(page.getByText(pingPongBall.name).first()).toBeVisible();

    await fillStripeCheckout(page, '4242424242424242');
    await page.getByTestId('hosted-payment-submit-button').or(page.locator('button.SubmitButton')).first().click();

    await expect(page).toHaveURL(new RegExp(`${SitePage.successPage}\\?session_id=cs_test_[^&]+&success=true`), {
      timeout: 60_000,
    });

    await storefront.gotoBasket();
    await storefront.expectEmpty();
  });

  test('a declined card keeps the customer on Stripe Checkout', async ({ page }) => {
    const storefront = new Storefront(page);
    await storefront.addToBasket(banjo.url);
    await storefront.gotoBasket();
    await storefront.checkout();
    await expect(page).toHaveURL(/checkout\.stripe\.com/, { timeout: 30_000 });

    await fillStripeCheckout(page, '4000000000000002');
    await page.getByTestId('hosted-payment-submit-button').or(page.locator('button.SubmitButton')).first().click();

    await expect(page.getByText(/card (was|has been) declined/i).first()).toBeVisible({ timeout: 30_000 });
    await expect(page).toHaveURL(/checkout\.stripe\.com/);

    await storefront.gotoBasket();
    await storefront.expectQuantity(banjo.name, 1);
  });

  test('cancelling on Stripe Checkout returns to the cancel page and keeps the basket', async ({ page }) => {
    const storefront = new Storefront(page);
    await storefront.addToBasket(banjo.url);
    await storefront.gotoBasket();
    await storefront.checkout();
    await expect(page).toHaveURL(/checkout\.stripe\.com/, { timeout: 30_000 });

    // The "back" link in the Stripe Checkout header goes to the session's cancel_url
    await page.locator(`a[href*="${SitePage.cancelPage}"]`).first().click();

    await expect(page).toHaveURL(new RegExp(`${SitePage.cancelPage}\\?session_id=cs_test_[^&]+&success=false`));
    await storefront.gotoBasket();
    await storefront.expectQuantity(banjo.name, 1);
  });
});

async function fillStripeCheckout(page: Page, cardNumber: string) {
  await page.locator('#email').fill('acceptance@umbcheckout.local');

  const cardAccordion = page.getByTestId('card-accordion-item-button');
  if (await cardAccordion.isVisible().catch(() => false)) {
    await cardAccordion.click();
  }

  await page.locator('#cardNumber').fill(cardNumber);
  await page.locator('#cardExpiry').fill('12 / 34');
  await page.locator('#cardCvc').fill('123');

  const phoneNumber = page.locator('#phoneNumber');
  if (await phoneNumber.isVisible().catch(() => false)) {
    const phoneCountry = page.getByRole('combobox', { name: 'Phone number country code' });
    if (await phoneCountry.isVisible().catch(() => false)) {
      await phoneCountry.selectOption('NL');
    }
    await phoneNumber.fill('0612345678');
  }

  for (const prefix of ['shipping', 'billing']) {
    const country = page.locator(`#${prefix}Country`);
    if (!(await country.isVisible().catch(() => false))) {
      continue;
    }

    await fillIfVisible(page, `#${prefix}Name`, 'Acceptance Tests');
    await country.selectOption('NL');

    const manualEntry = page
      .getByRole('group')
      .filter({ has: country })
      .locator('.AddressAutocomplete-manual-entry button')
      .first();
    if (await manualEntry.isVisible().catch(() => false)) {
      await manualEntry.click();
      await expect(page.locator(`#${prefix}AddressLine1`)).not.toHaveAttribute('role', 'combobox');
    }

    await fillIfVisible(page, `#${prefix}AddressLine1`, 'Teststraat 1');
    await fillIfVisible(page, `#${prefix}PostalCode`, '1234 AB');
    await fillIfVisible(page, `#${prefix}Locality`, 'Amsterdam');
  }

  await fillIfVisible(page, '#billingName', 'Acceptance Tests');

  const enableLink = page.locator('#enableStripePass');
  if ((await enableLink.isVisible().catch(() => false)) && (await enableLink.isChecked())) {
    await enableLink.uncheck();
  }

}

async function fillIfVisible(page: Page, selector: string, value: string) {
  const field = page.locator(selector);
  if ((await field.isVisible().catch(() => false)) && (await field.inputValue()) === '') {
    await field.fill(value);
  }
}

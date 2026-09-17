import { test, expect } from '../../helpers/test';
import { ApiRoute, MENU_ITEMS, SETTINGS_SECTION_URL } from '../../helpers/umbCheckoutConstants';

test.describe('UmbCheckout backoffice', () => {
  test('the management API endpoints respond', async ({ umbracoApi }) => {
    for (const route of Object.values(ApiRoute)) {
      const response = await umbracoApi.get(route);
      expect(response.status(), `GET ${route}`).toBe(200);
    }
  });

  test('the UmbCheckout menu is registered in the Settings section', async ({ umbracoUi, page }) => {
    await umbracoUi.goToBackOffice();
    await page.goto(SETTINGS_SECTION_URL);

    const sidebar = page.locator('umb-section-sidebar');
    await expect(sidebar.getByText('UmbCheckout', { exact: true })).toBeVisible();
    for (const item of MENU_ITEMS) {
      await expect(sidebar.getByRole('link', { name: item.label, exact: true })).toBeVisible();
    }
  });

  for (const item of MENU_ITEMS) {
    test(`the ${item.label} page loads`, async ({ umbracoUi, page }) => {
      await umbracoUi.goToBackOffice();
      await page.goto(SETTINGS_SECTION_URL);

      await page.locator('umb-section-sidebar').getByRole('link', { name: item.label, exact: true }).click();

      const workspace = page.locator(item.element);
      await expect(workspace, `${item.label} should render <${item.element}>`).toBeVisible();

      // Each page shows a loader until its data has been fetched, then renders its boxes.
      await expect(workspace.locator('uui-loader')).toHaveCount(0);
      await expect(workspace.locator('uui-box').first()).toBeVisible();
    });
  }

  test('the Configuration page shows the imported configuration', async ({ umbracoUi, page }) => {
    await umbracoUi.goToBackOffice();
    await page.goto(SETTINGS_SECTION_URL);
    await page.locator('umb-section-sidebar').getByRole('link', { name: 'Configuration', exact: true }).click();

    // Imported from uSync/v17/UmbCheckout/UmbCheckoutConfiguration.config
    const workspace = page.locator('umbcheckout-configuration-workspace');
    await expect(workspace.getByRole('link', { name: 'Success Page /success-page/' })).toBeVisible();
    await expect(workspace.getByRole('link', { name: 'Cancel Page /cancel-page/' })).toBeVisible();
  });

  test('the Stripe Shipping Rates page lists the imported shipping rate', async ({ umbracoUi, page }) => {
    await umbracoUi.goToBackOffice();
    await page.goto(SETTINGS_SECTION_URL);
    await page.locator('umb-section-sidebar').getByRole('link', { name: 'Stripe Shipping Rates', exact: true }).click();

    // Imported from uSync/v17/UmbCheckoutStripeShippingRates/test.config
    const list = page.locator('umbcheckout-stripe-shipping-rates-list');
    await expect(list.locator('uui-table-row').filter({ hasText: 'Test' })).toBeVisible();
  });
});

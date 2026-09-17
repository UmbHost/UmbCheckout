import { test as setup, expect } from '@playwright/test';
import { ApiHelpers } from '@umbraco-cms/acceptance-test-helpers';
import { ApiRoute } from '../helpers/umbCheckoutConstants';

const storageState = process.env.STORAGE_STATE_PATH ?? 'playwright/.auth/user.json';

setup('authenticate as admin', async ({ page }) => {
  expect(
    process.env.UMBRACO_USER_LOGIN,
    'UMBRACO_USER_LOGIN is not set - copy .env.example to .env (see README).',
  ).toBeTruthy();

  const umbracoApi = new ApiHelpers(page);
  await umbracoApi.loginToAdminUser();

  const response = await umbracoApi.get(ApiRoute.configuration);
  expect(response.status(), 'Could not reach the UmbCheckout management API as an authenticated admin.').toBe(200);

  await page.context().storageState({ path: storageState });
});

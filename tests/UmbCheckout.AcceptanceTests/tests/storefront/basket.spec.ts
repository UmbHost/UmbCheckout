import { test, expect } from '@playwright/test';
import { Storefront } from '../../helpers/storefront';
import { Page, Product } from '../../helpers/umbCheckoutConstants';

const { banjo, pingPongBall, bikerJacket } = Product;

test.describe('Basket', () => {
  test('a new visitor has an empty basket', async ({ page }) => {
    const storefront = new Storefront(page);

    await storefront.gotoBasket();

    await storefront.expectEmpty();
    await storefront.expectSubTotal(0);
  });

  test('adding products puts them in the basket', async ({ page }) => {
    const storefront = new Storefront(page);

    await storefront.addToBasket(banjo.url);
    await storefront.addToBasket(pingPongBall.url, { quantity: 3 });

    await storefront.gotoBasket();
    await expect(storefront.lineItems).toHaveCount(2);
    await storefront.expectQuantity(banjo.name, 1);
    await storefront.expectQuantity(pingPongBall.name, 3);
    await storefront.expectSubTotal(banjo.price + 3 * pingPongBall.price);
  });

  test('adding the same product again increases its quantity', async ({ page }) => {
    const storefront = new Storefront(page);

    await storefront.addToBasket(banjo.url);
    await storefront.addToBasket(banjo.url, { quantity: 2 });

    await storefront.gotoBasket();
    await expect(storefront.lineItems).toHaveCount(1);
    await storefront.expectQuantity(banjo.name, 3);
  });

  test('a product variant is added as its own line item', async ({ page }) => {
    const storefront = new Storefront(page);

    await storefront.addToBasket(bikerJacket.url, { variant: bikerJacket.variant });

    await storefront.gotoBasket();
    // Variants are rendered as "<parent> - <variant>"
    await expect(storefront.lineItems).toHaveCount(1);
    await storefront.expectQuantity(`${bikerJacket.name} - ${bikerJacket.variant}`, 1);
    await storefront.expectSubTotal(bikerJacket.price);
  });

  test('the basket link in the navigation shows the sub total', async ({ page }) => {
    const storefront = new Storefront(page);

    await storefront.addToBasket(banjo.url);

    await expect(page.locator('nav a[href="/basket/"]').first()).toContainText(/399[.,]00/);
  });

  test('the + and - buttons change the quantity', async ({ page }) => {
    const storefront = new Storefront(page);
    await storefront.addToBasket(banjo.url);
    await storefront.gotoBasket();

    await storefront.increase(banjo.name);
    await storefront.expectQuantity(banjo.name, 2);
    await storefront.expectSubTotal(2 * banjo.price);

    await storefront.increase(banjo.name);
    await storefront.expectQuantity(banjo.name, 3);

    await storefront.reduce(banjo.name);
    await storefront.expectQuantity(banjo.name, 2);
    await storefront.expectSubTotal(2 * banjo.price);
  });

  test('reducing a line item with quantity 1 removes it', async ({ page }) => {
    const storefront = new Storefront(page);
    await storefront.addToBasket(banjo.url);
    await storefront.gotoBasket();

    await storefront.reduce(banjo.name);

    await storefront.expectEmpty();
  });

  test('the remove button removes only that line item', async ({ page }) => {
    const storefront = new Storefront(page);
    await storefront.addToBasket(banjo.url, { quantity: 2 });
    await storefront.addToBasket(pingPongBall.url);
    await storefront.gotoBasket();

    await storefront.remove(banjo.name);

    await expect(storefront.lineItems).toHaveCount(1);
    await expect(storefront.lineItem(banjo.name)).toHaveCount(0);
    await storefront.expectQuantity(pingPongBall.name, 1);
    await storefront.expectSubTotal(pingPongBall.price);
  });

  test('empty basket removes all line items', async ({ page }) => {
    const storefront = new Storefront(page);
    await storefront.addToBasket(banjo.url);
    await storefront.addToBasket(pingPongBall.url);
    await storefront.gotoBasket();
    await expect(storefront.lineItems).toHaveCount(2);

    await storefront.emptyBasket();

    await expect(page).toHaveURL(new RegExp(`${Page.basket}$`));
    await storefront.expectEmpty();
    await storefront.expectSubTotal(0);
  });

  test('the basket is kept per visitor', async ({ page, browser }) => {
    const storefront = new Storefront(page);
    await storefront.addToBasket(banjo.url);

    const otherContext = await browser.newContext({ ignoreHTTPSErrors: true });
    try {
      const otherVisitor = new Storefront(await otherContext.newPage());
      await otherVisitor.gotoBasket();
      await otherVisitor.expectEmpty();
    } finally {
      await otherContext.close();
    }
  });

  test('checking out an empty basket stays on the basket page', async ({ page }) => {
    const storefront = new Storefront(page);
    await storefront.gotoBasket();

    await storefront.checkout();

    await expect(page).toHaveURL(new RegExp(`${Page.basket}$`));
    await storefront.expectEmpty();
  });
});

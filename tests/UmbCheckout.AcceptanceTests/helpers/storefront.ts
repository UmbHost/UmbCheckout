import { expect, type Locator, type Page } from '@playwright/test';
import { Page as SitePage } from './umbCheckoutConstants';

export class Storefront {
  constructor(private readonly page: Page) {}

  async addToBasket(productUrl: string, options: { quantity?: number; variant?: string } = {}) {
    await this.page.goto(productUrl);

    if (options.quantity !== undefined) {
      await this.page.locator('input[name="quantity"]').fill(String(options.quantity));
    }
    if (options.variant !== undefined) {
      const select = this.page.locator('select[name="key"]');
      const value = await select
        .locator('option', { hasText: options.variant })
        .getAttribute('value');
      await select.selectOption(value!);
    }

    await this.page.getByRole('button', { name: 'Add to Basket' }).click();
    await expect(this.page.locator('.alert-success')).toContainText('to basket');
  }

  async gotoBasket() {
    await this.page.goto(SitePage.basket);
  }

  get lineItems(): Locator {
    return this.page.locator('table tbody tr').filter({ has: this.page.locator('th[scope="row"]') });
  }

  lineItem(name: string): Locator {
    return this.lineItems.filter({ has: this.page.locator('th[scope="row"]', { hasText: name }) });
  }

  async expectQuantity(name: string, quantity: number) {
    await expect(this.lineItem(name).locator('td').nth(1)).toHaveText(String(quantity));
  }

  async expectEmpty() {
    await expect(this.lineItems).toHaveCount(0);
    await expect(this.page.getByText('Your basket is empty!')).toBeVisible();
  }

  async expectSubTotal(amount: number) {
    const [whole, fraction] = amount.toFixed(2).split('.');
    const wholeWithOptionalGrouping = whole.replace(/\B(?=(\d{3})+(?!\d))/g, '[.,\\s\\u00a0]?');
    await expect(this.page.locator('p', { hasText: 'Sub Total:' })).toHaveText(
      new RegExp(`Sub Total:.*\\b${wholeWithOptionalGrouping}[.,]${fraction}\\b`),
    );
  }

  async increase(name: string) {
    await this.lineItem(name).getByRole('button', { name: '+' }).click();
  }

  async reduce(name: string) {
    await this.lineItem(name).getByRole('button', { name: '-' }).click();
  }

  async remove(name: string) {
    await this.lineItem(name).getByRole('button', { name: 'Remove' }).click();
  }

  async emptyBasket() {
    await this.page.getByRole('link', { name: 'Empty basket' }).click();
  }

  async checkout() {
    await this.page.getByRole('button', { name: 'Checkout' }).click();
  }
}

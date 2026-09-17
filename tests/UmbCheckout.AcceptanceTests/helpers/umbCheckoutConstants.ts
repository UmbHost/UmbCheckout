/**
 * Values mirrored from the package and starter kit source. Update both together.
 */

/** Management API routes, see UmbCheckoutManagementApiControllerBase / UmbCheckoutStripeManagementApiControllerBase */
export const ApiRoute = {
  configuration: '/umbraco/umbcheckout/api/v1/configuration',
  stripeSettings: '/umbraco/umbcheckout/api/v1/stripe/settings',
  stripeShippingRates: '/umbraco/umbcheckout/api/v1/stripe/shipping-rates',
} as const;

export const SETTINGS_SECTION_URL = '/umbraco/section/settings';

/**
 * The menu items in the "UmbCheckout" group of the Settings section sidebar, and the element
 * each one renders. Labels come from the localization files, elements from the @customElement decorators.
 */
export const MENU_ITEMS = [
  { label: 'Configuration', element: 'umbcheckout-configuration-workspace' },
  { label: 'Stripe Settings', element: 'umbcheckout-stripe-settings-workspace' },
  { label: 'Stripe Shipping Rates', element: 'umbcheckout-stripe-shipping-rates-list' },
] as const;

/** Starter kit content, imported by uSync (uSync/v17/Content) */
export const Page = {
  basket: '/basket/',
  successPage: '/success-page/',
  cancelPage: '/cancel-page/',
} as const;

export const Product = {
  /** A simple product, 399.00 */
  banjo: { url: '/shop/banjo/', name: 'Banjo', price: 399 },
  /** A simple product, 2.00 */
  pingPongBall: { url: '/shop/ping-pong-ball/', name: 'Ping Pong Ball', price: 2 },
  /** A product with a "Red" variant, 199.00 */
  bikerJacket: { url: '/shop/biker-jacket/', name: 'Biker Jacket', price: 199, variant: 'Red' },
} as const;

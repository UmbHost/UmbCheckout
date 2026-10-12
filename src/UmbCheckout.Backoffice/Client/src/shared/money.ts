import type { UmbLocalizationController } from "@umbraco-cms/backoffice/localization-api";

/** A server-converted amount: `amount` is already in major units for `currency` (see StripeCurrency on the server). */
export type UmbCheckoutMoney = { amount: number; currency: string };

/** Formats money in the user's culture. Never divides by a hard-coded 100: the server has already converted it. */
export const formatMoney = (localize: UmbLocalizationController, money: UmbCheckoutMoney | undefined | null) =>
  money ? localize.number(money.amount, { style: "currency", currency: money.currency.toUpperCase() }) : "";

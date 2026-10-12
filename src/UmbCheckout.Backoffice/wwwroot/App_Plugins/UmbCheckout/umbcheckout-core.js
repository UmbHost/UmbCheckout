import { c as o, w as E } from "./api-client-DCzFYg7y.js";
import { a as c, b as s, U as T, c as _, d as m } from "./constants-BP4MpHU2.js";
import { U as A } from "./license.context-token-4gXpqREX.js";
import { U as S, a as u } from "./unlicensed-alert.element-BxElw7Mq.js";
import { U as l } from "./made-by.element-BHDU3_qS.js";
const C = (U, e) => e ? U.number(e.amount, { style: "currency", currency: e.currency.toUpperCase() }) : "";
export {
  A as UMBCHECKOUT_LICENSE_CONTEXT,
  c as UMBCHECKOUT_SECTION_ALIAS,
  s as UMBCHECKOUT_SECTION_PATHNAME,
  T as UMBCHECKOUT_SETTINGS_MENU_ALIAS,
  _ as UMBCHECKOUT_STORE_MENU_ALIAS,
  m as UMBCHECKOUT_STRIPE_MENU_ALIAS,
  S as UmbCheckoutAlertStyles,
  l as UmbCheckoutMadeByElement,
  u as UmbCheckoutUnlicensedAlertElement,
  o as configureUmbCheckoutClient,
  C as formatMoney,
  E as whenUmbCheckoutClientReady
};
//# sourceMappingURL=umbcheckout-core.js.map

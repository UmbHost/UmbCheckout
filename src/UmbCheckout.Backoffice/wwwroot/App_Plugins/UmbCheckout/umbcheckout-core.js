import { c as B, w as R } from "./api-client-DCzFYg7y.js";
import { a as H, b as L, U as v, c as K, d as q } from "./constants-BP4MpHU2.js";
import { U as X } from "./license.context-token-4gXpqREX.js";
import { U as F, a as P } from "./unlicensed-alert.element-BFU93TIj.js";
import { U as V } from "./made-by.element-BHDU3_qS.js";
import { UMB_ACTION_EVENT_CONTEXT as l } from "@umbraco-cms/backoffice/action";
import { UmbEntityActionBase as E, UmbRequestReloadStructureForEntityEvent as h, UmbEntityDeletedEvent as C } from "@umbraco-cms/backoffice/entity-action";
import { createExtensionApiByAlias as r } from "@umbraco-cms/backoffice/extension-registry";
import { html as U } from "@umbraco-cms/backoffice/external/lit";
import { UmbLocalizationController as u } from "@umbraco-cms/backoffice/localization-api";
import { umbConfirmModal as f } from "@umbraco-cms/backoffice/modal";
import { UMB_NOTIFICATION_CONTEXT as p } from "@umbraco-cms/backoffice/notification";
const M = (i, t) => t ? i.number(t.amount, { style: "currency", currency: t.currency.toUpperCase() }) : "";
function O(i, t) {
  return class extends E {
    #t = new u(this);
    async execute() {
      const e = this.args.unique;
      if (!e) throw new Error("Cannot delete an item without a unique identifier.");
      const s = await r(this, this.args.meta.itemRepositoryAlias), { data: m } = await s.requestItems([e]), o = m?.[0];
      if (!o) throw new Error("Item not found.");
      await f(this, {
        headline: this.args.meta.confirm?.headline ?? "#actions_delete",
        content: U`${this.#t.htmlString(this.args.meta.confirm?.message ?? "#defaultdialogs_confirmdelete", o.name)}`,
        color: "danger",
        confirmLabel: "#general_delete"
      });
      const c = await r(this, this.args.meta.detailRepositoryAlias), { error: a } = await c.delete(e);
      if (a) throw a;
      const n = await this.getContext(l);
      n?.dispatchEvent(new h({ unique: e, entityType: this.args.entityType })), n?.dispatchEvent(new C({ unique: e, entityType: this.args.entityType })), (await this.getContext(p))?.peek("positive", {
        data: { headline: this.#t.term(i), message: this.#t.term(t) }
      });
    }
  };
}
export {
  X as UMBCHECKOUT_LICENSE_CONTEXT,
  H as UMBCHECKOUT_SECTION_ALIAS,
  L as UMBCHECKOUT_SECTION_PATHNAME,
  v as UMBCHECKOUT_SETTINGS_MENU_ALIAS,
  K as UMBCHECKOUT_STORE_MENU_ALIAS,
  q as UMBCHECKOUT_STRIPE_MENU_ALIAS,
  F as UmbCheckoutAlertStyles,
  V as UmbCheckoutMadeByElement,
  P as UmbCheckoutUnlicensedAlertElement,
  B as configureUmbCheckoutClient,
  O as createUmbCheckoutDeleteAction,
  M as formatMoney,
  R as whenUmbCheckoutClientReady
};
//# sourceMappingURL=umbcheckout-core.js.map

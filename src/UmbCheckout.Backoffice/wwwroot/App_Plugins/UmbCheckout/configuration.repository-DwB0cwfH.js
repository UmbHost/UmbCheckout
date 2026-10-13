import { UmbLocalizationController as o } from "@umbraco-cms/backoffice/localization-api";
import { UMB_NOTIFICATION_CONTEXT as n } from "@umbraco-cms/backoffice/notification";
import { UmbDetailRepositoryBase as c } from "@umbraco-cms/backoffice/repository";
import { tryExecute as r } from "@umbraco-cms/backoffice/resources";
import { C as s } from "./sdk.gen-bktVcCtQ.js";
import { U as u, a as p } from "./bundle.manifests-BTymGh7T.js";
import { UMBCHECKOUT_CONFIGURATION_STORE_CONTEXT as y } from "./configuration.store-BlabXKVF.js";
class C {
  #e;
  constructor(e) {
    this.#e = e;
  }
  async createScaffold() {
    return this.read();
  }
  async read() {
    const { data: e, error: t } = await r(this.#e, s.getConfiguration());
    return t || !e ? { error: t } : { data: {
      entityType: p,
      unique: u,
      currencyCode: e.currencyCode,
      successPageKey: e.successPageKey ?? null,
      cancelPageKey: e.cancelPageKey ?? null,
      enableShipping: e.enableShipping,
      storeBasketInCookie: e.storeBasketInCookie,
      basketInCookieExpiry: e.basketInCookieExpiry,
      storeBasketInDatabase: e.storeBasketInDatabase,
      basketInDatabaseExpiry: e.basketInDatabaseExpiry,
      isLicensed: e.isLicensed
    } };
  }
  async create(e) {
    return this.update(e);
  }
  async update(e) {
    const { error: t } = await r(
      this.#e,
      s.updateConfiguration({
        body: {
          currencyCode: e.currencyCode,
          successPageKey: e.successPageKey,
          cancelPageKey: e.cancelPageKey,
          enableShipping: e.enableShipping,
          storeBasketInCookie: e.storeBasketInCookie,
          basketInCookieExpiry: e.basketInCookieExpiry,
          storeBasketInDatabase: e.storeBasketInDatabase,
          basketInDatabaseExpiry: e.basketInDatabaseExpiry
        }
      })
    );
    return t ? { error: t } : this.read();
  }
  async delete() {
    return { error: new Error("The configuration cannot be deleted.") };
  }
}
class d extends c {
  #e = new o(this);
  constructor(e) {
    super(e, C, y);
  }
  // The configuration is a single row: create and save both store it, and both confirm it like v13 did.
  async create(e, t) {
    const a = await super.create(e, t);
    return a.error || await this.#t(), a;
  }
  async save(e) {
    const t = await super.save(e);
    return t.error || await this.#t(), t;
  }
  async #t() {
    (await this.getContext(n))?.peek("positive", {
      data: {
        headline: this.#e.term("umbcheckout_configuration_saved_title"),
        message: this.#e.term("umbcheckout_configuration_saved_message")
      }
    });
  }
}
export {
  d as UmbCheckoutConfigurationRepository,
  d as api
};
//# sourceMappingURL=configuration.repository-DwB0cwfH.js.map

import { UmbDetailRepositoryBase as n } from "@umbraco-cms/backoffice/repository";
import { tryExecute as t } from "@umbraco-cms/backoffice/resources";
import { C as r } from "./sdk.gen-bktVcCtQ.js";
import { U as o, a as i } from "./bundle.manifests-C6ijG9He.js";
import { UMBCHECKOUT_CONFIGURATION_STORE_CONTEXT as c } from "./configuration.store-BlabXKVF.js";
class u {
  #e;
  constructor(e) {
    this.#e = e;
  }
  async createScaffold() {
    return this.read();
  }
  async read() {
    const { data: e, error: a } = await t(this.#e, r.getConfiguration());
    return a || !e ? { error: a } : { data: {
      entityType: i,
      unique: o,
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
    const { error: a } = await t(
      this.#e,
      r.updateConfiguration({
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
    return a ? { error: a } : this.read();
  }
  async delete() {
    return { error: new Error("The configuration cannot be deleted.") };
  }
}
class g extends n {
  constructor(e) {
    super(e, u, c);
  }
}
export {
  g as UmbCheckoutConfigurationRepository,
  g as api
};
//# sourceMappingURL=configuration.repository-X5L2Mra9.js.map

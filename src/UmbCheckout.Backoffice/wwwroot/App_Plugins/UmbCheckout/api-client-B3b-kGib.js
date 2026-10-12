import { UMB_AUTH_CONTEXT as s } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT as a } from "@umbraco-cms/backoffice/notification";
function c(n, o) {
  let r = !1;
  n.consumeContext(s, (t) => {
    const e = t?.getOpenApiConfiguration();
    o.setConfig({
      auth: e?.token ?? void 0,
      baseUrl: e?.base ?? "",
      credentials: e?.credentials ?? "same-origin"
    });
  }), n.consumeContext(a, (t) => {
    o.interceptors.response.use((e) => (e.status === 401 && !r && (r = !0, t?.peek("warning", {
      data: { headline: "UmbCheckout", message: "Your session has expired, please reload the page." }
    })), e));
  });
}
export {
  c
};
//# sourceMappingURL=api-client-B3b-kGib.js.map

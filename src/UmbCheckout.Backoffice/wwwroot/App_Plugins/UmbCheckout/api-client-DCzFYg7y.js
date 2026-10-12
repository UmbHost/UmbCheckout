import { UMB_AUTH_CONTEXT as a } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT as u } from "@umbraco-cms/backoffice/notification";
const s = /* @__PURE__ */ new WeakMap();
function i(n) {
  let e = s.get(n);
  if (!e) {
    let o;
    e = { promise: new Promise((r) => o = r), resolve: o }, s.set(n, e);
  }
  return e;
}
function c(n) {
  return i(n).promise;
}
function p(n, e) {
  let o = !1;
  n.consumeContext(a, (t) => {
    if (!t) return;
    const r = t.getOpenApiConfiguration();
    e.setConfig({
      auth: r.token,
      baseUrl: r.base ?? "",
      credentials: r.credentials ?? "same-origin"
    }), i(e).resolve();
  }), n.consumeContext(u, (t) => {
    e.interceptors.response.use((r) => (r.status === 401 && !o && (o = !0, t?.peek("warning", {
      data: { headline: "UmbCheckout", message: "Your session has expired, please reload the page." }
    })), r));
  });
}
export {
  p as c,
  c as w
};
//# sourceMappingURL=api-client-DCzFYg7y.js.map

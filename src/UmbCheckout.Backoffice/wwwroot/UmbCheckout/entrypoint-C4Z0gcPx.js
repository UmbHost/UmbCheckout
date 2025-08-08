var F = Object.defineProperty;
var G = (e, r, t) => r in e ? F(e, r, { enumerable: !0, configurable: !0, writable: !0, value: t }) : e[r] = t;
var $ = (e, r, t) => G(e, typeof r != "symbol" ? r + "" : r, t);
import { UMB_WORKSPACE_CONDITION_ALIAS as k } from "@umbraco-cms/backoffice/workspace";
import { UMB_SETTINGS_SECTION_ALIAS as V } from "@umbraco-cms/backoffice/settings";
import { UmbElementMixin as X } from "@umbraco-cms/backoffice/element-api";
import { umbExtensionsRegistry as K } from "@umbraco-cms/backoffice/extension-registry";
import { LitElement as Q, html as x, property as Y, state as I, customElement as Z } from "@umbraco-cms/backoffice/external/lit";
import { UMB_SECTION_CONTEXT as ee } from "@umbraco-cms/backoffice/section";
import { UMB_AUTH_CONTEXT as te } from "@umbraco-cms/backoffice/auth";
const re = {
  type: "workspace",
  kind: "default",
  alias: "umbcheckout-workspace",
  name: "UmbCheckout Workspace",
  meta: {
    entityType: "umbcheckout",
    headline: "#umbcheckout_workspace_title"
  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "Umb.Section.Settings"
    }
  ]
}, ae = {
  type: "workspaceView",
  alias: "umbcheckout-overview-workspace",
  name: "UmbCheckout Overview",
  element: () => import("./overview-workspace-DyM_5J9W.js"),
  elementName: "umbcheckout-overview-workspace",
  meta: {
    label: "#umbcheckout_overview",
    pathname: "overview",
    icon: "icon-dashboard"
  },
  conditions: [
    {
      alias: k,
      match: "umbcheckout-workspace"
    }
  ]
}, oe = {
  type: "workspaceContext",
  name: "UmbCheckout Overview Workspace Context",
  alias: "umbcheckout-overview-workspace-context",
  api: () => import("./overview-workspace-context-C797DZyv.js").then((e) => e.o),
  conditions: [
    {
      alias: k,
      match: "umbcheckout-workspace"
    }
  ]
}, ne = {
  type: "workspaceFooterApp",
  alias: "umbcheckout-workspace-footer-app",
  name: "UmbCheckout Footer App",
  element: () => import("./workspace-footer-app.element-DEg_JWR4.js"),
  weight: 900,
  conditions: [
    {
      alias: k,
      match: "umbcheckout-workspace"
    }
  ]
}, se = {
  type: "workspaceAction",
  kind: "default",
  name: "UmbCheckout Save Workspace Action",
  alias: "umbcheckout-workspace-save-action",
  weight: 1e3,
  api: () => import("./workspace-save-action-Bv43UAjJ.js"),
  meta: {
    label: "#buttons_save",
    look: "primary",
    color: "positive"
  },
  conditions: [
    {
      alias: k,
      match: "umbcheckout-workspace"
    }
  ]
}, ie = [
  re,
  ae,
  ne,
  oe,
  se
], W = [
  ...ie
], le = {
  type: "localization",
  alias: "umbcheckout-localize-en",
  name: "UmbCheckout Localization",
  meta: {
    culture: "en"
  },
  js: () => import("./en-DD9rt2y4.js")
}, q = [
  le
];
var ce = Object.defineProperty, ue = Object.getOwnPropertyDescriptor, R = (e) => {
  throw TypeError(e);
}, _ = (e, r, t, o) => {
  for (var s = o > 1 ? void 0 : o ? ue(r, t) : r, i = e.length - 1, n; i >= 0; i--)
    (n = e[i]) && (s = (o ? n(r, t, s) : n(s)) || s);
  return o && s && ce(r, t, s), s;
}, U = (e, r, t) => r.has(e) || R("Cannot " + t), A = (e, r, t) => (U(e, r, "read from private field"), r.get(e)), T = (e, r, t) => r.has(e) ? R("Cannot add the same private member more than once") : r instanceof WeakSet ? r.add(e) : r.set(e, t), me = (e, r, t, o) => (U(e, r, "write to private field"), r.set(e, t), t), pe = (e, r, t) => (U(e, r, "access private method"), t), y, g, z;
let h = class extends X(Q) {
  constructor() {
    super(), T(this, g), T(this, y), this.hasChildren = !1, K.byType("umbcheckout-menuItem").subscribe((e) => {
      this.hasChildren = e.length > 0;
    }), this.consumeContext(ee, (e) => {
      this.observe(
        e == null ? void 0 : e.pathname,
        (r) => {
          me(this, y, r), pe(this, g, z).call(this);
        },
        "observePathname"
      );
    });
  }
  render() {
    return x`<umb-menu-item-layout
			label=${this.localize.term(this.manifest.meta.label) ?? this.manifest.name}
			icon-name=${this.manifest.meta.icon ?? "icon-bug"}
			.href=${this.itemPath}
			?has-Children=${this.hasChildren}
			>${this.renderChildren()}
		</umb-menu-item-layout>`;
  }
  renderChildren() {
    return x`<umb-extension-slot
			type="usync-menuItem"
			default-element="umb-menu-item-default"></umb-extension-slot>`;
  }
};
y = /* @__PURE__ */ new WeakMap();
g = /* @__PURE__ */ new WeakSet();
z = function() {
  A(this, y) && (this.itemPath = `section/${A(this, y)}/workspace/${this.manifest.meta.entityType}`);
};
_([
  Y({ type: Object, attribute: !1 })
], h.prototype, "manifest", 2);
_([
  I()
], h.prototype, "hasChildren", 2);
_([
  I()
], h.prototype, "itemPath", 2);
h = _([
  Z("umbcheckout-menu")
], h);
const p = {
  alias: "umbcheckout.menu",
  name: "UmbCheckout Menu",
  icon: "icon-shopping-basket",
  rootElement: "umbcheckout"
}, N = {
  type: "menu",
  alias: p.alias,
  name: p.name,
  meta: {
    label: p.name,
    icon: p.icon,
    entityType: p.rootElement
  }
}, he = {
  type: "menuItem",
  alias: "umbcheckout.menu.item",
  name: "UmbCheckout menu item",
  element: h,
  meta: {
    label: "umbcheckout_sidebar_overview",
    icon: "icon-shopping-basket",
    entityType: "umbcheckout",
    menus: [p.alias]
  }
}, de = {
  type: "sectionSidebarApp",
  kind: "menu",
  alias: "umbcheckout-sidebar-menu",
  name: "UmbCheckout section sidebar menu",
  weight: 160,
  meta: {
    label: "#umbcheckout_sidebar_menu",
    menu: N.alias
  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: V
    }
  ]
}, P = [
  N,
  de,
  he
];
var fe = async (e, r) => {
  let t = typeof r == "function" ? await r(e) : r;
  if (t) return e.scheme === "bearer" ? `Bearer ${t}` : e.scheme === "basic" ? `Basic ${btoa(t)}` : t;
}, be = { bodySerializer: (e) => JSON.stringify(e, (r, t) => typeof t == "bigint" ? t.toString() : t) }, ye = (e) => {
  switch (e) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, we = (e) => {
  switch (e) {
    case "form":
      return ",";
    case "pipeDelimited":
      return "|";
    case "spaceDelimited":
      return "%20";
    default:
      return ",";
  }
}, ve = (e) => {
  switch (e) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, M = ({ allowReserved: e, explode: r, name: t, style: o, value: s }) => {
  if (!r) {
    let a = (e ? s : s.map((l) => encodeURIComponent(l))).join(we(o));
    switch (o) {
      case "label":
        return `.${a}`;
      case "matrix":
        return `;${t}=${a}`;
      case "simple":
        return a;
      default:
        return `${t}=${a}`;
    }
  }
  let i = ye(o), n = s.map((a) => o === "label" || o === "simple" ? e ? a : encodeURIComponent(a) : C({ allowReserved: e, name: t, value: a })).join(i);
  return o === "label" || o === "matrix" ? i + n : n;
}, C = ({ allowReserved: e, name: r, value: t }) => {
  if (t == null) return "";
  if (typeof t == "object") throw new Error("Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these.");
  return `${r}=${e ? t : encodeURIComponent(t)}`;
}, D = ({ allowReserved: e, explode: r, name: t, style: o, value: s }) => {
  if (s instanceof Date) return `${t}=${s.toISOString()}`;
  if (o !== "deepObject" && !r) {
    let a = [];
    Object.entries(s).forEach(([d, m]) => {
      a = [...a, d, e ? m : encodeURIComponent(m)];
    });
    let l = a.join(",");
    switch (o) {
      case "form":
        return `${t}=${l}`;
      case "label":
        return `.${l}`;
      case "matrix":
        return `;${t}=${l}`;
      default:
        return l;
    }
  }
  let i = ve(o), n = Object.entries(s).map(([a, l]) => C({ allowReserved: e, name: o === "deepObject" ? `${t}[${a}]` : a, value: l })).join(i);
  return o === "label" || o === "matrix" ? i + n : n;
}, ke = /\{[^{}]+\}/g, _e = ({ path: e, url: r }) => {
  let t = r, o = r.match(ke);
  if (o) for (let s of o) {
    let i = !1, n = s.substring(1, s.length - 1), a = "simple";
    n.endsWith("*") && (i = !0, n = n.substring(0, n.length - 1)), n.startsWith(".") ? (n = n.substring(1), a = "label") : n.startsWith(";") && (n = n.substring(1), a = "matrix");
    let l = e[n];
    if (l == null) continue;
    if (Array.isArray(l)) {
      t = t.replace(s, M({ explode: i, name: n, style: a, value: l }));
      continue;
    }
    if (typeof l == "object") {
      t = t.replace(s, D({ explode: i, name: n, style: a, value: l }));
      continue;
    }
    if (a === "matrix") {
      t = t.replace(s, `;${C({ name: n, value: l })}`);
      continue;
    }
    let d = encodeURIComponent(a === "label" ? `.${l}` : l);
    t = t.replace(s, d);
  }
  return t;
}, B = ({ allowReserved: e, array: r, object: t } = {}) => (o) => {
  let s = [];
  if (o && typeof o == "object") for (let i in o) {
    let n = o[i];
    if (n != null) if (Array.isArray(n)) {
      let a = M({ allowReserved: e, explode: !0, name: i, style: "form", value: n, ...r });
      a && s.push(a);
    } else if (typeof n == "object") {
      let a = D({ allowReserved: e, explode: !0, name: i, style: "deepObject", value: n, ...t });
      a && s.push(a);
    } else {
      let a = C({ allowReserved: e, name: i, value: n });
      a && s.push(a);
    }
  }
  return s.join("&");
}, Ce = (e) => {
  var t;
  if (!e) return "stream";
  let r = (t = e.split(";")[0]) == null ? void 0 : t.trim();
  if (r) {
    if (r.startsWith("application/json") || r.endsWith("+json")) return "json";
    if (r === "multipart/form-data") return "formData";
    if (["application/", "audio/", "image/", "video/"].some((o) => r.startsWith(o))) return "blob";
    if (r.startsWith("text/")) return "text";
  }
}, Se = async ({ security: e, ...r }) => {
  for (let t of e) {
    let o = await fe(t, r.auth);
    if (!o) continue;
    let s = t.name ?? "Authorization";
    switch (t.in) {
      case "query":
        r.query || (r.query = {}), r.query[s] = o;
        break;
      case "cookie":
        r.headers.append("Cookie", `${s}=${o}`);
        break;
      case "header":
      default:
        r.headers.set(s, o);
        break;
    }
    return;
  }
}, j = (e) => ge({ baseUrl: e.baseUrl, path: e.path, query: e.query, querySerializer: typeof e.querySerializer == "function" ? e.querySerializer : B(e.querySerializer), url: e.url }), ge = ({ baseUrl: e, path: r, query: t, querySerializer: o, url: s }) => {
  let i = s.startsWith("/") ? s : `/${s}`, n = (e ?? "") + i;
  r && (n = _e({ path: r, url: n }));
  let a = t ? o(t) : "";
  return a.startsWith("?") && (a = a.substring(1)), a && (n += `?${a}`), n;
}, E = (e, r) => {
  var o;
  let t = { ...e, ...r };
  return (o = t.baseUrl) != null && o.endsWith("/") && (t.baseUrl = t.baseUrl.substring(0, t.baseUrl.length - 1)), t.headers = H(e.headers, r.headers), t;
}, H = (...e) => {
  let r = new Headers();
  for (let t of e) {
    if (!t || typeof t != "object") continue;
    let o = t instanceof Headers ? t.entries() : Object.entries(t);
    for (let [s, i] of o) if (i === null) r.delete(s);
    else if (Array.isArray(i)) for (let n of i) r.append(s, n);
    else i !== void 0 && r.set(s, typeof i == "object" ? JSON.stringify(i) : i);
  }
  return r;
}, S = class {
  constructor() {
    $(this, "_fns");
    this._fns = [];
  }
  clear() {
    this._fns = [];
  }
  getInterceptorIndex(e) {
    return typeof e == "number" ? this._fns[e] ? e : -1 : this._fns.indexOf(e);
  }
  exists(e) {
    let r = this.getInterceptorIndex(e);
    return !!this._fns[r];
  }
  eject(e) {
    let r = this.getInterceptorIndex(e);
    this._fns[r] && (this._fns[r] = null);
  }
  update(e, r) {
    let t = this.getInterceptorIndex(e);
    return this._fns[t] ? (this._fns[t] = r, e) : !1;
  }
  use(e) {
    return this._fns = [...this._fns, e], this._fns.length - 1;
  }
}, Ue = () => ({ error: new S(), request: new S(), response: new S() }), Oe = B({ allowReserved: !1, array: { explode: !0, style: "form" }, object: { explode: !0, style: "deepObject" } }), $e = { "Content-Type": "application/json" }, O = (e = {}) => ({ ...be, headers: $e, parseAs: "auto", querySerializer: Oe, ...e }), L = (e = {}) => {
  let r = E(O(), e), t = () => ({ ...r }), o = (n) => (r = E(r, n), t()), s = Ue(), i = async (n) => {
    let a = { ...r, ...n, fetch: n.fetch ?? r.fetch ?? globalThis.fetch, headers: H(r.headers, n.headers) };
    a.security && await Se({ ...a, security: a.security }), a.body && a.bodySerializer && (a.body = a.bodySerializer(a.body)), (a.body === void 0 || a.body === "") && a.headers.delete("Content-Type");
    let l = j(a), d = { redirect: "follow", ...a }, m = new Request(l, d);
    for (let u of s.request._fns) u && (m = await u(m, a));
    let J = a.fetch, c = await J(m);
    for (let u of s.response._fns) u && (c = await u(c, m, a));
    let w = { request: m, response: c };
    if (c.ok) {
      if (c.status === 204 || c.headers.get("Content-Length") === "0") return a.responseStyle === "data" ? {} : { data: {}, ...w };
      let u = (a.parseAs === "auto" ? Ce(c.headers.get("Content-Type")) : a.parseAs) ?? "json";
      if (u === "stream") return a.responseStyle === "data" ? c.body : { data: c.body, ...w };
      let b = await c[u]();
      return u === "json" && (a.responseValidator && await a.responseValidator(b), a.responseTransformer && (b = await a.responseTransformer(b))), a.responseStyle === "data" ? b : { data: b, ...w };
    }
    let v = await c.text();
    try {
      v = JSON.parse(v);
    } catch {
    }
    let f = v;
    for (let u of s.error._fns) u && (f = await u(v, c, m, a));
    if (f = f || {}, a.throwOnError) throw f;
    return a.responseStyle === "data" ? void 0 : { error: f, ...w };
  };
  return { buildUrl: j, connect: (n) => i({ ...n, method: "CONNECT" }), delete: (n) => i({ ...n, method: "DELETE" }), get: (n) => i({ ...n, method: "GET" }), getConfig: t, head: (n) => i({ ...n, method: "HEAD" }), interceptors: s, options: (n) => i({ ...n, method: "OPTIONS" }), patch: (n) => i({ ...n, method: "PATCH" }), post: (n) => i({ ...n, method: "POST" }), put: (n) => i({ ...n, method: "PUT" }), request: i, setConfig: o, trace: (n) => i({ ...n, method: "TRACE" }) };
};
const xe = L(O({
  baseUrl: "https://localhost:44390"
})), Ae = L(O({
  baseUrl: "https://localhost:44390"
})), Te = (e, r) => {
  e.consumeContext(te, (t) => {
    const o = t == null ? void 0 : t.getOpenApiConfiguration();
    xe.setConfig({
      throwOnError: !0,
      auth: (o == null ? void 0 : o.token) ?? void 0,
      baseUrl: (o == null ? void 0 : o.base) ?? "",
      credentials: (o == null ? void 0 : o.credentials) ?? "same-origin"
    }), Ae.setConfig({
      throwOnError: !0,
      auth: (o == null ? void 0 : o.token) ?? void 0,
      baseUrl: (o == null ? void 0 : o.base) ?? "",
      credentials: (o == null ? void 0 : o.credentials) ?? "same-origin"
    }), r.registerMany([
      ...P,
      ...q,
      ...W
    ]);
  });
}, je = (e, r) => {
  r.unregisterMany([
    ...P.map((t) => t.alias),
    ...q.map((t) => t.alias),
    ...W.map((t) => t.alias)
  ]);
}, Me = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  onInit: Te,
  onUnload: je
}, Symbol.toStringTag, { value: "Module" }));
export {
  xe as a,
  Ae as c,
  Me as e
};
//# sourceMappingURL=entrypoint-C4Z0gcPx.js.map

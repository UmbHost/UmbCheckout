var J = Object.defineProperty;
var G = (e, r, t) => r in e ? J(e, r, { enumerable: !0, configurable: !0, writable: !0, value: t }) : e[r] = t;
var $ = (e, r, t) => G(e, typeof r != "symbol" ? r + "" : r, t);
import { UMB_SETTINGS_SECTION_ALIAS as V } from "@umbraco-cms/backoffice/settings";
import { UmbElementMixin as X } from "@umbraco-cms/backoffice/element-api";
import { umbExtensionsRegistry as Q } from "@umbraco-cms/backoffice/extension-registry";
import { LitElement as F, html as j, property as K, state as A, customElement as Y } from "@umbraco-cms/backoffice/external/lit";
import { UMB_SECTION_CONTEXT as Z } from "@umbraco-cms/backoffice/section";
import { UMB_AUTH_CONTEXT as ee } from "@umbraco-cms/backoffice/auth";
const te = {
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
}, re = {
  type: "workspaceView",
  alias: "umbcheckout-overview",
  name: "UmbCheckout Overview",
  element: () => import("./overview-B_Jshwsz.js"),
  elementName: "umbcheckout-overview",
  meta: {
    label: "#umbcheckout_overview",
    pathname: "overview",
    icon: "icon-dashboard"
  },
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "umbcheckout-workspace"
    }
  ]
}, I = [
  te,
  re
], ae = {
  type: "localization",
  alias: "umbcheckout-localize-en",
  name: "UmbCheckout Localization",
  meta: {
    culture: "en"
  },
  js: () => import("./en-BHRoS06T.js")
}, q = [
  ae
];
var ne = Object.defineProperty, se = Object.getOwnPropertyDescriptor, R = (e) => {
  throw TypeError(e);
}, _ = (e, r, t, n) => {
  for (var o = n > 1 ? void 0 : n ? se(r, t) : r, i = e.length - 1, s; i >= 0; i--)
    (s = e[i]) && (o = (n ? s(r, t, o) : s(o)) || o);
  return n && o && ne(r, t, o), o;
}, S = (e, r, t) => r.has(e) || R("Cannot " + t), O = (e, r, t) => (S(e, r, "read from private field"), r.get(e)), T = (e, r, t) => r.has(e) ? R("Cannot add the same private member more than once") : r instanceof WeakSet ? r.add(e) : r.set(e, t), oe = (e, r, t, n) => (S(e, r, "write to private field"), r.set(e, t), t), ie = (e, r, t) => (S(e, r, "access private method"), t), y, g, z;
let p = class extends X(F) {
  constructor() {
    super(), T(this, g), T(this, y), this.hasChildren = !1, Q.byType("umbcheckout-menuItem").subscribe((e) => {
      this.hasChildren = e.length > 0;
    }), this.consumeContext(Z, (e) => {
      this.observe(
        e == null ? void 0 : e.pathname,
        (r) => {
          oe(this, y, r), ie(this, g, z).call(this);
        },
        "observePathname"
      );
    });
  }
  render() {
    return j`<umb-menu-item-layout
			label=${this.localize.term(this.manifest.meta.label) ?? this.manifest.name}
			icon-name=${this.manifest.meta.icon ?? "icon-bug"}
			.href=${this.itemPath}
			?has-Children=${this.hasChildren}
			>${this.renderChildren()}
		</umb-menu-item-layout>`;
  }
  renderChildren() {
    return j`<umb-extension-slot
			type="usync-menuItem"
			default-element="umb-menu-item-default"></umb-extension-slot>`;
  }
};
y = /* @__PURE__ */ new WeakMap();
g = /* @__PURE__ */ new WeakSet();
z = function() {
  O(this, y) && (this.itemPath = `section/${O(this, y)}/workspace/${this.manifest.meta.entityType}`);
};
_([
  K({ type: Object, attribute: !1 })
], p.prototype, "manifest", 2);
_([
  A()
], p.prototype, "hasChildren", 2);
_([
  A()
], p.prototype, "itemPath", 2);
p = _([
  Y("umbcheckout-menu")
], p);
const h = {
  alias: "umbcheckout.menu",
  name: "UmbCheckout Menu",
  icon: "icon-shopping-basket",
  rootElement: "umbcheckout"
}, W = {
  type: "menu",
  alias: h.alias,
  name: h.name,
  meta: {
    label: h.name,
    icon: h.icon,
    entityType: h.rootElement
  }
}, le = {
  type: "menuItem",
  alias: "umbcheckout.menu.item",
  name: "UmbCheckout menu item",
  element: p,
  meta: {
    label: "umbcheckout_sidebar_overview",
    icon: "icon-shopping-basket",
    entityType: "umbcheckout",
    menus: [h.alias]
  }
}, ue = {
  type: "sectionSidebarApp",
  kind: "menu",
  alias: "umbcheckout-sidebar-menu",
  name: "UmbCheckout section sidebar menu",
  weight: 160,
  meta: {
    label: "#umbcheckout_sidebar_menu",
    menu: W.alias
  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: V
    }
  ]
}, P = [
  W,
  ue,
  le
];
var ce = async (e, r) => {
  let t = typeof r == "function" ? await r(e) : r;
  if (t) return e.scheme === "bearer" ? `Bearer ${t}` : e.scheme === "basic" ? `Basic ${btoa(t)}` : t;
}, me = { bodySerializer: (e) => JSON.stringify(e, (r, t) => typeof t == "bigint" ? t.toString() : t) }, he = (e) => {
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
}, pe = (e) => {
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
}, de = (e) => {
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
}, N = ({ allowReserved: e, explode: r, name: t, style: n, value: o }) => {
  if (!r) {
    let a = (e ? o : o.map((l) => encodeURIComponent(l))).join(pe(n));
    switch (n) {
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
  let i = he(n), s = o.map((a) => n === "label" || n === "simple" ? e ? a : encodeURIComponent(a) : k({ allowReserved: e, name: t, value: a })).join(i);
  return n === "label" || n === "matrix" ? i + s : s;
}, k = ({ allowReserved: e, name: r, value: t }) => {
  if (t == null) return "";
  if (typeof t == "object") throw new Error("Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these.");
  return `${r}=${e ? t : encodeURIComponent(t)}`;
}, M = ({ allowReserved: e, explode: r, name: t, style: n, value: o }) => {
  if (o instanceof Date) return `${t}=${o.toISOString()}`;
  if (n !== "deepObject" && !r) {
    let a = [];
    Object.entries(o).forEach(([d, m]) => {
      a = [...a, d, e ? m : encodeURIComponent(m)];
    });
    let l = a.join(",");
    switch (n) {
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
  let i = de(n), s = Object.entries(o).map(([a, l]) => k({ allowReserved: e, name: n === "deepObject" ? `${t}[${a}]` : a, value: l })).join(i);
  return n === "label" || n === "matrix" ? i + s : s;
}, fe = /\{[^{}]+\}/g, be = ({ path: e, url: r }) => {
  let t = r, n = r.match(fe);
  if (n) for (let o of n) {
    let i = !1, s = o.substring(1, o.length - 1), a = "simple";
    s.endsWith("*") && (i = !0, s = s.substring(0, s.length - 1)), s.startsWith(".") ? (s = s.substring(1), a = "label") : s.startsWith(";") && (s = s.substring(1), a = "matrix");
    let l = e[s];
    if (l == null) continue;
    if (Array.isArray(l)) {
      t = t.replace(o, N({ explode: i, name: s, style: a, value: l }));
      continue;
    }
    if (typeof l == "object") {
      t = t.replace(o, M({ explode: i, name: s, style: a, value: l }));
      continue;
    }
    if (a === "matrix") {
      t = t.replace(o, `;${k({ name: s, value: l })}`);
      continue;
    }
    let d = encodeURIComponent(a === "label" ? `.${l}` : l);
    t = t.replace(o, d);
  }
  return t;
}, D = ({ allowReserved: e, array: r, object: t } = {}) => (n) => {
  let o = [];
  if (n && typeof n == "object") for (let i in n) {
    let s = n[i];
    if (s != null) if (Array.isArray(s)) {
      let a = N({ allowReserved: e, explode: !0, name: i, style: "form", value: s, ...r });
      a && o.push(a);
    } else if (typeof s == "object") {
      let a = M({ allowReserved: e, explode: !0, name: i, style: "deepObject", value: s, ...t });
      a && o.push(a);
    } else {
      let a = k({ allowReserved: e, name: i, value: s });
      a && o.push(a);
    }
  }
  return o.join("&");
}, ye = (e) => {
  var t;
  if (!e) return "stream";
  let r = (t = e.split(";")[0]) == null ? void 0 : t.trim();
  if (r) {
    if (r.startsWith("application/json") || r.endsWith("+json")) return "json";
    if (r === "multipart/form-data") return "formData";
    if (["application/", "audio/", "image/", "video/"].some((n) => r.startsWith(n))) return "blob";
    if (r.startsWith("text/")) return "text";
  }
}, ve = async ({ security: e, ...r }) => {
  for (let t of e) {
    let n = await ce(t, r.auth);
    if (!n) continue;
    let o = t.name ?? "Authorization";
    switch (t.in) {
      case "query":
        r.query || (r.query = {}), r.query[o] = n;
        break;
      case "cookie":
        r.headers.append("Cookie", `${o}=${n}`);
        break;
      case "header":
      default:
        r.headers.set(o, n);
        break;
    }
    return;
  }
}, x = (e) => we({ baseUrl: e.baseUrl, path: e.path, query: e.query, querySerializer: typeof e.querySerializer == "function" ? e.querySerializer : D(e.querySerializer), url: e.url }), we = ({ baseUrl: e, path: r, query: t, querySerializer: n, url: o }) => {
  let i = o.startsWith("/") ? o : `/${o}`, s = (e ?? "") + i;
  r && (s = be({ path: r, url: s }));
  let a = t ? n(t) : "";
  return a.startsWith("?") && (a = a.substring(1)), a && (s += `?${a}`), s;
}, E = (e, r) => {
  var n;
  let t = { ...e, ...r };
  return (n = t.baseUrl) != null && n.endsWith("/") && (t.baseUrl = t.baseUrl.substring(0, t.baseUrl.length - 1)), t.headers = H(e.headers, r.headers), t;
}, H = (...e) => {
  let r = new Headers();
  for (let t of e) {
    if (!t || typeof t != "object") continue;
    let n = t instanceof Headers ? t.entries() : Object.entries(t);
    for (let [o, i] of n) if (i === null) r.delete(o);
    else if (Array.isArray(i)) for (let s of i) r.append(o, s);
    else i !== void 0 && r.set(o, typeof i == "object" ? JSON.stringify(i) : i);
  }
  return r;
}, C = class {
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
}, _e = () => ({ error: new C(), request: new C(), response: new C() }), ke = D({ allowReserved: !1, array: { explode: !0, style: "form" }, object: { explode: !0, style: "deepObject" } }), Ce = { "Content-Type": "application/json" }, U = (e = {}) => ({ ...me, headers: Ce, parseAs: "auto", querySerializer: ke, ...e }), B = (e = {}) => {
  let r = E(U(), e), t = () => ({ ...r }), n = (s) => (r = E(r, s), t()), o = _e(), i = async (s) => {
    let a = { ...r, ...s, fetch: s.fetch ?? r.fetch ?? globalThis.fetch, headers: H(r.headers, s.headers) };
    a.security && await ve({ ...a, security: a.security }), a.body && a.bodySerializer && (a.body = a.bodySerializer(a.body)), (a.body === void 0 || a.body === "") && a.headers.delete("Content-Type");
    let l = x(a), d = { redirect: "follow", ...a }, m = new Request(l, d);
    for (let c of o.request._fns) c && (m = await c(m, a));
    let L = a.fetch, u = await L(m);
    for (let c of o.response._fns) c && (u = await c(u, m, a));
    let v = { request: m, response: u };
    if (u.ok) {
      if (u.status === 204 || u.headers.get("Content-Length") === "0") return a.responseStyle === "data" ? {} : { data: {}, ...v };
      let c = (a.parseAs === "auto" ? ye(u.headers.get("Content-Type")) : a.parseAs) ?? "json";
      if (c === "stream") return a.responseStyle === "data" ? u.body : { data: u.body, ...v };
      let b = await u[c]();
      return c === "json" && (a.responseValidator && await a.responseValidator(b), a.responseTransformer && (b = await a.responseTransformer(b))), a.responseStyle === "data" ? b : { data: b, ...v };
    }
    let w = await u.text();
    try {
      w = JSON.parse(w);
    } catch {
    }
    let f = w;
    for (let c of o.error._fns) c && (f = await c(w, u, m, a));
    if (f = f || {}, a.throwOnError) throw f;
    return a.responseStyle === "data" ? void 0 : { error: f, ...v };
  };
  return { buildUrl: x, connect: (s) => i({ ...s, method: "CONNECT" }), delete: (s) => i({ ...s, method: "DELETE" }), get: (s) => i({ ...s, method: "GET" }), getConfig: t, head: (s) => i({ ...s, method: "HEAD" }), interceptors: o, options: (s) => i({ ...s, method: "OPTIONS" }), patch: (s) => i({ ...s, method: "PATCH" }), post: (s) => i({ ...s, method: "POST" }), put: (s) => i({ ...s, method: "PUT" }), request: i, setConfig: n, trace: (s) => i({ ...s, method: "TRACE" }) };
};
const ge = B(U({
  baseUrl: "https://localhost:44390"
})), Se = B(U({
  baseUrl: "https://localhost:44390"
})), Ue = (e, r) => {
  e.consumeContext(ee, (t) => {
    const n = t == null ? void 0 : t.getOpenApiConfiguration();
    ge.setConfig({
      auth: (n == null ? void 0 : n.token) ?? void 0,
      baseUrl: (n == null ? void 0 : n.base) ?? "",
      credentials: (n == null ? void 0 : n.credentials) ?? "same-origin"
    }), Se.setConfig({
      auth: (n == null ? void 0 : n.token) ?? void 0,
      baseUrl: (n == null ? void 0 : n.base) ?? "",
      credentials: (n == null ? void 0 : n.credentials) ?? "same-origin"
    }), r.registerMany([
      ...P,
      ...q,
      ...I
    ]);
  });
}, $e = (e, r) => {
  r.unregisterMany([
    ...P.map((t) => t.alias),
    ...q.map((t) => t.alias),
    ...I.map((t) => t.alias)
  ]);
}, qe = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  onInit: Ue,
  onUnload: $e
}, Symbol.toStringTag, { value: "Module" }));
export {
  ge as a,
  Se as c,
  qe as e
};
//# sourceMappingURL=entrypoint-CkUrIMau.js.map

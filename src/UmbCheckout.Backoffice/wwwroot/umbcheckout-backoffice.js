/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const z = globalThis, Z = z.ShadowRoot && (z.ShadyCSS === void 0 || z.ShadyCSS.nativeShadow) && "adoptedStyleSheets" in Document.prototype && "replace" in CSSStyleSheet.prototype, F = Symbol(), tt = /* @__PURE__ */ new WeakMap();
let dt = class {
  constructor(t, e, s) {
    if (this._$cssResult$ = !0, s !== F) throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");
    this.cssText = t, this.t = e;
  }
  get styleSheet() {
    let t = this.o;
    const e = this.t;
    if (Z && t === void 0) {
      const s = e !== void 0 && e.length === 1;
      s && (t = tt.get(e)), t === void 0 && ((this.o = t = new CSSStyleSheet()).replaceSync(this.cssText), s && tt.set(e, t));
    }
    return t;
  }
  toString() {
    return this.cssText;
  }
};
const yt = (r) => new dt(typeof r == "string" ? r : r + "", void 0, F), pt = (r, ...t) => {
  const e = r.length === 1 ? r[0] : t.reduce((s, i, a) => s + ((n) => {
    if (n._$cssResult$ === !0) return n.cssText;
    if (typeof n == "number") return n;
    throw Error("Value passed to 'css' function must be a 'css' function result: " + n + ". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.");
  })(i) + r[a + 1], r[0]);
  return new dt(e, r, F);
}, bt = (r, t) => {
  if (Z) r.adoptedStyleSheets = t.map((e) => e instanceof CSSStyleSheet ? e : e.styleSheet);
  else for (const e of t) {
    const s = document.createElement("style"), i = z.litNonce;
    i !== void 0 && s.setAttribute("nonce", i), s.textContent = e.cssText, r.appendChild(s);
  }
}, et = Z ? (r) => r : (r) => r instanceof CSSStyleSheet ? ((t) => {
  let e = "";
  for (const s of t.cssRules) e += s.cssText;
  return yt(e);
})(r) : r;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const { is: At, defineProperty: St, getOwnPropertyDescriptor: wt, getOwnPropertyNames: Et, getOwnPropertySymbols: Ct, getPrototypeOf: xt } = Object, m = globalThis, st = m.trustedTypes, Pt = st ? st.emptyScript : "", q = m.reactiveElementPolyfillSupport, O = (r, t) => r, L = { toAttribute(r, t) {
  switch (t) {
    case Boolean:
      r = r ? Pt : null;
      break;
    case Object:
    case Array:
      r = r == null ? r : JSON.stringify(r);
  }
  return r;
}, fromAttribute(r, t) {
  let e = r;
  switch (t) {
    case Boolean:
      e = r !== null;
      break;
    case Number:
      e = r === null ? null : Number(r);
      break;
    case Object:
    case Array:
      try {
        e = JSON.parse(r);
      } catch {
        e = null;
      }
  }
  return e;
} }, K = (r, t) => !At(r, t), it = { attribute: !0, type: String, converter: L, reflect: !1, useDefault: !1, hasChanged: K };
Symbol.metadata ?? (Symbol.metadata = Symbol("metadata")), m.litPropertyMetadata ?? (m.litPropertyMetadata = /* @__PURE__ */ new WeakMap());
let C = class extends HTMLElement {
  static addInitializer(t) {
    this._$Ei(), (this.l ?? (this.l = [])).push(t);
  }
  static get observedAttributes() {
    return this.finalize(), this._$Eh && [...this._$Eh.keys()];
  }
  static createProperty(t, e = it) {
    if (e.state && (e.attribute = !1), this._$Ei(), this.prototype.hasOwnProperty(t) && ((e = Object.create(e)).wrapped = !0), this.elementProperties.set(t, e), !e.noAccessor) {
      const s = Symbol(), i = this.getPropertyDescriptor(t, s, e);
      i !== void 0 && St(this.prototype, t, i);
    }
  }
  static getPropertyDescriptor(t, e, s) {
    const { get: i, set: a } = wt(this.prototype, t) ?? { get() {
      return this[e];
    }, set(n) {
      this[e] = n;
    } };
    return { get: i, set(n) {
      const l = i == null ? void 0 : i.call(this);
      a == null || a.call(this, n), this.requestUpdate(t, l, s);
    }, configurable: !0, enumerable: !0 };
  }
  static getPropertyOptions(t) {
    return this.elementProperties.get(t) ?? it;
  }
  static _$Ei() {
    if (this.hasOwnProperty(O("elementProperties"))) return;
    const t = xt(this);
    t.finalize(), t.l !== void 0 && (this.l = [...t.l]), this.elementProperties = new Map(t.elementProperties);
  }
  static finalize() {
    if (this.hasOwnProperty(O("finalized"))) return;
    if (this.finalized = !0, this._$Ei(), this.hasOwnProperty(O("properties"))) {
      const e = this.properties, s = [...Et(e), ...Ct(e)];
      for (const i of s) this.createProperty(i, e[i]);
    }
    const t = this[Symbol.metadata];
    if (t !== null) {
      const e = litPropertyMetadata.get(t);
      if (e !== void 0) for (const [s, i] of e) this.elementProperties.set(s, i);
    }
    this._$Eh = /* @__PURE__ */ new Map();
    for (const [e, s] of this.elementProperties) {
      const i = this._$Eu(e, s);
      i !== void 0 && this._$Eh.set(i, e);
    }
    this.elementStyles = this.finalizeStyles(this.styles);
  }
  static finalizeStyles(t) {
    const e = [];
    if (Array.isArray(t)) {
      const s = new Set(t.flat(1 / 0).reverse());
      for (const i of s) e.unshift(et(i));
    } else t !== void 0 && e.push(et(t));
    return e;
  }
  static _$Eu(t, e) {
    const s = e.attribute;
    return s === !1 ? void 0 : typeof s == "string" ? s : typeof t == "string" ? t.toLowerCase() : void 0;
  }
  constructor() {
    super(), this._$Ep = void 0, this.isUpdatePending = !1, this.hasUpdated = !1, this._$Em = null, this._$Ev();
  }
  _$Ev() {
    var t;
    this._$ES = new Promise((e) => this.enableUpdating = e), this._$AL = /* @__PURE__ */ new Map(), this._$E_(), this.requestUpdate(), (t = this.constructor.l) == null || t.forEach((e) => e(this));
  }
  addController(t) {
    var e;
    (this._$EO ?? (this._$EO = /* @__PURE__ */ new Set())).add(t), this.renderRoot !== void 0 && this.isConnected && ((e = t.hostConnected) == null || e.call(t));
  }
  removeController(t) {
    var e;
    (e = this._$EO) == null || e.delete(t);
  }
  _$E_() {
    const t = /* @__PURE__ */ new Map(), e = this.constructor.elementProperties;
    for (const s of e.keys()) this.hasOwnProperty(s) && (t.set(s, this[s]), delete this[s]);
    t.size > 0 && (this._$Ep = t);
  }
  createRenderRoot() {
    const t = this.shadowRoot ?? this.attachShadow(this.constructor.shadowRootOptions);
    return bt(t, this.constructor.elementStyles), t;
  }
  connectedCallback() {
    var t;
    this.renderRoot ?? (this.renderRoot = this.createRenderRoot()), this.enableUpdating(!0), (t = this._$EO) == null || t.forEach((e) => {
      var s;
      return (s = e.hostConnected) == null ? void 0 : s.call(e);
    });
  }
  enableUpdating(t) {
  }
  disconnectedCallback() {
    var t;
    (t = this._$EO) == null || t.forEach((e) => {
      var s;
      return (s = e.hostDisconnected) == null ? void 0 : s.call(e);
    });
  }
  attributeChangedCallback(t, e, s) {
    this._$AK(t, s);
  }
  _$ET(t, e) {
    var a;
    const s = this.constructor.elementProperties.get(t), i = this.constructor._$Eu(t, s);
    if (i !== void 0 && s.reflect === !0) {
      const n = (((a = s.converter) == null ? void 0 : a.toAttribute) !== void 0 ? s.converter : L).toAttribute(e, s.type);
      this._$Em = t, n == null ? this.removeAttribute(i) : this.setAttribute(i, n), this._$Em = null;
    }
  }
  _$AK(t, e) {
    var a, n;
    const s = this.constructor, i = s._$Eh.get(t);
    if (i !== void 0 && this._$Em !== i) {
      const l = s.getPropertyOptions(i), o = typeof l.converter == "function" ? { fromAttribute: l.converter } : ((a = l.converter) == null ? void 0 : a.fromAttribute) !== void 0 ? l.converter : L;
      this._$Em = i;
      const u = o.fromAttribute(e, l.type);
      this[i] = u ?? ((n = this._$Ej) == null ? void 0 : n.get(i)) ?? u, this._$Em = null;
    }
  }
  requestUpdate(t, e, s, i = !1, a) {
    var n;
    if (t !== void 0) {
      const l = this.constructor;
      if (i === !1 && (a = this[t]), s ?? (s = l.getPropertyOptions(t)), !((s.hasChanged ?? K)(a, e) || s.useDefault && s.reflect && a === ((n = this._$Ej) == null ? void 0 : n.get(t)) && !this.hasAttribute(l._$Eu(t, s)))) return;
      this.C(t, e, s);
    }
    this.isUpdatePending === !1 && (this._$ES = this._$EP());
  }
  C(t, e, { useDefault: s, reflect: i, wrapped: a }, n) {
    s && !(this._$Ej ?? (this._$Ej = /* @__PURE__ */ new Map())).has(t) && (this._$Ej.set(t, n ?? e ?? this[t]), a !== !0 || n !== void 0) || (this._$AL.has(t) || (this.hasUpdated || s || (e = void 0), this._$AL.set(t, e)), i === !0 && this._$Em !== t && (this._$Eq ?? (this._$Eq = /* @__PURE__ */ new Set())).add(t));
  }
  async _$EP() {
    this.isUpdatePending = !0;
    try {
      await this._$ES;
    } catch (e) {
      Promise.reject(e);
    }
    const t = this.scheduleUpdate();
    return t != null && await t, !this.isUpdatePending;
  }
  scheduleUpdate() {
    return this.performUpdate();
  }
  performUpdate() {
    var s;
    if (!this.isUpdatePending) return;
    if (!this.hasUpdated) {
      if (this.renderRoot ?? (this.renderRoot = this.createRenderRoot()), this._$Ep) {
        for (const [a, n] of this._$Ep) this[a] = n;
        this._$Ep = void 0;
      }
      const i = this.constructor.elementProperties;
      if (i.size > 0) for (const [a, n] of i) {
        const { wrapped: l } = n, o = this[a];
        l !== !0 || this._$AL.has(a) || o === void 0 || this.C(a, void 0, n, o);
      }
    }
    let t = !1;
    const e = this._$AL;
    try {
      t = this.shouldUpdate(e), t ? (this.willUpdate(e), (s = this._$EO) == null || s.forEach((i) => {
        var a;
        return (a = i.hostUpdate) == null ? void 0 : a.call(i);
      }), this.update(e)) : this._$EM();
    } catch (i) {
      throw t = !1, this._$EM(), i;
    }
    t && this._$AE(e);
  }
  willUpdate(t) {
  }
  _$AE(t) {
    var e;
    (e = this._$EO) == null || e.forEach((s) => {
      var i;
      return (i = s.hostUpdated) == null ? void 0 : i.call(s);
    }), this.hasUpdated || (this.hasUpdated = !0, this.firstUpdated(t)), this.updated(t);
  }
  _$EM() {
    this._$AL = /* @__PURE__ */ new Map(), this.isUpdatePending = !1;
  }
  get updateComplete() {
    return this.getUpdateComplete();
  }
  getUpdateComplete() {
    return this._$ES;
  }
  shouldUpdate(t) {
    return !0;
  }
  update(t) {
    this._$Eq && (this._$Eq = this._$Eq.forEach((e) => this._$ET(e, this[e]))), this._$EM();
  }
  updated(t) {
  }
  firstUpdated(t) {
  }
};
C.elementStyles = [], C.shadowRootOptions = { mode: "open" }, C[O("elementProperties")] = /* @__PURE__ */ new Map(), C[O("finalized")] = /* @__PURE__ */ new Map(), q == null || q({ ReactiveElement: C }), (m.reactiveElementVersions ?? (m.reactiveElementVersions = [])).push("2.1.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const T = globalThis, rt = (r) => r, j = T.trustedTypes, nt = j ? j.createPolicy("lit-html", { createHTML: (r) => r }) : void 0, _t = "$lit$", v = `lit$${Math.random().toFixed(9).slice(2)}$`, $t = "?" + v, Ut = `<${$t}>`, S = document, D = () => S.createComment(""), H = (r) => r === null || typeof r != "object" && typeof r != "function", Y = Array.isArray, kt = (r) => Y(r) || typeof (r == null ? void 0 : r[Symbol.iterator]) == "function", W = `[ 	
\f\r]`, k = /<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g, at = /-->/g, ot = />/g, y = RegExp(`>|${W}(?:([^\\s"'>=/]+)(${W}*=${W}*(?:[^ 	
\f\r"'\`<>=]|("|')|))|$)`, "g"), lt = /'/g, ct = /"/g, ft = /^(?:script|style|textarea|title)$/i, Ot = (r) => (t, ...e) => ({ _$litType$: r, strings: t, values: e }), p = Ot(1), P = Symbol.for("lit-noChange"), c = Symbol.for("lit-nothing"), ht = /* @__PURE__ */ new WeakMap(), b = S.createTreeWalker(S, 129);
function vt(r, t) {
  if (!Y(r) || !r.hasOwnProperty("raw")) throw Error("invalid template strings array");
  return nt !== void 0 ? nt.createHTML(t) : t;
}
const Tt = (r, t) => {
  const e = r.length - 1, s = [];
  let i, a = t === 2 ? "<svg>" : t === 3 ? "<math>" : "", n = k;
  for (let l = 0; l < e; l++) {
    const o = r[l];
    let u, d, h = -1, _ = 0;
    for (; _ < o.length && (n.lastIndex = _, d = n.exec(o), d !== null); ) _ = n.lastIndex, n === k ? d[1] === "!--" ? n = at : d[1] !== void 0 ? n = ot : d[2] !== void 0 ? (ft.test(d[2]) && (i = RegExp("</" + d[2], "g")), n = y) : d[3] !== void 0 && (n = y) : n === y ? d[0] === ">" ? (n = i ?? k, h = -1) : d[1] === void 0 ? h = -2 : (h = n.lastIndex - d[2].length, u = d[1], n = d[3] === void 0 ? y : d[3] === '"' ? ct : lt) : n === ct || n === lt ? n = y : n === at || n === ot ? n = k : (n = y, i = void 0);
    const f = n === y && r[l + 1].startsWith("/>") ? " " : "";
    a += n === k ? o + Ut : h >= 0 ? (s.push(u), o.slice(0, h) + _t + o.slice(h) + v + f) : o + v + (h === -2 ? l : f);
  }
  return [vt(r, a + (r[e] || "<?>") + (t === 2 ? "</svg>" : t === 3 ? "</math>" : "")), s];
};
class M {
  constructor({ strings: t, _$litType$: e }, s) {
    let i;
    this.parts = [];
    let a = 0, n = 0;
    const l = t.length - 1, o = this.parts, [u, d] = Tt(t, e);
    if (this.el = M.createElement(u, s), b.currentNode = this.el.content, e === 2 || e === 3) {
      const h = this.el.content.firstChild;
      h.replaceWith(...h.childNodes);
    }
    for (; (i = b.nextNode()) !== null && o.length < l; ) {
      if (i.nodeType === 1) {
        if (i.hasAttributes()) for (const h of i.getAttributeNames()) if (h.endsWith(_t)) {
          const _ = d[n++], f = i.getAttribute(h).split(v), R = /([.?@])?(.*)/.exec(_);
          o.push({ type: 1, index: a, name: R[2], strings: f, ctor: R[1] === "." ? Ht : R[1] === "?" ? Mt : R[1] === "@" ? It : B }), i.removeAttribute(h);
        } else h.startsWith(v) && (o.push({ type: 6, index: a }), i.removeAttribute(h));
        if (ft.test(i.tagName)) {
          const h = i.textContent.split(v), _ = h.length - 1;
          if (_ > 0) {
            i.textContent = j ? j.emptyScript : "";
            for (let f = 0; f < _; f++) i.append(h[f], D()), b.nextNode(), o.push({ type: 2, index: ++a });
            i.append(h[_], D());
          }
        }
      } else if (i.nodeType === 8) if (i.data === $t) o.push({ type: 2, index: a });
      else {
        let h = -1;
        for (; (h = i.data.indexOf(v, h + 1)) !== -1; ) o.push({ type: 7, index: a }), h += v.length - 1;
      }
      a++;
    }
  }
  static createElement(t, e) {
    const s = S.createElement("template");
    return s.innerHTML = t, s;
  }
}
function U(r, t, e = r, s) {
  var n, l;
  if (t === P) return t;
  let i = s !== void 0 ? (n = e._$Co) == null ? void 0 : n[s] : e._$Cl;
  const a = H(t) ? void 0 : t._$litDirective$;
  return (i == null ? void 0 : i.constructor) !== a && ((l = i == null ? void 0 : i._$AO) == null || l.call(i, !1), a === void 0 ? i = void 0 : (i = new a(r), i._$AT(r, e, s)), s !== void 0 ? (e._$Co ?? (e._$Co = []))[s] = i : e._$Cl = i), i !== void 0 && (t = U(r, i._$AS(r, t.values), i, s)), t;
}
class Dt {
  constructor(t, e) {
    this._$AV = [], this._$AN = void 0, this._$AD = t, this._$AM = e;
  }
  get parentNode() {
    return this._$AM.parentNode;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  u(t) {
    const { el: { content: e }, parts: s } = this._$AD, i = ((t == null ? void 0 : t.creationScope) ?? S).importNode(e, !0);
    b.currentNode = i;
    let a = b.nextNode(), n = 0, l = 0, o = s[0];
    for (; o !== void 0; ) {
      if (n === o.index) {
        let u;
        o.type === 2 ? u = new I(a, a.nextSibling, this, t) : o.type === 1 ? u = new o.ctor(a, o.name, o.strings, this, t) : o.type === 6 && (u = new Nt(a, this, t)), this._$AV.push(u), o = s[++l];
      }
      n !== (o == null ? void 0 : o.index) && (a = b.nextNode(), n++);
    }
    return b.currentNode = S, i;
  }
  p(t) {
    let e = 0;
    for (const s of this._$AV) s !== void 0 && (s.strings !== void 0 ? (s._$AI(t, s, e), e += s.strings.length - 2) : s._$AI(t[e])), e++;
  }
}
class I {
  get _$AU() {
    var t;
    return ((t = this._$AM) == null ? void 0 : t._$AU) ?? this._$Cv;
  }
  constructor(t, e, s, i) {
    this.type = 2, this._$AH = c, this._$AN = void 0, this._$AA = t, this._$AB = e, this._$AM = s, this.options = i, this._$Cv = (i == null ? void 0 : i.isConnected) ?? !0;
  }
  get parentNode() {
    let t = this._$AA.parentNode;
    const e = this._$AM;
    return e !== void 0 && (t == null ? void 0 : t.nodeType) === 11 && (t = e.parentNode), t;
  }
  get startNode() {
    return this._$AA;
  }
  get endNode() {
    return this._$AB;
  }
  _$AI(t, e = this) {
    t = U(this, t, e), H(t) ? t === c || t == null || t === "" ? (this._$AH !== c && this._$AR(), this._$AH = c) : t !== this._$AH && t !== P && this._(t) : t._$litType$ !== void 0 ? this.$(t) : t.nodeType !== void 0 ? this.T(t) : kt(t) ? this.k(t) : this._(t);
  }
  O(t) {
    return this._$AA.parentNode.insertBefore(t, this._$AB);
  }
  T(t) {
    this._$AH !== t && (this._$AR(), this._$AH = this.O(t));
  }
  _(t) {
    this._$AH !== c && H(this._$AH) ? this._$AA.nextSibling.data = t : this.T(S.createTextNode(t)), this._$AH = t;
  }
  $(t) {
    var a;
    const { values: e, _$litType$: s } = t, i = typeof s == "number" ? this._$AC(t) : (s.el === void 0 && (s.el = M.createElement(vt(s.h, s.h[0]), this.options)), s);
    if (((a = this._$AH) == null ? void 0 : a._$AD) === i) this._$AH.p(e);
    else {
      const n = new Dt(i, this), l = n.u(this.options);
      n.p(e), this.T(l), this._$AH = n;
    }
  }
  _$AC(t) {
    let e = ht.get(t.strings);
    return e === void 0 && ht.set(t.strings, e = new M(t)), e;
  }
  k(t) {
    Y(this._$AH) || (this._$AH = [], this._$AR());
    const e = this._$AH;
    let s, i = 0;
    for (const a of t) i === e.length ? e.push(s = new I(this.O(D()), this.O(D()), this, this.options)) : s = e[i], s._$AI(a), i++;
    i < e.length && (this._$AR(s && s._$AB.nextSibling, i), e.length = i);
  }
  _$AR(t = this._$AA.nextSibling, e) {
    var s;
    for ((s = this._$AP) == null ? void 0 : s.call(this, !1, !0, e); t !== this._$AB; ) {
      const i = rt(t).nextSibling;
      rt(t).remove(), t = i;
    }
  }
  setConnected(t) {
    var e;
    this._$AM === void 0 && (this._$Cv = t, (e = this._$AP) == null || e.call(this, t));
  }
}
class B {
  get tagName() {
    return this.element.tagName;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  constructor(t, e, s, i, a) {
    this.type = 1, this._$AH = c, this._$AN = void 0, this.element = t, this.name = e, this._$AM = i, this.options = a, s.length > 2 || s[0] !== "" || s[1] !== "" ? (this._$AH = Array(s.length - 1).fill(new String()), this.strings = s) : this._$AH = c;
  }
  _$AI(t, e = this, s, i) {
    const a = this.strings;
    let n = !1;
    if (a === void 0) t = U(this, t, e, 0), n = !H(t) || t !== this._$AH && t !== P, n && (this._$AH = t);
    else {
      const l = t;
      let o, u;
      for (t = a[0], o = 0; o < a.length - 1; o++) u = U(this, l[s + o], e, o), u === P && (u = this._$AH[o]), n || (n = !H(u) || u !== this._$AH[o]), u === c ? t = c : t !== c && (t += (u ?? "") + a[o + 1]), this._$AH[o] = u;
    }
    n && !i && this.j(t);
  }
  j(t) {
    t === c ? this.element.removeAttribute(this.name) : this.element.setAttribute(this.name, t ?? "");
  }
}
class Ht extends B {
  constructor() {
    super(...arguments), this.type = 3;
  }
  j(t) {
    this.element[this.name] = t === c ? void 0 : t;
  }
}
class Mt extends B {
  constructor() {
    super(...arguments), this.type = 4;
  }
  j(t) {
    this.element.toggleAttribute(this.name, !!t && t !== c);
  }
}
class It extends B {
  constructor(t, e, s, i, a) {
    super(t, e, s, i, a), this.type = 5;
  }
  _$AI(t, e = this) {
    if ((t = U(this, t, e, 0) ?? c) === P) return;
    const s = this._$AH, i = t === c && s !== c || t.capture !== s.capture || t.once !== s.once || t.passive !== s.passive, a = t !== c && (s === c || i);
    i && this.element.removeEventListener(this.name, this, s), a && this.element.addEventListener(this.name, this, t), this._$AH = t;
  }
  handleEvent(t) {
    var e;
    typeof this._$AH == "function" ? this._$AH.call(((e = this.options) == null ? void 0 : e.host) ?? this.element, t) : this._$AH.handleEvent(t);
  }
}
class Nt {
  constructor(t, e, s) {
    this.element = t, this.type = 6, this._$AN = void 0, this._$AM = e, this.options = s;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  _$AI(t) {
    U(this, t);
  }
}
const J = T.litHtmlPolyfillSupport;
J == null || J(M, I), (T.litHtmlVersions ?? (T.litHtmlVersions = [])).push("3.3.2");
const Rt = (r, t, e) => {
  const s = (e == null ? void 0 : e.renderBefore) ?? t;
  let i = s._$litPart$;
  if (i === void 0) {
    const a = (e == null ? void 0 : e.renderBefore) ?? null;
    s._$litPart$ = i = new I(t.insertBefore(D(), a), a, void 0, e ?? {});
  }
  return i._$AI(r), i;
};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const A = globalThis;
class x extends C {
  constructor() {
    super(...arguments), this.renderOptions = { host: this }, this._$Do = void 0;
  }
  createRenderRoot() {
    var e;
    const t = super.createRenderRoot();
    return (e = this.renderOptions).renderBefore ?? (e.renderBefore = t.firstChild), t;
  }
  update(t) {
    const e = this.render();
    this.hasUpdated || (this.renderOptions.isConnected = this.isConnected), super.update(t), this._$Do = Rt(e, this.renderRoot, this.renderOptions);
  }
  connectedCallback() {
    var t;
    super.connectedCallback(), (t = this._$Do) == null || t.setConnected(!0);
  }
  disconnectedCallback() {
    var t;
    super.disconnectedCallback(), (t = this._$Do) == null || t.setConnected(!1);
  }
  render() {
    return P;
  }
}
var ut;
x._$litElement$ = !0, x.finalized = !0, (ut = A.litElementHydrateSupport) == null || ut.call(A, { LitElement: x });
const G = A.litElementPolyfillSupport;
G == null || G({ LitElement: x });
(A.litElementVersions ?? (A.litElementVersions = [])).push("4.2.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const mt = (r) => (t, e) => {
  e !== void 0 ? e.addInitializer(() => {
    customElements.define(r, t);
  }) : customElements.define(r, t);
};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const zt = { attribute: !0, type: String, converter: L, reflect: !1, hasChanged: K }, Lt = (r = zt, t, e) => {
  const { kind: s, metadata: i } = e;
  let a = globalThis.litPropertyMetadata.get(i);
  if (a === void 0 && globalThis.litPropertyMetadata.set(i, a = /* @__PURE__ */ new Map()), s === "setter" && ((r = Object.create(r)).wrapped = !0), a.set(e.name, r), s === "accessor") {
    const { name: n } = e;
    return { set(l) {
      const o = t.get.call(this);
      t.set.call(this, l), this.requestUpdate(n, o, r, !0, l);
    }, init(l) {
      return l !== void 0 && this.C(n, void 0, r, l), l;
    } };
  }
  if (s === "setter") {
    const { name: n } = e;
    return function(l) {
      const o = this[n];
      t.call(this, l), this.requestUpdate(n, o, r, !0, l);
    };
  }
  throw Error("Unsupported decorator location: " + s);
};
function Q(r) {
  return (t, e) => typeof e == "object" ? Lt(r, t, e) : ((s, i, a) => {
    const n = i.hasOwnProperty(a);
    return i.constructor.createProperty(a, s), n ? Object.getOwnPropertyDescriptor(i, a) : void 0;
  })(r, t, e);
}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
function g(r) {
  return Q({ ...r, state: !0, attribute: !1 });
}
const X = "/umbraco/management/api/v1/umbcheckout", gt = "/umbraco/management/api/v1/umbhost-licensing";
async function V(r, t) {
  const e = await fetch(r, {
    ...t,
    headers: {
      "Content-Type": "application/json",
      ...t == null ? void 0 : t.headers
    }
  });
  if (!e.ok) throw new Error(`HTTP ${e.status}`);
  return e.json();
}
async function jt() {
  return V(`${gt}/license-status`);
}
async function Bt() {
  return V(`${X}/configuration`);
}
async function Vt() {
  return V(`${X}/configuration/has-url-set`);
}
async function qt(r) {
  await fetch(`${X}/configuration`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(r)
  });
}
async function Wt() {
  await V(`${gt}/licence-check`);
}
var Jt = Object.defineProperty, Gt = Object.getOwnPropertyDescriptor, E = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? Gt(t, e) : t, a = r.length - 1, n; a >= 0; a--)
    (n = r[a]) && (i = (s ? n(t, e, i) : n(i)) || i);
  return s && i && Jt(t, e, i), i;
};
let $ = class extends x {
  constructor() {
    super(...arguments), this._properties = [], this._licenseState = null, this._hasUmbracoUrl = !0, this._umbracoUrlMessage = "", this._saveState = "init", this._checkLicenseState = "init";
  }
  async connectedCallback() {
    super.connectedCallback(), await this._loadData();
  }
  async _loadData() {
    try {
      const [r, t, e] = await Promise.all([
        Bt(),
        Vt(),
        jt()
      ]);
      this._properties = r, this._hasUmbracoUrl = t, this._licenseState = e;
    } catch (r) {
      console.error("Failed to load dashboard data", r);
    }
  }
  async _saveConfiguration() {
    this._saveState = "busy";
    try {
      const r = {};
      for (const t of this._properties)
        r[t.alias] = t.value;
      await qt(r), this._saveState = "success", setTimeout(() => this._saveState = "init", 2e3);
    } catch {
      this._saveState = "error", setTimeout(() => this._saveState = "init", 2e3);
    }
  }
  async _checkLicense() {
    this._checkLicenseState = "busy";
    try {
      await Wt(), this._checkLicenseState = "success", setTimeout(() => window.location.reload(), 1500);
    } catch {
      this._checkLicenseState = "error", setTimeout(() => this._checkLicenseState = "init", 2e3);
    }
  }
  _onPropertyChange(r, t) {
    this._properties = this._properties.map(
      (e) => e.alias === r ? { ...e, value: t } : e
    );
  }
  _renderProperty(r) {
    return p`
      <div class="property-row">
        <div class="property-label">${r.label}</div>
        <div class="property-description">${r.description}</div>
        ${r.view === "boolean" ? p`<uui-toggle
              .checked=${r.value === !0 || r.value === "true" || r.value === "1"}
              @change=${(t) => this._onPropertyChange(r.alias, t.target.checked)}
            ></uui-toggle>` : r.view === "integer" ? p`<uui-input
                type="number"
                .value=${r.value ?? ""}
                @change=${(t) => this._onPropertyChange(r.alias, t.target.value)}
              ></uui-input>` : p`<uui-input
                .value=${r.value ?? ""}
                @change=${(t) => this._onPropertyChange(r.alias, t.target.value)}
              ></uui-input>`}
      </div>
    `;
  }
  render() {
    var r, t, e;
    return p`
      <umb-body-layout header-transparent>
        <div slot="header">
          <h3>UmbCheckout</h3>
        </div>

        <div class="dashboard-overview">
          <div class="left-col">
            ${this._hasUmbracoUrl ? c : p`<uui-box headline="Warning">
                  <div class="warning-box">
                    The "Umbraco Application Url" configuration setting is unset.
                    <a href="https://docs.umbraco.com/umbraco-cms/reference/configuration/webroutingsettings#umbraco-application-url" target="_blank">
                      Set the value here</a>. This is required to match your license.
                  </div>
                </uui-box>`}

            <uui-box headline="Help &amp; Support">
              <p>Documentation available at <a href="https://docs.umbcheckout.net/" target="_blank">https://docs.umbcheckout.net/</a></p>
              <p>Please report any issues in the bug tracker found on the <a target="_blank" href="https://github.com/UmbHost/UmbCheckout/issues">GitHub Repo</a></p>
              <p>Open a support ticket through <a target="_blank" href="https://my.umbhost.net/submitticket.php?step=2&deptid=8&subject=UmbCheckout:%20[ISSUE%20NAME%20HERE]">My.UmbHost</a></p>
            </uui-box>

            <uui-box headline="Configuration">
              ${this._properties.map((s) => this._renderProperty(s))}
            </uui-box>
          </div>

          <div class="right-col">
            ${this._licenseState ? p`
                  <uui-box headline="License Details">
                    ${this._licenseState.isDevelopment ? p`<div class="license-row">
                          <span class="license-label">Development License</span>
                          <span>Yes</span>
                        </div>` : c}
                    <div class="license-row">
                      <span class="license-label">License Status</span>
                      <span>${this._licenseState.status}</span>
                    </div>
                    <div class="license-row">
                      <span class="license-label">Registration Date</span>
                      <span>${this._licenseState.registrationDate}</span>
                    </div>
                    ${this._licenseState.expiryDate ? p`<div class="license-row">
                          <span class="license-label">Expiry Date</span>
                          <span>${this._licenseState.expiryDate}</span>
                        </div>` : c}
                    <div class="license-row">
                      <span class="license-label">Valid Domains</span>
                      <span>${(r = this._licenseState.validDomains) == null ? void 0 : r.join(", ")}</span>
                    </div>
                    <div class="license-row">
                      <span class="license-label">Valid Paths</span>
                      <span>${(t = this._licenseState.validPaths) == null ? void 0 : t.join(", ")}</span>
                    </div>
                    ${(e = this._licenseState.addons) == null ? void 0 : e.map(
      (s) => p`
                        <div class="addon-item">
                          <span>${s.name}</span>
                          <span>${s.status}</span>
                        </div>
                      `
    )}
                    <uui-button
                      look="secondary"
                      label="Re-Check License"
                      .state=${this._checkLicenseState === "busy" ? "waiting" : void 0}
                      @click=${this._checkLicense}
                    >
                      Re-Check License
                    </uui-button>
                  </uui-box>
                ` : c}
          </div>
        </div>

        <div slot="footer">
          <div class="footer">
            <uui-button
              look="primary"
              color="positive"
              label="Save"
              .state=${this._saveState === "busy" ? "waiting" : this._saveState === "success" ? "success" : this._saveState === "error" ? "failed" : void 0}
              @click=${this._saveConfiguration}
            >
              Save
            </uui-button>
          </div>
        </div>
      </umb-body-layout>
    `;
  }
};
$.styles = pt`
    :host {
      display: block;
      padding: var(--uui-size-layout-1);
    }
    .dashboard-overview {
      display: flex;
      gap: var(--uui-size-layout-1);
    }
    .left-col {
      flex: 1 1 auto;
      width: calc(100% - 370px);
    }
    .right-col {
      flex: 0 0 350px;
      position: sticky;
      top: 0;
      align-self: flex-start;
    }
    uui-box {
      margin-bottom: var(--uui-size-layout-1);
    }
    .property-row {
      padding: var(--uui-size-space-4) 0;
      border-bottom: 1px solid var(--uui-color-border);
    }
    .property-label {
      font-weight: bold;
      margin-bottom: var(--uui-size-space-2);
    }
    .property-description {
      color: var(--uui-color-text-alt);
      font-size: 0.85em;
      margin-bottom: var(--uui-size-space-2);
    }
    .license-row {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-2) 0;
    }
    .license-label {
      font-weight: bold;
    }
    .addon-item {
      display: flex;
      justify-content: space-between;
      padding: var(--uui-size-space-2) 0;
      border-top: 1px solid var(--uui-color-border);
    }
    .warning-box {
      background: var(--uui-color-warning);
      padding: var(--uui-size-space-4);
      border-radius: var(--uui-border-radius);
      margin-bottom: var(--uui-size-space-4);
    }
    .footer {
      display: flex;
      justify-content: flex-end;
      padding: var(--uui-size-space-4) 0;
    }
  `;
E([
  g()
], $.prototype, "_properties", 2);
E([
  g()
], $.prototype, "_licenseState", 2);
E([
  g()
], $.prototype, "_hasUmbracoUrl", 2);
E([
  g()
], $.prototype, "_umbracoUrlMessage", 2);
E([
  g()
], $.prototype, "_saveState", 2);
E([
  g()
], $.prototype, "_checkLicenseState", 2);
$ = E([
  mt("umbcheckout-dashboard")
], $);
var Zt = Object.defineProperty, Ft = Object.getOwnPropertyDescriptor, N = (r, t, e, s) => {
  for (var i = s > 1 ? void 0 : s ? Ft(t, e) : t, a = r.length - 1, n; a >= 0; a--)
    (n = r[a]) && (i = (s ? n(t, e, i) : n(i)) || i);
  return s && i && Zt(t, e, i), i;
};
let w = class extends x {
  constructor() {
    super(...arguments), this.value = [], this.readonly = !1, this._confirmIndex = null, this._dragIndex = null;
  }
  _dispatchChange() {
    this.dispatchEvent(
      new CustomEvent("change", {
        detail: { value: this.value.filter((r) => r.name || r.value) },
        bubbles: !0,
        composed: !0
      })
    );
  }
  _addItem() {
    this.value = [...this.value, { name: "", value: "" }], this._dispatchChange();
  }
  _removeItem(r) {
    this.value = this.value.filter((t, e) => e !== r), this._confirmIndex = null, this._dispatchChange();
  }
  _updateItem(r, t, e) {
    this.value = this.value.map(
      (s, i) => i === r ? { ...s, [t]: e } : s
    ), this._dispatchChange();
  }
  _onDragStart(r, t) {
    this._dragIndex = t, r.dataTransfer && (r.dataTransfer.effectAllowed = "move");
  }
  _onDragOver(r, t) {
    if (r.preventDefault(), this._dragIndex === null || this._dragIndex === t) return;
    const e = [...this.value], [s] = e.splice(this._dragIndex, 1);
    e.splice(t, 0, s), this.value = e, this._dragIndex = t;
  }
  _onDragEnd() {
    this._dragIndex = null, this._dispatchChange();
  }
  render() {
    return p`
      ${this.value.map(
      (r, t) => p`
          <div
            class="item-row ${this._dragIndex === t ? "dragging" : ""}"
            draggable=${this.readonly ? "false" : "true"}
            @dragstart=${(e) => this._onDragStart(e, t)}
            @dragover=${(e) => this._onDragOver(e, t)}
            @dragend=${this._onDragEnd}
          >
            ${this.readonly ? c : p`<uui-icon name="icon-navigation" class="drag-handle"></uui-icon>`}
            <div class="inputs">
              <uui-input
                placeholder="Name"
                .value=${r.name}
                ?readonly=${this.readonly}
                @change=${(e) => this._updateItem(t, "name", e.target.value)}
              ></uui-input>
              <uui-input
                placeholder="Value"
                .value=${r.value}
                ?readonly=${this.readonly}
                @change=${(e) => this._updateItem(t, "value", e.target.value)}
              ></uui-input>
            </div>
            ${this.readonly ? c : this._confirmIndex === t ? p`
                    <div class="confirm-prompt">
                      <uui-button
                        look="primary"
                        color="danger"
                        compact
                        label="Confirm"
                        @click=${() => this._removeItem(t)}
                      >
                        Confirm
                      </uui-button>
                      <uui-button
                        compact
                        label="Cancel"
                        @click=${() => this._confirmIndex = null}
                      >
                        Cancel
                      </uui-button>
                    </div>
                  ` : p`
                    <uui-button
                      look="default"
                      color="danger"
                      compact
                      label="Remove"
                      @click=${() => this._confirmIndex = t}
                    >
                      <uui-icon name="icon-trash"></uui-icon>
                    </uui-button>
                  `}
          </div>
        `
    )}
      ${this.readonly ? c : p`
            <uui-button
              class="add-button"
              look="placeholder"
              label="Add"
              @click=${this._addItem}
            >
              Add
            </uui-button>
          `}
    `;
  }
};
w.styles = pt`
    :host {
      display: block;
    }
    .item-row {
      display: flex;
      align-items: center;
      gap: var(--uui-size-space-3);
      padding: var(--uui-size-space-3) 0;
      border-bottom: 1px solid var(--uui-color-border);
    }
    .drag-handle {
      cursor: move;
      opacity: 0.5;
    }
    .drag-handle:hover {
      opacity: 1;
    }
    .inputs {
      display: flex;
      flex: 1;
      gap: var(--uui-size-space-3);
      flex-wrap: wrap;
    }
    .inputs uui-input {
      flex: 1;
      min-width: 150px;
    }
    .add-button {
      margin-top: var(--uui-size-space-3);
    }
    .confirm-prompt {
      display: flex;
      gap: var(--uui-size-space-2);
      align-items: center;
    }
    .dragging {
      opacity: 0.5;
    }
  `;
N([
  Q({ type: Array })
], w.prototype, "value", 2);
N([
  Q({ type: Boolean })
], w.prototype, "readonly", 2);
N([
  g()
], w.prototype, "_confirmIndex", 2);
N([
  g()
], w.prototype, "_dragIndex", 2);
w = N([
  mt("umbcheckout-metadata-editor")
], w);
//# sourceMappingURL=umbcheckout-backoffice.js.map

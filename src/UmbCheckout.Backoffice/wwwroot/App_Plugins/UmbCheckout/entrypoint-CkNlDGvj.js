import { c as R } from "./api-client-B3b-kGib.js";
const re = {
  bodySerializer: (r) => JSON.stringify(
    r,
    (e, t) => typeof t == "bigint" ? t.toString() : t
  )
}, se = ({
  onRequest: r,
  onSseError: e,
  onSseEvent: t,
  responseTransformer: o,
  responseValidator: n,
  sseDefaultRetryDelay: l,
  sseMaxRetryAttempts: c,
  sseMaxRetryDelay: a,
  sseSleepFn: i,
  url: u,
  ...s
}) => {
  let d;
  const E = i ?? ((f) => new Promise((y) => setTimeout(y, f)));
  return { stream: async function* () {
    let f = l ?? 3e3, y = 0;
    const S = s.signal ?? new AbortController().signal;
    for (; !S.aborted; ) {
      y++;
      const z = s.headers instanceof Headers ? s.headers : new Headers(s.headers);
      d !== void 0 && z.set("Last-Event-ID", d);
      try {
        const $ = {
          redirect: "follow",
          ...s,
          body: s.serializedBody,
          headers: z,
          signal: S
        };
        let m = new Request(u, $);
        r && (m = await r(u, $));
        const p = await (s.fetch ?? globalThis.fetch)(m);
        if (!p.ok)
          throw new Error(
            `SSE failed: ${p.status} ${p.statusText}`
          );
        if (!p.body) throw new Error("No body in SSE response");
        const w = p.body.pipeThrough(new TextDecoderStream()).getReader();
        let C = "";
        const O = () => {
          try {
            w.cancel();
          } catch {
          }
        };
        S.addEventListener("abort", O);
        try {
          for (; ; ) {
            const { done: D, value: N } = await w.read();
            if (D) break;
            C += N;
            const A = C.split(`

`);
            C = A.pop() ?? "";
            for (const P of A) {
              const v = P.split(`
`), q = [];
              let T;
              for (const b of v)
                if (b.startsWith("data:"))
                  q.push(b.replace(/^data:\s*/, ""));
                else if (b.startsWith("event:"))
                  T = b.replace(/^event:\s*/, "");
                else if (b.startsWith("id:"))
                  d = b.replace(/^id:\s*/, "");
                else if (b.startsWith("retry:")) {
                  const I = Number.parseInt(
                    b.replace(/^retry:\s*/, ""),
                    10
                  );
                  Number.isNaN(I) || (f = I);
                }
              let j, k = !1;
              if (q.length) {
                const b = q.join(`
`);
                try {
                  j = JSON.parse(b), k = !0;
                } catch {
                  j = b;
                }
              }
              k && (n && await n(j), o && (j = await o(j))), t?.({
                data: j,
                event: T,
                id: d,
                retry: f
              }), q.length && (yield j);
            }
          }
        } finally {
          S.removeEventListener("abort", O), w.releaseLock();
        }
        break;
      } catch ($) {
        if (e?.($), c !== void 0 && y >= c)
          break;
        const m = Math.min(
          f * 2 ** (y - 1),
          a ?? 3e4
        );
        await E(m);
      }
    }
  }() };
}, ae = (r) => {
  switch (r) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, ne = (r) => {
  switch (r) {
    case "form":
      return ",";
    case "pipeDelimited":
      return "|";
    case "spaceDelimited":
      return "%20";
    default:
      return ",";
  }
}, ie = (r) => {
  switch (r) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, _ = ({
  allowReserved: r,
  explode: e,
  name: t,
  style: o,
  value: n
}) => {
  if (!e) {
    const a = (r ? n : n.map((i) => encodeURIComponent(i))).join(ne(o));
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
  const l = ae(o), c = n.map((a) => o === "label" || o === "simple" ? r ? a : encodeURIComponent(a) : B({
    allowReserved: r,
    name: t,
    value: a
  })).join(l);
  return o === "label" || o === "matrix" ? l + c : c;
}, B = ({
  allowReserved: r,
  name: e,
  value: t
}) => {
  if (t == null)
    return "";
  if (typeof t == "object")
    throw new Error(
      "Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these."
    );
  return `${e}=${r ? t : encodeURIComponent(t)}`;
}, G = ({
  allowReserved: r,
  explode: e,
  name: t,
  style: o,
  value: n,
  valueOnly: l
}) => {
  if (n instanceof Date)
    return l ? n.toISOString() : `${t}=${n.toISOString()}`;
  if (o !== "deepObject" && !e) {
    let i = [];
    Object.entries(n).forEach(([s, d]) => {
      i = [
        ...i,
        s,
        r ? d : encodeURIComponent(d)
      ];
    });
    const u = i.join(",");
    switch (o) {
      case "form":
        return `${t}=${u}`;
      case "label":
        return `.${u}`;
      case "matrix":
        return `;${t}=${u}`;
      default:
        return u;
    }
  }
  const c = ie(o), a = Object.entries(n).map(
    ([i, u]) => B({
      allowReserved: r,
      name: o === "deepObject" ? `${t}[${i}]` : i,
      value: u
    })
  ).join(c);
  return o === "label" || o === "matrix" ? c + a : a;
}, oe = /\{[^{}]+\}/g, ce = ({ path: r, url: e }) => {
  let t = e;
  const o = e.match(oe);
  if (o)
    for (const n of o) {
      let l = !1, c = n.substring(1, n.length - 1), a = "simple";
      c.endsWith("*") && (l = !0, c = c.substring(0, c.length - 1)), c.startsWith(".") ? (c = c.substring(1), a = "label") : c.startsWith(";") && (c = c.substring(1), a = "matrix");
      const i = r[c];
      if (i == null)
        continue;
      if (Array.isArray(i)) {
        t = t.replace(
          n,
          _({ explode: l, name: c, style: a, value: i })
        );
        continue;
      }
      if (typeof i == "object") {
        t = t.replace(
          n,
          G({
            explode: l,
            name: c,
            style: a,
            value: i,
            valueOnly: !0
          })
        );
        continue;
      }
      if (a === "matrix") {
        t = t.replace(
          n,
          `;${B({
            name: c,
            value: i
          })}`
        );
        continue;
      }
      const u = encodeURIComponent(
        a === "label" ? `.${i}` : i
      );
      t = t.replace(n, u);
    }
  return t;
}, le = ({
  baseUrl: r,
  path: e,
  query: t,
  querySerializer: o,
  url: n
}) => {
  const l = n.startsWith("/") ? n : `/${n}`;
  let c = (r ?? "") + l;
  e && (c = ce({ path: e, url: c }));
  let a = t ? o(t) : "";
  return a.startsWith("?") && (a = a.substring(1)), a && (c += `?${a}`), c;
};
function fe(r) {
  const e = r.body !== void 0;
  if (e && r.bodySerializer)
    return "serializedBody" in r ? r.serializedBody !== void 0 && r.serializedBody !== "" ? r.serializedBody : null : r.body !== "" ? r.body : null;
  if (e)
    return r.body;
}
const ue = async (r, e) => {
  const t = typeof e == "function" ? await e(r) : e;
  if (t)
    return r.scheme === "bearer" ? `Bearer ${t}` : r.scheme === "basic" ? `Basic ${btoa(t)}` : t;
}, M = ({
  allowReserved: r,
  array: e,
  object: t
} = {}) => (n) => {
  const l = [];
  if (n && typeof n == "object")
    for (const c in n) {
      const a = n[c];
      if (a != null)
        if (Array.isArray(a)) {
          const i = _({
            allowReserved: r,
            explode: !0,
            name: c,
            style: "form",
            value: a,
            ...e
          });
          i && l.push(i);
        } else if (typeof a == "object") {
          const i = G({
            allowReserved: r,
            explode: !0,
            name: c,
            style: "deepObject",
            value: a,
            ...t
          });
          i && l.push(i);
        } else {
          const i = B({
            allowReserved: r,
            name: c,
            value: a
          });
          i && l.push(i);
        }
    }
  return l.join("&");
}, de = (r) => {
  if (!r)
    return "stream";
  const e = r.split(";")[0]?.trim();
  if (e) {
    if (e.startsWith("application/json") || e.endsWith("+json"))
      return "json";
    if (e === "multipart/form-data")
      return "formData";
    if (["application/", "audio/", "image/", "video/"].some(
      (t) => e.startsWith(t)
    ))
      return "blob";
    if (e.startsWith("text/"))
      return "text";
  }
}, he = (r, e) => e ? !!(r.headers.has(e) || r.query?.[e] || r.headers.get("Cookie")?.includes(`${e}=`)) : !1, ye = async ({
  security: r,
  ...e
}) => {
  for (const t of r) {
    if (he(e, t.name))
      continue;
    const o = await ue(t, e.auth);
    if (!o)
      continue;
    const n = t.name ?? "Authorization";
    switch (t.in) {
      case "query":
        e.query || (e.query = {}), e.query[n] = o;
        break;
      case "cookie":
        e.headers.append("Cookie", `${n}=${o}`);
        break;
      default:
        e.headers.set(n, o);
        break;
    }
  }
}, V = (r) => le({
  baseUrl: r.baseUrl,
  path: r.path,
  query: r.query,
  querySerializer: typeof r.querySerializer == "function" ? r.querySerializer : M(r.querySerializer),
  url: r.url
}), L = (r, e) => {
  const t = { ...r, ...e };
  return t.baseUrl?.endsWith("/") && (t.baseUrl = t.baseUrl.substring(0, t.baseUrl.length - 1)), t.headers = Q(r.headers, e.headers), t;
}, pe = (r) => {
  const e = [];
  return r.forEach((t, o) => {
    e.push([o, t]);
  }), e;
}, Q = (...r) => {
  const e = new Headers();
  for (const t of r) {
    if (!t)
      continue;
    const o = t instanceof Headers ? pe(t) : Object.entries(t);
    for (const [n, l] of o)
      if (l === null)
        e.delete(n);
      else if (Array.isArray(l))
        for (const c of l)
          e.append(n, c);
      else l !== void 0 && e.set(
        n,
        typeof l == "object" ? JSON.stringify(l) : l
      );
  }
  return e;
};
let W = class {
  constructor() {
    this.fns = [];
  }
  clear() {
    this.fns = [];
  }
  eject(e) {
    const t = this.getInterceptorIndex(e);
    this.fns[t] && (this.fns[t] = null);
  }
  exists(e) {
    const t = this.getInterceptorIndex(e);
    return !!this.fns[t];
  }
  getInterceptorIndex(e) {
    return typeof e == "number" ? this.fns[e] ? e : -1 : this.fns.indexOf(e);
  }
  update(e, t) {
    const o = this.getInterceptorIndex(e);
    return this.fns[o] ? (this.fns[o] = t, e) : !1;
  }
  use(e) {
    return this.fns.push(e), this.fns.length - 1;
  }
};
const be = () => ({
  error: new W(),
  request: new W(),
  response: new W()
}), me = M({
  allowReserved: !1,
  array: {
    explode: !0,
    style: "form"
  },
  object: {
    explode: !0,
    style: "deepObject"
  }
}), we = {
  "Content-Type": "application/json"
}, K = (r = {}) => ({
  ...re,
  headers: we,
  parseAs: "auto",
  querySerializer: me,
  ...r
}), ge = (r = {}) => {
  let e = L(K(), r);
  const t = () => ({ ...e }), o = (u) => (e = L(e, u), t()), n = be(), l = async (u) => {
    const s = {
      ...e,
      ...u,
      fetch: u.fetch ?? e.fetch ?? globalThis.fetch,
      headers: Q(e.headers, u.headers),
      serializedBody: void 0
    };
    s.security && await ye({
      ...s,
      security: s.security
    }), s.requestValidator && await s.requestValidator(s), s.body !== void 0 && s.bodySerializer && (s.serializedBody = s.bodySerializer(s.body)), (s.body === void 0 || s.serializedBody === "") && s.headers.delete("Content-Type");
    const d = V(s);
    return { opts: s, url: d };
  }, c = async (u) => {
    const { opts: s, url: d } = await l(u), E = {
      redirect: "follow",
      ...s,
      body: fe(s)
    };
    let g = new Request(d, E);
    for (const h of n.request.fns)
      h && (g = await h(g, s));
    const x = s.fetch;
    let f = await x(g);
    for (const h of n.response.fns)
      h && (f = await h(f, g, s));
    const y = {
      request: g,
      response: f
    };
    if (f.ok) {
      const h = (s.parseAs === "auto" ? de(f.headers.get("Content-Type")) : s.parseAs) ?? "json";
      if (f.status === 204 || f.headers.get("Content-Length") === "0") {
        let w;
        switch (h) {
          case "arrayBuffer":
          case "blob":
          case "text":
            w = await f[h]();
            break;
          case "formData":
            w = new FormData();
            break;
          case "stream":
            w = f.body;
            break;
          default:
            w = {};
            break;
        }
        return s.responseStyle === "data" ? w : {
          data: w,
          ...y
        };
      }
      let p;
      switch (h) {
        case "arrayBuffer":
        case "blob":
        case "formData":
        case "json":
        case "text":
          p = await f[h]();
          break;
        case "stream":
          return s.responseStyle === "data" ? f.body : {
            data: f.body,
            ...y
          };
      }
      return h === "json" && (s.responseValidator && await s.responseValidator(p), s.responseTransformer && (p = await s.responseTransformer(p))), s.responseStyle === "data" ? p : {
        data: p,
        ...y
      };
    }
    const S = await f.text();
    let z;
    try {
      z = JSON.parse(S);
    } catch {
    }
    const $ = z ?? S;
    let m = $;
    for (const h of n.error.fns)
      h && (m = await h($, f, g, s));
    if (m = m || {}, s.throwOnError)
      throw m;
    return s.responseStyle === "data" ? void 0 : {
      error: m,
      ...y
    };
  }, a = (u) => (s) => c({ ...s, method: u }), i = (u) => async (s) => {
    const { opts: d, url: E } = await l(s);
    return se({
      ...d,
      body: d.body,
      headers: d.headers,
      method: u,
      onRequest: async (g, x) => {
        let f = new Request(g, x);
        for (const y of n.request.fns)
          y && (f = await y(f, d));
        return f;
      },
      url: E
    });
  };
  return {
    buildUrl: V,
    connect: a("CONNECT"),
    delete: a("DELETE"),
    get: a("GET"),
    getConfig: t,
    head: a("HEAD"),
    interceptors: n,
    options: a("OPTIONS"),
    patch: a("PATCH"),
    post: a("POST"),
    put: a("PUT"),
    request: c,
    setConfig: o,
    sse: {
      connect: i("CONNECT"),
      delete: i("DELETE"),
      get: i("GET"),
      head: i("HEAD"),
      options: i("OPTIONS"),
      patch: i("PATCH"),
      post: i("POST"),
      put: i("PUT"),
      trace: i("TRACE")
    },
    trace: a("TRACE")
  };
}, Se = ge(K({
  baseUrl: "https://localhost:44380",
  throwOnError: !0
})), $e = {
  bodySerializer: (r) => JSON.stringify(
    r,
    (e, t) => typeof t == "bigint" ? t.toString() : t
  )
}, je = ({
  onRequest: r,
  onSseError: e,
  onSseEvent: t,
  responseTransformer: o,
  responseValidator: n,
  sseDefaultRetryDelay: l,
  sseMaxRetryAttempts: c,
  sseMaxRetryDelay: a,
  sseSleepFn: i,
  url: u,
  ...s
}) => {
  let d;
  const E = i ?? ((f) => new Promise((y) => setTimeout(y, f)));
  return { stream: async function* () {
    let f = l ?? 3e3, y = 0;
    const S = s.signal ?? new AbortController().signal;
    for (; !S.aborted; ) {
      y++;
      const z = s.headers instanceof Headers ? s.headers : new Headers(s.headers);
      d !== void 0 && z.set("Last-Event-ID", d);
      try {
        const $ = {
          redirect: "follow",
          ...s,
          body: s.serializedBody,
          headers: z,
          signal: S
        };
        let m = new Request(u, $);
        r && (m = await r(u, $));
        const p = await (s.fetch ?? globalThis.fetch)(m);
        if (!p.ok)
          throw new Error(
            `SSE failed: ${p.status} ${p.statusText}`
          );
        if (!p.body) throw new Error("No body in SSE response");
        const w = p.body.pipeThrough(new TextDecoderStream()).getReader();
        let C = "";
        const O = () => {
          try {
            w.cancel();
          } catch {
          }
        };
        S.addEventListener("abort", O);
        try {
          for (; ; ) {
            const { done: D, value: N } = await w.read();
            if (D) break;
            C += N;
            const A = C.split(`

`);
            C = A.pop() ?? "";
            for (const P of A) {
              const v = P.split(`
`), q = [];
              let T;
              for (const b of v)
                if (b.startsWith("data:"))
                  q.push(b.replace(/^data:\s*/, ""));
                else if (b.startsWith("event:"))
                  T = b.replace(/^event:\s*/, "");
                else if (b.startsWith("id:"))
                  d = b.replace(/^id:\s*/, "");
                else if (b.startsWith("retry:")) {
                  const I = Number.parseInt(
                    b.replace(/^retry:\s*/, ""),
                    10
                  );
                  Number.isNaN(I) || (f = I);
                }
              let j, k = !1;
              if (q.length) {
                const b = q.join(`
`);
                try {
                  j = JSON.parse(b), k = !0;
                } catch {
                  j = b;
                }
              }
              k && (n && await n(j), o && (j = await o(j))), t?.({
                data: j,
                event: T,
                id: d,
                retry: f
              }), q.length && (yield j);
            }
          }
        } finally {
          S.removeEventListener("abort", O), w.releaseLock();
        }
        break;
      } catch ($) {
        if (e?.($), c !== void 0 && y >= c)
          break;
        const m = Math.min(
          f * 2 ** (y - 1),
          a ?? 3e4
        );
        await E(m);
      }
    }
  }() };
}, Ee = (r) => {
  switch (r) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, ze = (r) => {
  switch (r) {
    case "form":
      return ",";
    case "pipeDelimited":
      return "|";
    case "spaceDelimited":
      return "%20";
    default:
      return ",";
  }
}, xe = (r) => {
  switch (r) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, X = ({
  allowReserved: r,
  explode: e,
  name: t,
  style: o,
  value: n
}) => {
  if (!e) {
    const a = (r ? n : n.map((i) => encodeURIComponent(i))).join(ze(o));
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
  const l = Ee(o), c = n.map((a) => o === "label" || o === "simple" ? r ? a : encodeURIComponent(a) : U({
    allowReserved: r,
    name: t,
    value: a
  })).join(l);
  return o === "label" || o === "matrix" ? l + c : c;
}, U = ({
  allowReserved: r,
  name: e,
  value: t
}) => {
  if (t == null)
    return "";
  if (typeof t == "object")
    throw new Error(
      "Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these."
    );
  return `${e}=${r ? t : encodeURIComponent(t)}`;
}, Y = ({
  allowReserved: r,
  explode: e,
  name: t,
  style: o,
  value: n,
  valueOnly: l
}) => {
  if (n instanceof Date)
    return l ? n.toISOString() : `${t}=${n.toISOString()}`;
  if (o !== "deepObject" && !e) {
    let i = [];
    Object.entries(n).forEach(([s, d]) => {
      i = [
        ...i,
        s,
        r ? d : encodeURIComponent(d)
      ];
    });
    const u = i.join(",");
    switch (o) {
      case "form":
        return `${t}=${u}`;
      case "label":
        return `.${u}`;
      case "matrix":
        return `;${t}=${u}`;
      default:
        return u;
    }
  }
  const c = xe(o), a = Object.entries(n).map(
    ([i, u]) => U({
      allowReserved: r,
      name: o === "deepObject" ? `${t}[${i}]` : i,
      value: u
    })
  ).join(c);
  return o === "label" || o === "matrix" ? c + a : a;
}, qe = /\{[^{}]+\}/g, Ce = ({ path: r, url: e }) => {
  let t = e;
  const o = e.match(qe);
  if (o)
    for (const n of o) {
      let l = !1, c = n.substring(1, n.length - 1), a = "simple";
      c.endsWith("*") && (l = !0, c = c.substring(0, c.length - 1)), c.startsWith(".") ? (c = c.substring(1), a = "label") : c.startsWith(";") && (c = c.substring(1), a = "matrix");
      const i = r[c];
      if (i == null)
        continue;
      if (Array.isArray(i)) {
        t = t.replace(
          n,
          X({ explode: l, name: c, style: a, value: i })
        );
        continue;
      }
      if (typeof i == "object") {
        t = t.replace(
          n,
          Y({
            explode: l,
            name: c,
            style: a,
            value: i,
            valueOnly: !0
          })
        );
        continue;
      }
      if (a === "matrix") {
        t = t.replace(
          n,
          `;${U({
            name: c,
            value: i
          })}`
        );
        continue;
      }
      const u = encodeURIComponent(
        a === "label" ? `.${i}` : i
      );
      t = t.replace(n, u);
    }
  return t;
}, Oe = ({
  baseUrl: r,
  path: e,
  query: t,
  querySerializer: o,
  url: n
}) => {
  const l = n.startsWith("/") ? n : `/${n}`;
  let c = (r ?? "") + l;
  e && (c = Ce({ path: e, url: c }));
  let a = t ? o(t) : "";
  return a.startsWith("?") && (a = a.substring(1)), a && (c += `?${a}`), c;
};
function Ae(r) {
  const e = r.body !== void 0;
  if (e && r.bodySerializer)
    return "serializedBody" in r ? r.serializedBody !== void 0 && r.serializedBody !== "" ? r.serializedBody : null : r.body !== "" ? r.body : null;
  if (e)
    return r.body;
}
const Te = async (r, e) => {
  const t = typeof e == "function" ? await e(r) : e;
  if (t)
    return r.scheme === "bearer" ? `Bearer ${t}` : r.scheme === "basic" ? `Basic ${btoa(t)}` : t;
}, Z = ({
  allowReserved: r,
  array: e,
  object: t
} = {}) => (n) => {
  const l = [];
  if (n && typeof n == "object")
    for (const c in n) {
      const a = n[c];
      if (a != null)
        if (Array.isArray(a)) {
          const i = X({
            allowReserved: r,
            explode: !0,
            name: c,
            style: "form",
            value: a,
            ...e
          });
          i && l.push(i);
        } else if (typeof a == "object") {
          const i = Y({
            allowReserved: r,
            explode: !0,
            name: c,
            style: "deepObject",
            value: a,
            ...t
          });
          i && l.push(i);
        } else {
          const i = U({
            allowReserved: r,
            name: c,
            value: a
          });
          i && l.push(i);
        }
    }
  return l.join("&");
}, ke = (r) => {
  if (!r)
    return "stream";
  const e = r.split(";")[0]?.trim();
  if (e) {
    if (e.startsWith("application/json") || e.endsWith("+json"))
      return "json";
    if (e === "multipart/form-data")
      return "formData";
    if (["application/", "audio/", "image/", "video/"].some(
      (t) => e.startsWith(t)
    ))
      return "blob";
    if (e.startsWith("text/"))
      return "text";
  }
}, Ie = (r, e) => e ? !!(r.headers.has(e) || r.query?.[e] || r.headers.get("Cookie")?.includes(`${e}=`)) : !1, Be = async ({
  security: r,
  ...e
}) => {
  for (const t of r) {
    if (Ie(e, t.name))
      continue;
    const o = await Te(t, e.auth);
    if (!o)
      continue;
    const n = t.name ?? "Authorization";
    switch (t.in) {
      case "query":
        e.query || (e.query = {}), e.query[n] = o;
        break;
      case "cookie":
        e.headers.append("Cookie", `${n}=${o}`);
        break;
      default:
        e.headers.set(n, o);
        break;
    }
  }
}, J = (r) => Oe({
  baseUrl: r.baseUrl,
  path: r.path,
  query: r.query,
  querySerializer: typeof r.querySerializer == "function" ? r.querySerializer : Z(r.querySerializer),
  url: r.url
}), F = (r, e) => {
  const t = { ...r, ...e };
  return t.baseUrl?.endsWith("/") && (t.baseUrl = t.baseUrl.substring(0, t.baseUrl.length - 1)), t.headers = ee(r.headers, e.headers), t;
}, Ue = (r) => {
  const e = [];
  return r.forEach((t, o) => {
    e.push([o, t]);
  }), e;
}, ee = (...r) => {
  const e = new Headers();
  for (const t of r) {
    if (!t)
      continue;
    const o = t instanceof Headers ? Ue(t) : Object.entries(t);
    for (const [n, l] of o)
      if (l === null)
        e.delete(n);
      else if (Array.isArray(l))
        for (const c of l)
          e.append(n, c);
      else l !== void 0 && e.set(
        n,
        typeof l == "object" ? JSON.stringify(l) : l
      );
  }
  return e;
};
class H {
  constructor() {
    this.fns = [];
  }
  clear() {
    this.fns = [];
  }
  eject(e) {
    const t = this.getInterceptorIndex(e);
    this.fns[t] && (this.fns[t] = null);
  }
  exists(e) {
    const t = this.getInterceptorIndex(e);
    return !!this.fns[t];
  }
  getInterceptorIndex(e) {
    return typeof e == "number" ? this.fns[e] ? e : -1 : this.fns.indexOf(e);
  }
  update(e, t) {
    const o = this.getInterceptorIndex(e);
    return this.fns[o] ? (this.fns[o] = t, e) : !1;
  }
  use(e) {
    return this.fns.push(e), this.fns.length - 1;
  }
}
const De = () => ({
  error: new H(),
  request: new H(),
  response: new H()
}), Ne = Z({
  allowReserved: !1,
  array: {
    explode: !0,
    style: "form"
  },
  object: {
    explode: !0,
    style: "deepObject"
  }
}), Pe = {
  "Content-Type": "application/json"
}, te = (r = {}) => ({
  ...$e,
  headers: Pe,
  parseAs: "auto",
  querySerializer: Ne,
  ...r
}), ve = (r = {}) => {
  let e = F(te(), r);
  const t = () => ({ ...e }), o = (u) => (e = F(e, u), t()), n = De(), l = async (u) => {
    const s = {
      ...e,
      ...u,
      fetch: u.fetch ?? e.fetch ?? globalThis.fetch,
      headers: ee(e.headers, u.headers),
      serializedBody: void 0
    };
    s.security && await Be({
      ...s,
      security: s.security
    }), s.requestValidator && await s.requestValidator(s), s.body !== void 0 && s.bodySerializer && (s.serializedBody = s.bodySerializer(s.body)), (s.body === void 0 || s.serializedBody === "") && s.headers.delete("Content-Type");
    const d = J(s);
    return { opts: s, url: d };
  }, c = async (u) => {
    const { opts: s, url: d } = await l(u), E = {
      redirect: "follow",
      ...s,
      body: Ae(s)
    };
    let g = new Request(d, E);
    for (const h of n.request.fns)
      h && (g = await h(g, s));
    const x = s.fetch;
    let f = await x(g);
    for (const h of n.response.fns)
      h && (f = await h(f, g, s));
    const y = {
      request: g,
      response: f
    };
    if (f.ok) {
      const h = (s.parseAs === "auto" ? ke(f.headers.get("Content-Type")) : s.parseAs) ?? "json";
      if (f.status === 204 || f.headers.get("Content-Length") === "0") {
        let w;
        switch (h) {
          case "arrayBuffer":
          case "blob":
          case "text":
            w = await f[h]();
            break;
          case "formData":
            w = new FormData();
            break;
          case "stream":
            w = f.body;
            break;
          default:
            w = {};
            break;
        }
        return s.responseStyle === "data" ? w : {
          data: w,
          ...y
        };
      }
      let p;
      switch (h) {
        case "arrayBuffer":
        case "blob":
        case "formData":
        case "json":
        case "text":
          p = await f[h]();
          break;
        case "stream":
          return s.responseStyle === "data" ? f.body : {
            data: f.body,
            ...y
          };
      }
      return h === "json" && (s.responseValidator && await s.responseValidator(p), s.responseTransformer && (p = await s.responseTransformer(p))), s.responseStyle === "data" ? p : {
        data: p,
        ...y
      };
    }
    const S = await f.text();
    let z;
    try {
      z = JSON.parse(S);
    } catch {
    }
    const $ = z ?? S;
    let m = $;
    for (const h of n.error.fns)
      h && (m = await h($, f, g, s));
    if (m = m || {}, s.throwOnError)
      throw m;
    return s.responseStyle === "data" ? void 0 : {
      error: m,
      ...y
    };
  }, a = (u) => (s) => c({ ...s, method: u }), i = (u) => async (s) => {
    const { opts: d, url: E } = await l(s);
    return je({
      ...d,
      body: d.body,
      headers: d.headers,
      method: u,
      onRequest: async (g, x) => {
        let f = new Request(g, x);
        for (const y of n.request.fns)
          y && (f = await y(f, d));
        return f;
      },
      url: E
    });
  };
  return {
    buildUrl: J,
    connect: a("CONNECT"),
    delete: a("DELETE"),
    get: a("GET"),
    getConfig: t,
    head: a("HEAD"),
    interceptors: n,
    options: a("OPTIONS"),
    patch: a("PATCH"),
    post: a("POST"),
    put: a("PUT"),
    request: c,
    setConfig: o,
    sse: {
      connect: i("CONNECT"),
      delete: i("DELETE"),
      get: i("GET"),
      head: i("HEAD"),
      options: i("OPTIONS"),
      patch: i("PATCH"),
      post: i("POST"),
      put: i("PUT"),
      trace: i("TRACE")
    },
    trace: a("TRACE")
  };
}, We = ve(te({
  baseUrl: "https://localhost:44380",
  throwOnError: !0
})), Ve = (r) => {
  R(r, Se), R(r, We);
}, Le = () => {
};
export {
  Ve as onInit,
  Le as onUnload
};
//# sourceMappingURL=entrypoint-CkNlDGvj.js.map

import { css as S, LitElement as U, when as u, nothing as C, html as s, repeat as A, state as d, customElement as T } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as D } from "@umbraco-cms/backoffice/element-api";
import { UmbTextStyles as E } from "@umbraco-cms/backoffice/style";
import { UUITextStyles as O } from "@umbraco-cms/backoffice/external/uui";
import { c as y, a as p } from "./entrypoint-CkUrIMau.js";
import { UMB_NOTIFICATION_CONTEXT as B } from "@umbraco-cms/backoffice/notification";
import { UmbLocalizationController as P } from "@umbraco-cms/backoffice/localization-api";
const I = S`
	${O}

    .red {
        color: #d42054;
    }

    .bold {
        font-weight: 700;
    }

    .underline {
        text-decoration: underline;
    }
`;
class $ {
  static checkLicence(e) {
    return ((e == null ? void 0 : e.client) ?? y).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/umbhost-licencing/v2.0/check-licence/check-licence",
      ...e
    });
  }
  static getLicenceStatus(e) {
    return ((e == null ? void 0 : e.client) ?? y).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/umbhost-licencing/v2.0/get-licence-status/get-licence-status",
      ...e
    });
  }
}
class q {
  static getConfiguration(e) {
    return ((e == null ? void 0 : e.client) ?? p).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/umbcheckout/v2.0/configuration-api/get-configuration",
      ...e
    });
  }
  static hasUmbracoApplicationUrlSet(e) {
    return ((e == null ? void 0 : e.client) ?? p).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/umbcheckout/v2.0/configuration-api/has-umbraco-application-url-set",
      ...e
    });
  }
  static updateConfiguration(e) {
    return ((e == null ? void 0 : e.client) ?? p).patch({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/umbcheckout/v2.0/configuration-api/update-configuration",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e == null ? void 0 : e.headers
      }
    });
  }
}
var M = Object.defineProperty, W = Object.getOwnPropertyDescriptor, w = (t) => {
  throw TypeError(t);
}, m = (t, e, c, i) => {
  for (var a = i > 1 ? void 0 : i ? W(e, c) : e, n = t.length - 1, o; n >= 0; n--)
    (o = t[n]) && (a = (i ? o(e, c, a) : o(a)) || a);
  return i && a && M(e, c, a), a;
}, z = (t, e, c) => e.has(t) || w("Cannot " + c), N = (t, e, c) => (z(t, e, "read from private field"), c ? c.call(t) : e.get(t)), x = (t, e, c) => e.has(t) ? w("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, c), _ = (t, e, c) => (z(t, e, "access private method"), c), g, h, b, L;
let l = class extends D(U) {
  constructor() {
    super(...arguments), x(this, h), x(this, g, new P(this)), this.hasUmbracoApplicationUrlSet = !1, this.checkingLicence = !0;
  }
  connectedCallback() {
    super.connectedCallback(), this.consumeContext(B, (t) => {
      this._notificationContext = t;
    }), _(this, h, b).call(this), _(this, h, L).call(this);
  }
  async recheckLicence() {
    this.recheckLicenceButtonState = "waiting", await $.checkLicence().then((t) => {
      var e, c;
      if (this.checkingLicence = !0, t.data.accepted === "Accepted") {
        const i = { headline: this.localize.term("umbcheckout_license_check_requested_title"), message: this.localize.term("umbcheckout_license_check_requested_message") };
        (e = this._notificationContext) == null || e.peek("positive", { data: i }), _(this, h, b).call(this), setTimeout(() => {
          this.checkingLicence = !1, this.recheckLicenceButtonState = "success";
        }, 5e3);
      }
      if (t.data.accepted === "Wait") {
        const i = { headline: this.localize.term("umbcheckout_license_check_request_wait_title"), message: N(this, g).string("#umbcheckout_license_check_request_wait_message", t.data.timeLeft) };
        (c = this._notificationContext) == null || c.peek("warning", { data: i }), this.recheckLicenceButtonState = "failed", this.checkingLicence = !1;
      }
    }).catch((t) => {
      var c;
      const e = { headline: this.localize.term("umbcheckout_license_check_error_title"), message: this.localize.term("umbcheckout_license_check_error_message") };
      (c = this._notificationContext) == null || c.peek("danger", { data: e }), this.recheckLicenceButtonState = "failed", this.checkingLicence = !1;
    });
  }
  render() {
    var t, e, c, i, a, n, o, k, v, f;
    return s`   
        <section id="umbcheckout-workspace">
            <div class="dashboard-overview">
                <div class="left-col">
                    <uui-box class="introduction" headline=${this.localize.term("umbcheckout_workspace_overview_title")}>
                        <umb-localize key="umbcheckout_workspace_overview_introduction"></umb-localize>

                        ${u(((t = this.licenceStatus) == null ? void 0 : t.status) === "Active" || ((e = this.licenceStatus) == null ? void 0 : e.status) === "Expired" || ((c = this.licenceStatus) == null ? void 0 : c.status) === "Invalid", () => s`
                            <p class="red bold"><strong><umb-localize class="red bold underline" key="umbcheckout_unlicenced_warning"></umb-localize></strong></p>
                        `)}
                    </uui-box>
                </div>
                <div class="right-col">
                    <uui-box id="licence-details" headline=${this.localize.term("umbcheckout_licence_details")}>
                    ${this.checkingLicence ? s`<uui-loader></uui-loader>` : s`    
                        <div class="licence-detail-container">
                            ${u(!this.hasUmbracoApplicationUrlSet, () => s`<p class="red bold"><strong><umb-localize key="umbcheckout_umbraco_application_url_unset"></umb-localize></strong></p>`)}
                            ${u((i = this.licenceStatus) == null ? void 0 : i.isDevelopmentLicense, () => s`<p><strong>${this.localize.term("umbcheckout_license_development")}</strong>: ${this.localize.term("umbcheckout_license_development_true")}</p>`)}
                            <p><strong>${this.localize.term("umbcheckout_licence_status")}</strong>: ${(a = this.licenceStatus) == null ? void 0 : a.status}</p>
                            <p><strong>${this.localize.term("umbcheckout_licence_registration_date")}</strong>: ${(n = this.licenceStatus) == null ? void 0 : n.regDate}</p>
                            <p><strong>${this.localize.term("umbcheckout_licence_expiry_date")}</strong>: ${u(((o = this.licenceStatus) == null ? void 0 : o.expiryDateTime) !== "0000-00-00", () => {
      var r;
      return s`${(r = this.licenceStatus) == null ? void 0 : r.expiryDateTime}`;
    })}</p>
                            <p><strong>${this.localize.term("umbcheckout_licence_valid_domains")}</strong>: ${(k = this.licenceStatus) == null ? void 0 : k.validDomains}</p>
                            <p><strong>${this.localize.term("umbcheckout_licence_valid_paths")}</strong>: ${(v = this.licenceStatus) == null ? void 0 : v.validPaths}</p>
                        </div>
                        <uui-button look="primary" ?disabled=${!this.hasUmbracoApplicationUrlSet} .state=${this.recheckLicenceButtonState} @click="${this.recheckLicence}">
                            ${this.localize.term("umbcheckout_licence_recheck")}
                        </uui-button>
                        `}
                    </uui-box>

                    ${this.checkingLicence ? C : s` 
                        ${A(((f = this.licenceStatus) == null ? void 0 : f.licenceAddons) ?? [], (r) => s`
                        <uui-box id="licence-details-addons" headline=${this.localize.term("umbcheckout_license_addon")}>
                            <div class="licence-detail-container">
                                <p><strong>${this.localize.term("umbcheckout_license_addon_name")}</strong>: ${r.name}</p>
                                <p><strong>${this.localize.term("umbcheckout_licence_status")}</strong>: ${r.status}</p>
                                <p><strong>${this.localize.term("umbcheckout_licence_expiry_date")}</strong>: ${u(r.nextDueDate !== "0000-00-00", () => s`${r.nextDueDate}`)}</p>
                            </div>
                        </uui-box>
                        `)}
                    `}
                </div>
            </div>
        </section>
    `;
  }
};
g = /* @__PURE__ */ new WeakMap();
h = /* @__PURE__ */ new WeakSet();
b = async function() {
  await $.getLicenceStatus().then((t) => {
    this.licenceStatus = t.data, this.checkingLicence = !1;
  });
};
L = async function() {
  const t = await q.hasUmbracoApplicationUrlSet();
  this.hasUmbracoApplicationUrlSet = t.data;
};
l.styles = [
  E,
  I,
  S`
            #umbcheckout-workspace {
				padding: var(--uui-size-layout-1);
			}
            #licence-warning {
                margin-top: var(--uui-size-layout-1);
            }
            uui-box {
				p:first-child {
					margin-top: 0;
				}
			}
            uui-box:not(:last-of-type) {
				margin-bottom: var(--uui-size-layout-1);
			}

            .dashboard-overview {
                /*top: 0px;*/
                flex-direction: row;
                display: flex;
            }

            #licence-details, #licence-details-addons {
                display: flex;
                flex-direction: column;
                gap: var(--uui-size-space-3);
            }

            .licence-detail-container {
                display: block;
            }

            .left-col {
                flex: 1 1 auto;
                margin-right: 20px;
                width: calc(100% - 370px)
            }

            .right-col {
                flex: 0 0 350px;
                margin-top: 0;
                align-self: start
            }
  `
];
m([
  d()
], l.prototype, "licenceStatus", 2);
m([
  d()
], l.prototype, "hasUmbracoApplicationUrlSet", 2);
m([
  d()
], l.prototype, "recheckLicenceButtonState", 2);
m([
  d()
], l.prototype, "checkingLicence", 2);
l = m([
  T("umbcheckout-overview")
], l);
export {
  l as default
};
//# sourceMappingURL=overview-B_Jshwsz.js.map

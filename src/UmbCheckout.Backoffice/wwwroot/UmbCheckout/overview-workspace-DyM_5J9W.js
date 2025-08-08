import { css as U, LitElement as B, when as p, repeat as x, nothing as W, html as s, state as m, customElement as q } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as I } from "@umbraco-cms/backoffice/element-api";
import { UmbTextStyles as M } from "@umbraco-cms/backoffice/style";
import { UUITextStyles as V } from "@umbraco-cms/backoffice/external/uui";
import { c as S } from "./entrypoint-C4Z0gcPx.js";
import { U as N, C } from "./overview-workspace-context-C797DZyv.js";
import { UMB_NOTIFICATION_CONTEXT as H } from "@umbraco-cms/backoffice/notification";
import { UmbLocalizationController as K } from "@umbraco-cms/backoffice/localization-api";
const R = U`
	${V}

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
class L {
  static checkLicence(e) {
    return ((e == null ? void 0 : e.client) ?? S).get({
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
    return ((e == null ? void 0 : e.client) ?? S).get({
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
var X = Object.defineProperty, F = Object.getOwnPropertyDescriptor, E = (t) => {
  throw TypeError(t);
}, n = (t, e, i, r) => {
  for (var a = r > 1 ? void 0 : r ? F(e, i) : e, u = t.length - 1, h; u >= 0; u--)
    (h = t[u]) && (a = (r ? h(e, i, a) : h(a)) || a);
  return r && a && X(e, i, a), a;
}, v = (t, e, i) => e.has(t) || E("Cannot " + i), f = (t, e, i) => (v(t, e, "read from private field"), i ? i.call(t) : e.get(t)), b = (t, e, i) => e.has(t) ? E("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, i), G = (t, e, i, r) => (v(t, e, "write to private field"), e.set(t, i), i), _ = (t, e, i) => (v(t, e, "access private method"), i), k, d, l, g, D, A, O;
let o = class extends I(B) {
  constructor() {
    super(...arguments), b(this, l), b(this, k, new K(this)), b(this, d), this.hasUmbracoApplicationUrlSet = !1, this.checkingLicence = !0, this.propertyEditors = [];
  }
  connectedCallback() {
    super.connectedCallback(), this.consumeContext(H, (t) => {
      this._notificationContext = t;
    }), this.consumeContext(N, (t) => {
      G(this, d, t);
    }), _(this, l, g).call(this), _(this, l, A).call(this), _(this, l, D).call(this);
  }
  async recheckLicence() {
    this.recheckLicenceButtonState = "waiting", await L.checkLicence().then((t) => {
      var i, r;
      this.checkingLicence = !0;
      var e = t.data;
      if (e.accepted === "Accepted") {
        const a = { headline: this.localize.term("umbcheckout_license_check_requested_title"), message: this.localize.term("umbcheckout_license_check_requested_message") };
        (i = this._notificationContext) == null || i.peek("positive", { data: a }), _(this, l, g).call(this), setTimeout(() => {
          this.checkingLicence = !1, this.recheckLicenceButtonState = "success";
        }, 5e3);
      }
      if (e.accepted === "Wait") {
        const a = { headline: this.localize.term("umbcheckout_license_check_request_wait_title"), message: f(this, k).string("#umbcheckout_license_check_request_wait_message", e.timeLeft) };
        (r = this._notificationContext) == null || r.peek("warning", { data: a }), this.recheckLicenceButtonState = "failed", this.checkingLicence = !1;
      }
    }).catch(() => {
      var e;
      const t = { headline: this.localize.term("umbcheckout_license_check_error_title"), message: this.localize.term("umbcheckout_license_check_error_message") };
      (e = this._notificationContext) == null || e.peek("danger", { data: t }), this.recheckLicenceButtonState = "failed", this.checkingLicence = !1;
    });
  }
  render() {
    var t, e, i, r, a, u, h, $, y, w, z;
    return s`
        <section id="umbcheckout-workspace">
            <div class="dashboard-overview">
            <div class="left-col">
                <uui-box class="introduction" headline=${this.localize.term("umbcheckout_workspace_overview_title")}>
                <p>${this.localize.term("umbcheckout_workspace_overview_introduction_documentation_available_at")} <a href="${this.localize.term("umbcheckout_documentation_url")}" target="_blank">${this.localize.term("umbcheckout_documentation_url")}</a></p>
                <p>${this.localize.term("umbcheckout_github_report_issues")} <a href="${this.localize.term("umbcheckout_github_report_issues_url")}" target="_blank">${this.localize.term("umbcheckout_github_report_repo")}</a> 
                ${p(((t = this.licenceStatus) == null ? void 0 : t.status) === "Active", () => s`
                    ${this.localize.term("umbcheckout_support_ticket_or")} <a href="${this.localize.term("umbcheckout_support_ticket_url")}" target="_blank">${this.localize.term("umbcheckout_support_ticket")}</a>
                `)}</p>

                ${p(((e = this.licenceStatus) == null ? void 0 : e.status) === "Unlicensed" || ((i = this.licenceStatus) == null ? void 0 : i.status) === "Expired" || ((r = this.licenceStatus) == null ? void 0 : r.status) === "Invalid", () => s`
                    <p class="red">
                    <strong>
                        ${this.localize.term("umbcheckout_unlicenced_warning_mode")} <a href="${this.localize.term("umbcheckout_product_url")}" target="_blank">${this.localize.term("umbcheckout_unlicenced_warning_purchase")}</a> ${this.localize.term("umbcheckout_unlicenced_warning_support")}
                    </strong>
                    </p>
                `)}
                </uui-box>
                <uui-box headline=${this.localize.term("umbcheckout_workspace_overview_configuration")}>
                <umb-property-dataset .value=${this.propertyEditorsData} @change=${_(this, l, O)}>
                    ${x(this.propertyEditors ?? [], (c) => s`
                    <umb-property data-path="$.values[?(@.alias == '${c.alias}')].value"
                    .label=${this.localize.term(c.label)}
                    .description=${this.localize.term(c.description)}
                    .alias=${c.alias}
                    .config=${Object.entries(c.config ?? {}).map(([T, P]) => ({ alias: T, value: P }))}
                    .validation=${c.validation}
                    property-editor-ui-alias=${c.editorUiAlias}></umb-property>
                    `)}
                </umb-property-dataset>
                </uui-box>
            </div>
            <div class="right-col">
                <uui-box id="licence-details" headline=${this.localize.term("umbcheckout_licence_details")}>
                ${this.checkingLicence ? s`<uui-loader></uui-loader>` : s`    
                <div class="licence-detail-container">
                    ${p(!this.hasUmbracoApplicationUrlSet, () => s`<p class="red bold"><strong><umb-localize key="umbcheckout_umbraco_application_url_unset"></umb-localize></strong></p>`)}
                    ${p((a = this.licenceStatus) == null ? void 0 : a.isDevelopmentLicense, () => s`<p><strong>${this.localize.term("umbcheckout_license_development")}</strong>: ${this.localize.term("umbcheckout_license_development_true")}</p>`)}
                    <p><strong>${this.localize.term("umbcheckout_licence_status")}</strong>: ${(u = this.licenceStatus) == null ? void 0 : u.status}</p>
                    <p><strong>${this.localize.term("umbcheckout_licence_registration_date")}</strong>: ${(h = this.licenceStatus) == null ? void 0 : h.regDate}</p>
                    <p><strong>${this.localize.term("umbcheckout_licence_expiry_date")}</strong>: ${p((($ = this.licenceStatus) == null ? void 0 : $.expiryDateTime) !== "0000-00-00", () => {
      var c;
      return s`${(c = this.licenceStatus) == null ? void 0 : c.expiryDateTime}`;
    })}</p>
                    <p class="valid-domains"><strong>${this.localize.term("umbcheckout_licence_valid_domains")}</strong>: ${(y = this.licenceStatus) == null ? void 0 : y.validDomains}</p>
                    <p class="valid-paths"><strong>${this.localize.term("umbcheckout_licence_valid_paths")}</strong>: ${(w = this.licenceStatus) == null ? void 0 : w.validPaths}</p>
                </div>
                <uui-button look="primary" ?disabled=${!this.hasUmbracoApplicationUrlSet} .state=${this.recheckLicenceButtonState} @click="${this.recheckLicence}">
                    ${this.localize.term("umbcheckout_licence_recheck")}
                </uui-button>
                `}
                </uui-box>

                ${this.checkingLicence ? W : s` 
                ${x(((z = this.licenceStatus) == null ? void 0 : z.licenceAddons) ?? [], (c) => s`
                <uui-box id="licence-details-addons" headline=${this.localize.term("umbcheckout_license_addon")}>
                    <div class="licence-detail-container">
                    <p><strong>${this.localize.term("umbcheckout_license_addon_name")}</strong>: ${c.name}</p>
                    <p><strong>${this.localize.term("umbcheckout_licence_status")}</strong>: ${c.status}</p>
                    <p><strong>${this.localize.term("umbcheckout_licence_expiry_date")}</strong>: ${p(c.nextDueDate !== "0000-00-00", () => s`${c.nextDueDate}`)}</p>
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
k = /* @__PURE__ */ new WeakMap();
d = /* @__PURE__ */ new WeakMap();
l = /* @__PURE__ */ new WeakSet();
g = async function() {
  await L.getLicenceStatus().then((t) => {
    this.licenceStatus = t.data, this.checkingLicence = !1;
  });
};
D = async function() {
  await C.getConfiguration().then((t) => {
    this.propertyEditors = t.data, this.propertyEditorsData = t.data.map((e) => ({
      alias: e.alias,
      value: e.value
    })) ?? [], f(this, d).propertyEditorsData = this.propertyEditorsData, this.requestUpdate("propertyEditorsData");
  }).catch((t) => {
    console.error("Error fetching property editors:", t);
  });
};
A = async function() {
  const t = await C.hasUmbracoApplicationUrlSet();
  this.hasUmbracoApplicationUrlSet = t.data;
};
O = function(t) {
  const e = this.propertyEditorsData;
  this.propertyEditorsData = t.target.value, f(this, d).propertyEditorsData = this.propertyEditorsData, this.requestUpdate("propertyEditorsData", e);
};
o.styles = [
  M,
  R,
  U`
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

            .valid-domains, .valid-paths {
                word-break: break-word
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
n([
  m()
], o.prototype, "licenceStatus", 2);
n([
  m()
], o.prototype, "hasUmbracoApplicationUrlSet", 2);
n([
  m()
], o.prototype, "recheckLicenceButtonState", 2);
n([
  m()
], o.prototype, "checkingLicence", 2);
n([
  m()
], o.prototype, "propertyEditors", 2);
n([
  m()
], o.prototype, "propertyEditorsData", 2);
o = n([
  q("umbcheckout-overview-workspace")
], o);
export {
  o as default
};
//# sourceMappingURL=overview-workspace-DyM_5J9W.js.map

import { UmbEntityDetailWorkspaceContextBase as m } from "@umbraco-cms/backoffice/workspace";
import { b as n, a as u, c as p, U as l } from "./bundle.manifests-B1q7guFd.js";
import { html as C, customElement as O } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as U } from "@umbraco-cms/backoffice/lit-element";
import "./made-by.element-BHDU3_qS.js";
var _ = Object.getOwnPropertyDescriptor, b = (r, e, t, i) => {
  for (var o = i > 1 ? void 0 : i ? _(e, t) : e, s = r.length - 1, c; s >= 0; s--)
    (c = r[s]) && (o = c(o) || o);
  return o;
};
let a = class extends U {
  render() {
    return C`<umb-workspace-editor headline=${this.localize.term("umbcheckout_section")}>
      <umbcheckout-made-by slot="footer-info"></umbcheckout-made-by>
    </umb-workspace-editor>`;
  }
};
a = b([
  O("umbcheckout-configuration-workspace-editor")
], a);
class f extends m {
  constructor(e) {
    super(e, {
      workspaceAlias: p,
      entityType: u,
      detailRepositoryAlias: n
    }), this.model = this._data.createObservablePartOfCurrent((t) => t), this.routes.setRoutes([
      {
        path: "",
        component: a,
        setup: () => {
          this.load(l);
        }
      }
    ]);
  }
  setValue(e, t) {
    this._data.updateCurrent({ [e]: t });
  }
}
export {
  f as UmbCheckoutConfigurationWorkspaceContext,
  f as api
};
//# sourceMappingURL=configuration-workspace.context-CkYWlFUU.js.map

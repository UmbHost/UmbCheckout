import { UmbEntityDetailWorkspaceContextBase as c } from "@umbraco-cms/backoffice/workspace";
import { b as u, a as m, c as p, U as C } from "./bundle.manifests-yQeLHgja.js";
import { html as l, customElement as O } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as U } from "@umbraco-cms/backoffice/lit-element";
var _ = Object.getOwnPropertyDescriptor, h = (o, t, e, i) => {
  for (var r = i > 1 ? void 0 : i ? _(t, e) : t, a = o.length - 1, n; a >= 0; a--)
    (n = o[a]) && (r = n(r) || r);
  return r;
};
let s = class extends U {
  render() {
    return l`<umb-workspace-editor headline=${this.localize.term("umbcheckout_configuration")}></umb-workspace-editor>`;
  }
};
s = h([
  O("umbcheckout-configuration-workspace-editor")
], s);
class b extends c {
  constructor(t) {
    super(t, {
      workspaceAlias: p,
      entityType: m,
      detailRepositoryAlias: u
    }), this.model = this._data.createObservablePartOfCurrent((e) => e), this.routes.setRoutes([
      {
        path: "",
        component: s,
        setup: () => {
          this.load(C);
        }
      }
    ]);
  }
  setValue(t, e) {
    this._data.updateCurrent({ [t]: e });
  }
}
export {
  b as UmbCheckoutConfigurationWorkspaceContext,
  b as api
};
//# sourceMappingURL=configuration-workspace.context-D_vcFVF9.js.map

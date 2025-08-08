import { UmbWorkspaceActionBase as e } from "@umbraco-cms/backoffice/workspace";
import { U as o } from "./overview-workspace-context-C797DZyv.js";
class a extends e {
  // This method is executed
  async execute() {
    const t = await this.getContext(o);
    if (!t)
      throw new Error("Could not get the counter context");
    await t.save();
  }
}
const s = a;
export {
  a as UmbCheckoutSaveWorkspaceAction,
  s as api
};
//# sourceMappingURL=workspace-save-action-Bv43UAjJ.js.map

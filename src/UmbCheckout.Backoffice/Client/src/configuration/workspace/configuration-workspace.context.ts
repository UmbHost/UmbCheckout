import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbEntityDetailWorkspaceContextBase } from "@umbraco-cms/backoffice/workspace";
import type { UmbCheckoutConfigurationRepository } from "../repository/configuration.repository.js";
import type { UmbCheckoutConfigurationModel } from "../types.js";
import {
  UMBCHECKOUT_CONFIGURATION_ENTITY_TYPE,
  UMBCHECKOUT_CONFIGURATION_REPOSITORY_ALIAS,
  UMBCHECKOUT_CONFIGURATION_UNIQUE,
  UMBCHECKOUT_CONFIGURATION_WORKSPACE_ALIAS,
} from "../entity.js";
import { UmbCheckoutConfigurationWorkspaceEditorElement } from "./configuration-workspace-editor.element.js";

export class UmbCheckoutConfigurationWorkspaceContext extends UmbEntityDetailWorkspaceContextBase<
  UmbCheckoutConfigurationModel,
  UmbCheckoutConfigurationRepository
> {
  readonly model = this._data.createObservablePartOfCurrent((data) => data);

  constructor(host: UmbControllerHost) {
    super(host, {
      workspaceAlias: UMBCHECKOUT_CONFIGURATION_WORKSPACE_ALIAS,
      entityType: UMBCHECKOUT_CONFIGURATION_ENTITY_TYPE,
      detailRepositoryAlias: UMBCHECKOUT_CONFIGURATION_REPOSITORY_ALIAS,
    });

    this.routes.setRoutes([
      {
        path: "",
        component: UmbCheckoutConfigurationWorkspaceEditorElement,
        setup: () => {
          this.load(UMBCHECKOUT_CONFIGURATION_UNIQUE);
        },
      },
    ]);
  }

  setValue<K extends keyof UmbCheckoutConfigurationModel>(alias: K, value: UmbCheckoutConfigurationModel[K]) {
    this._data.updateCurrent({ [alias]: value } as Partial<UmbCheckoutConfigurationModel>);
  }
}

export { UmbCheckoutConfigurationWorkspaceContext as api };

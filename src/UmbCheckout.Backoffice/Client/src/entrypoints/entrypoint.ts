import type {
  UmbEntryPointOnInit,
  UmbEntryPointOnUnload,
} from "@umbraco-cms/backoffice/extension-api";
import { manifests as workspaces } from "../workspaces/manifests.ts";
import { manifests as localizations } from "../localization/manifest.ts";
import { manifests as menus } from "../trees/manifest.ts";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { client as backofficeClient } from "../api/backoffice/client.gen.ts";
import { client as licencingClient } from "../api/licencing/client.gen.ts";

// load up the manifests here
export const onInit: UmbEntryPointOnInit = (_host, _extensionRegistry) => {
    _host.consumeContext(UMB_AUTH_CONTEXT,(auth)=> {

    const config = auth?.getOpenApiConfiguration();

    backofficeClient.setConfig({
        throwOnError: true,
        auth: config?.token ?? undefined,
        baseUrl: config?.base ?? "",
        credentials: config?.credentials ?? "same-origin",
     });

    licencingClient.setConfig({
        throwOnError: true,
        auth: config?.token ?? undefined,
        baseUrl: config?.base ?? "",
        credentials: config?.credentials ?? "same-origin",
     });

    _extensionRegistry.registerMany([
        ...menus,
        ...localizations,
        ...workspaces
    ]);
  });
};

export const onUnload: UmbEntryPointOnUnload = (_host, _extensionRegistry) => {
    _extensionRegistry.unregisterMany([
        ...menus.map((manifest) => manifest.alias),
        ...localizations.map((manifest) => manifest.alias),
        ...workspaces.map((manifest) => manifest.alias),
    ]);
};

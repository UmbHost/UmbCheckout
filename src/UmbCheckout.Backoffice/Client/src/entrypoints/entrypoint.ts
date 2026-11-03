import type { UmbEntryPointOnInit, UmbEntryPointOnUnload } from "@umbraco-cms/backoffice/extension-api";
import { client } from "../api/client.gen.js";
import { client as licensingClient } from "../api/licensing/client.gen.js";
import { configureUmbCheckoutClient } from "../core/api-client.js";

export const onInit: UmbEntryPointOnInit = (host) => {
  configureUmbCheckoutClient(host, client);
  configureUmbCheckoutClient(host, licensingClient);
};

// Required by the 17.0 UmbEntryPointModule type; nothing to tear down.
export const onUnload: UmbEntryPointOnUnload = () => {};

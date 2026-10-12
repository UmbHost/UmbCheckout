import type { UmbElement } from "@umbraco-cms/backoffice/element-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";

/** The subset of a hey-api fetch client that UmbCheckout configures. */
export interface UmbCheckoutApiClient {
  setConfig(config: { auth?: unknown; baseUrl?: string; credentials?: RequestCredentials }): unknown;
  interceptors: { response: { use(fn: (response: Response) => Response | Promise<Response>): unknown } };
}

/**
 * Configures a generated client with the back-office token (Umbraco 17.0 template pattern)
 * and warns once when the session has expired, because the 17.0 pattern does not trigger Umbraco's re-login prompt.
 */
export function configureUmbCheckoutClient(host: UmbElement, client: UmbCheckoutApiClient): void {
  let warned = false;

  host.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
    const config = authContext?.getOpenApiConfiguration();
    client.setConfig({
      auth: config?.token ?? undefined,
      baseUrl: config?.base ?? "",
      credentials: config?.credentials ?? "same-origin",
    });
  });

  host.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
    client.interceptors.response.use((response) => {
      if (response.status === 401 && !warned) {
        warned = true;
        notificationContext?.peek("warning", {
          data: { headline: "UmbCheckout", message: "Your session has expired, please reload the page." },
        });
      }
      return response;
    });
  });
}

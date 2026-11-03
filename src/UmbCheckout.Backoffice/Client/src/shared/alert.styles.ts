import { css } from "@umbraco-cms/backoffice/external/lit";

/** The v13 "alert alert-danger" box, built from UUI theme tokens. */
export const UmbCheckoutAlertStyles = css`
  .umbcheckout-alert {
    display: block;
    padding: var(--uui-size-space-4) var(--uui-size-space-5);
    margin-bottom: var(--uui-size-space-5);
    border: 1px solid var(--uui-color-danger-standalone);
    border-left-width: 4px;
    border-radius: var(--uui-border-radius);
    background-color: var(--uui-color-danger);
    color: var(--uui-color-danger-contrast);
    font-weight: 700;
  }
  .umbcheckout-alert--info {
    border-color: var(--uui-color-default);
    background-color: var(--uui-color-default);
    color: var(--uui-color-default-contrast);
    font-weight: 400;
  }
  .umbcheckout-alert a {
    color: inherit;
    text-decoration: underline;
  }
`;

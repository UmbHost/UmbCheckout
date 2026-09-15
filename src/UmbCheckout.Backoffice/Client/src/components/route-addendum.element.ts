import { css, customElement, html, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbRoutePathAddendumContext } from "@umbraco-cms/backoffice/router";

/**
 * Gives the slotted content its own route path addendum, like umb-property does with the property alias.
 * Needed when multiple inputs that register modal routes (e.g. umb-input-multi-url) are used outside of umb-property,
 * otherwise their modal routes clash and all open for the first input.
 */
@customElement("umbcheckout-route-addendum")
export class UmbCheckoutRouteAddendumElement extends UmbLitElement {
  #pathAddendum = new UmbRoutePathAddendumContext(this);

  @property()
  set addendum(value: string) {
    this.#pathAddendum.setAddendum(value);
  }

  override render() {
    return html`<slot></slot>`;
  }

  static override styles = [
    css`
      :host {
        display: block;
      }
    `,
  ];
}

declare global {
  interface HTMLElementTagNameMap {
    "umbcheckout-route-addendum": UmbCheckoutRouteAddendumElement;
  }
}

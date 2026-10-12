import { UMB_ACTION_EVENT_CONTEXT } from "@umbraco-cms/backoffice/action";
import { UmbEntityActionBase, UmbEntityDeletedEvent, UmbRequestReloadStructureForEntityEvent } from "@umbraco-cms/backoffice/entity-action";
import type { MetaEntityActionDeleteKind } from "@umbraco-cms/backoffice/entity-action";
import { createExtensionApiByAlias } from "@umbraco-cms/backoffice/extension-registry";
import { html } from "@umbraco-cms/backoffice/external/lit";
import { UmbLocalizationController } from "@umbraco-cms/backoffice/localization-api";
import { umbConfirmModal } from "@umbraco-cms/backoffice/modal";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import type { UmbDetailRepository, UmbItemRepository } from "@umbraco-cms/backoffice/repository";

/**
 * Umbraco's delete entity action (confirm, delete, refresh) with v13's "… deleted" toast in place of the generic "Deleted".
 * Use as the `api` of a `kind: "delete"` entity action: `api: () => import(...)` exporting `createUmbCheckoutDeleteAction(...)`'s class.
 */
export function createUmbCheckoutDeleteAction(deletedTitleKey: string, deletedMessageKey: string) {
  return class UmbCheckoutDeleteEntityAction extends UmbEntityActionBase<MetaEntityActionDeleteKind> {
    #localize = new UmbLocalizationController(this);

    override async execute() {
      const unique = this.args.unique;
      if (!unique) throw new Error("Cannot delete an item without a unique identifier.");

      const itemRepository = await createExtensionApiByAlias<UmbItemRepository<{ name: string }>>(this, this.args.meta.itemRepositoryAlias);
      const { data } = await itemRepository.requestItems([unique]);
      const item = data?.[0];
      if (!item) throw new Error("Item not found.");

      await umbConfirmModal(this, {
        headline: this.args.meta.confirm?.headline ?? "#actions_delete",
        content: html`${this.#localize.htmlString(this.args.meta.confirm?.message ?? "#defaultdialogs_confirmdelete", item.name)}`,
        color: "danger",
        confirmLabel: "#general_delete",
      });

      const detailRepository = await createExtensionApiByAlias<UmbDetailRepository<never>>(this, this.args.meta.detailRepositoryAlias);
      const { error } = await detailRepository.delete(unique);
      if (error) throw error;

      const actionEvents = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
      actionEvents?.dispatchEvent(new UmbRequestReloadStructureForEntityEvent({ unique, entityType: this.args.entityType }));
      actionEvents?.dispatchEvent(new UmbEntityDeletedEvent({ unique, entityType: this.args.entityType }));

      const notifications = await this.getContext(UMB_NOTIFICATION_CONTEXT);
      notifications?.peek("positive", {
        data: { headline: this.#localize.term(deletedTitleKey), message: this.#localize.term(deletedMessageKey) },
      });
    }
  };
}

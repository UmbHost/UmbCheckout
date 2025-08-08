import { UmbWorkspaceActionBase, type UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import { UMBCHECKOUT_OVERVIEW_WORKSPACE_CONTEXT } from './overview-workspace-context';

export class UmbCheckoutSaveWorkspaceAction extends UmbWorkspaceActionBase implements UmbWorkspaceAction {
	// This method is executed
	override async execute() {
		const context = await this.getContext(UMBCHECKOUT_OVERVIEW_WORKSPACE_CONTEXT);
		if (!context) {
			throw new Error('Could not get the counter context');
		}
		await context.save();
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export const api = UmbCheckoutSaveWorkspaceAction;
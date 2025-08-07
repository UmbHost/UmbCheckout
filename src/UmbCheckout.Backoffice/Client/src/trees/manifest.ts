import { UMB_SETTINGS_SECTION_ALIAS } from "@umbraco-cms/backoffice/settings";
import { UmbCheckoutMenuElement } from './umbcheckout.menu-element'

const menuConstants = {
    type: 'menu',
    alias: 'umbcheckout.menu',
    name: 'UmbCheckout Menu',
    icon: 'icon-shopping-basket',
    rootElement: 'umbcheckout',
}

const menu: UmbExtensionManifest = {
	type: 'menu',
	alias: menuConstants.alias,
	name: menuConstants.name,
	meta: {
		label: menuConstants.name,
		icon: menuConstants.icon,
		entityType: menuConstants.rootElement,
	},
};

const menuItem: UmbExtensionManifest = {
	type: 'menuItem',
	alias: 'umbcheckout.menu.item',
	name: 'UmbCheckout menu item',
	element: UmbCheckoutMenuElement,
	meta: {
		label: 'umbcheckout_sidebar_overview',
		icon: 'icon-shopping-basket',
		entityType: 'umbcheckout',
		menus: [menuConstants.alias],
	},
};

const menuSidebarApp: UmbExtensionManifest = {
	type: 'sectionSidebarApp',
	kind: 'menu',
	alias: 'umbcheckout-sidebar-menu',
	name: 'UmbCheckout section sidebar menu',
	weight: 160,
	meta: {
		label: '#umbcheckout_sidebar_menu',
		menu: menu.alias,
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: UMB_SETTINGS_SECTION_ALIAS,
		},
	],
};

export const manifests = [
    menu,
    menuSidebarApp,
    menuItem
];
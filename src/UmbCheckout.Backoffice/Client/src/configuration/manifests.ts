import { manifests as repository } from "./repository/manifests.js";
import { manifests as workspace } from "./workspace/manifests.js";
import { manifests as menuItem } from "./menu-item/manifests.js";

export const manifests: Array<UmbExtensionManifest> = [...repository, ...workspace, ...menuItem];

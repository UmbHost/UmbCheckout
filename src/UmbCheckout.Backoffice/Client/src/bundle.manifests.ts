import { manifests as entrypoints } from "./entrypoints/manifest.js";
import { manifests as localization } from "./localization/manifest.js";
import { manifests as settingsMenu } from "./settings-menu/manifests.js";
import { manifests as license } from "./license/manifests.js";
import { manifests as configuration } from "./configuration/manifests.js";
import { manifests as metadataEditor } from "./property-editors/metadata/manifests.js";

export const manifests: Array<UmbExtensionManifest> = [...entrypoints, ...localization, ...settingsMenu, ...license, ...configuration, ...metadataEditor];

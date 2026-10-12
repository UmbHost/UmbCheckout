import { manifests as entrypoints } from "./entrypoints/manifest.js";
import { manifests as localization } from "./localization/manifest.js";
import { manifests as section } from "./section/manifests.js";
import { manifests as license } from "./license/manifests.js";
import { manifests as status } from "./status/manifests.js";
import { manifests as configuration } from "./configuration/manifests.js";
import { manifests as metadataEditor } from "./property-editors/metadata/manifests.js";

export const manifests: Array<UmbExtensionManifest> = [...entrypoints, ...localization, ...section, ...license, ...status, ...configuration, ...metadataEditor];

import { manifests as entrypoints } from "./entrypoints/manifest.js";
import { manifests as localization } from "./localization/manifest.js";

export const manifests: Array<UmbExtensionManifest> = [...entrypoints, ...localization];

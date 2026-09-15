import { manifests as menuManifests } from "./menu/manifests.js";
import { manifests as configurationManifests } from "./configuration/manifests.js";
import { manifests as propertyEditorManifests } from "./property-editors/manifests.js";
import { manifests as localizationManifests } from "./localization/manifests.js";

// Collates all the manifests of the UmbCheckout backoffice, this bundle is loaded from the package manifest
export const manifests: Array<UmbExtensionManifest> = [
  ...menuManifests,
  ...configurationManifests,
  ...propertyEditorManifests,
  ...localizationManifests,
];

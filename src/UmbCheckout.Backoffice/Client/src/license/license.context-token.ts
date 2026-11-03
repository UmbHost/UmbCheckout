import { UmbContextToken } from "@umbraco-cms/backoffice/context-api";
import type { UmbCheckoutLicenseContext } from "./license.context.js";

export const UMBCHECKOUT_LICENSE_CONTEXT = new UmbContextToken<UmbCheckoutLicenseContext>("UmbCheckoutLicenseContext");

CREATE TABLE UmbCheckoutConfiguration
(
 "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
, "SuccessPageUrl" TEXT COLLATE NOCASE NOT NULL
, "CancelPageUrl" TEXT COLLATE NOCASE NOT NULL
, "StoreBasketInCookie" INTEGER NOT NULL
, "StoreBasketInDatabase" INTEGER NOT NULL
, "BasketInDatabaseExpiry" INTEGER NOT NULL
, "BasketInCookieExpiry" INTEGER NOT NULL
, "EnableShipping" INTEGER NOT NULL
, "Key" TEXT NULL, "CurrencyCode" TEXT COLLATE NOCASE NULL, CONSTRAINT PK_UmbCheckoutConfiguration UNIQUE ("Id")
);
CREATE TABLE UmbCheckoutStripeSettings
(
 "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
, "Key" TEXT NULL
, "UseLiveApiDetails" INTEGER NOT NULL
, "ShippingAllowedCountries" TEXT COLLATE NOCASE NULL, "CollectPhoneNumber" INTEGER NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_CollectPhoneNumber" DEFAULT ('False'), "CollectPromotionalConsent" INTEGER NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_CollectPromotionalConsent" DEFAULT ('False'), "AllowPromotionalCodes" INTEGER NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_AllowPromotionalCodes" DEFAULT ('False'), "EnableAbandonedCartRecovery" INTEGER NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_EnableAbandonedCartRecovery" DEFAULT ('False'), "AllowPromotionalCodesOnRecoveredCarts" INTEGER NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_AllowPromotionalCodesOnRecoveredCarts" DEFAULT ('False'), CONSTRAINT PK_UmbCheckoutStripeSettings UNIQUE ("Id")
);
CREATE TABLE UmbCheckoutStripeShipping
(
 "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
, "Name" TEXT COLLATE NOCASE NOT NULL
, "Value" TEXT COLLATE NOCASE NOT NULL
, "Key" TEXT NULL, CONSTRAINT PK_UmbCheckoutStripeShipping UNIQUE ("Id")
);
CREATE UNIQUE INDEX "IX_shippingKey" ON "UmbCheckoutStripeShipping" ("Key");
CREATE UNIQUE INDEX "IX_stripeIdShippingRate" ON "UmbCheckoutStripeShipping" ("Value");

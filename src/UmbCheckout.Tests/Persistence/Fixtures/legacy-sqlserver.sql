CREATE TABLE "UmbCheckoutConfiguration" (
  "Id" INT IDENTITY(1,1) NOT NULL,
  "SuccessPageUrl" NVARCHAR(255) NOT NULL,
  "CancelPageUrl" NVARCHAR(255) NOT NULL,
  "StoreBasketInCookie" BIT NOT NULL,
  "StoreBasketInDatabase" BIT NOT NULL,
  "BasketInDatabaseExpiry" INT NOT NULL,
  "BasketInCookieExpiry" INT NOT NULL,
  "EnableShipping" BIT NOT NULL,
  "Key" UNIQUEIDENTIFIER NULL,
  "CurrencyCode" NVARCHAR(255) NULL,
  CONSTRAINT "PK_UmbCheckoutConfiguration" PRIMARY KEY CLUSTERED ("Id")
);
CREATE TABLE "UmbCheckoutStripeSettings" (
  "Id" BIGINT IDENTITY(1,1) NOT NULL,
  "Key" UNIQUEIDENTIFIER NULL,
  "UseLiveApiDetails" BIT NOT NULL,
  "ShippingAllowedCountries" NVARCHAR(255) NULL,
  "CollectPhoneNumber" BIT NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_CollectPhoneNumber" DEFAULT ('False'),
  "CollectPromotionalConsent" BIT NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_CollectPromotionalConsent" DEFAULT ('False'),
  "AllowPromotionalCodes" BIT NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_AllowPromotionalCodes" DEFAULT ('False'),
  "EnableAbandonedCartRecovery" BIT NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_EnableAbandonedCartRecovery" DEFAULT ('False'),
  "AllowPromotionalCodesOnRecoveredCarts" BIT NOT NULL CONSTRAINT "DF_UmbCheckoutStripeSettings_AllowPromotionalCodesOnRecoveredCarts" DEFAULT ('False'),
  CONSTRAINT "PK_UmbCheckoutStripeSettings" PRIMARY KEY CLUSTERED ("Id")
);
CREATE TABLE "UmbCheckoutStripeShipping" (
  "Id" BIGINT IDENTITY(1,1) NOT NULL,
  "Name" NVARCHAR(255) NOT NULL,
  "Value" NVARCHAR(255) NOT NULL,
  "Key" UNIQUEIDENTIFIER NULL,
  CONSTRAINT "PK_UmbCheckoutStripeShipping" PRIMARY KEY CLUSTERED ("Id")
);
CREATE UNIQUE NONCLUSTERED INDEX "IX_shippingKey" ON "UmbCheckoutStripeShipping" ("Key");
CREATE UNIQUE NONCLUSTERED INDEX "IX_stripeIdShippingRate" ON "UmbCheckoutStripeShipping" ("Value");

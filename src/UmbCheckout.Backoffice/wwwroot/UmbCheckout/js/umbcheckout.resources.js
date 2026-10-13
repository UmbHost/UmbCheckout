angular.module("umbraco.resources").factory("umbCheckoutResources", function ($http) {
    return {
        getLicenseStatus: function () {

            return $http.get("backoffice/UmbCheckout/LicenseStatusApi/GetLicenseStatus")
                .then(function (response) {
                    return response;
                }
                );
        },
        getConfiguration: function () {

            return $http.get("backoffice/UmbCheckout/ConfigurationApi/GetConfiguration")
                .then(function (response) {
                    return response;
                }
                );
        },
        getHasUmbracoApplicationUrlSet: function () {

            return $http.get("backoffice/UmbCheckout/ConfigurationApi/HasUmbracoApplicationUrlSet")
                .then(function (response) {
                    return response;
                }
                );
        },
        updateConfiguration: function (configurationValues) {

            return $http.patch("backoffice/UmbCheckout/ConfigurationApi/UpdateConfiguration", configurationValues)
                .then(function (response) {
                    return response;
                }
                );
        },
        checkLicense: function () {

            return $http.get("backoffice/UmbCheckout/LicenceCheck/CheckLicense")
                .then(function (response) {
                    return response;
                }
                );
        }
    }
});

// Shared by the UmbCheckout screens (open and add-on).
angular.module("umbraco.services").factory("umbCheckoutHelpers", function (notificationsService, localizationService) {
    return {
        // Shows a failed API request in a toast. A 403 is "access denied" (accessDeniedKey, or the generic message); a problem
        // response (a missing Stripe key, a Stripe error, Stripe unreachable) shows its own title and detail; anything else a
        // generic failure.
        requestFailed: function (response, accessDeniedKey) {
            var data = response ? response.data : null;

            if (response && response.status === 403) {
                return localizationService.localizeMany(["umbcheckout_access_denied_title", accessDeniedKey || "umbcheckout_access_denied_message"])
                    .then(function (values) {
                        notificationsService.error(values[0], values[1]);
                    });
            }

            if (data && data.detail) {
                return localizationService.localize("umbcheckout_request_failed_title").then(function (title) {
                    notificationsService.error(data.title || title, data.detail);
                });
            }

            return localizationService.localizeMany(["umbcheckout_request_failed_title", "umbcheckout_request_failed_message"])
                .then(function (values) {
                    notificationsService.error(values[0], values[1]);
                });
        },

        // Whether live mode is saved, from the Stripe settings API's property list (useLiveApiDetails is "True" or "False").
        isLiveMode: function (properties) {
            var live = false;
            angular.forEach(properties, function (property) {
                if (property && property.alias === "useLiveApiDetails") {
                    live = String(property.value).toLowerCase() === "true";
                }
            });
            return live;
        },

        // Stripe objects carry created in Unix seconds (Stripe.net's own JSON); a Date needs milliseconds.
        fromUnixSeconds: function (seconds) {
            return new Date(seconds * 1000);
        }
    };
});

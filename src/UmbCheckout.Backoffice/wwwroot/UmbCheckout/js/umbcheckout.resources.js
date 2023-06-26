angular.module("umbraco.resources").factory("umbCheckoutResources", function ($http) {
    return {
        getLicenseStatus: function () {

            return $http.get("backoffice/UmbCheckout/ConfigurationApi/GetLicenseStatus")
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
        updateConfiguration: function (configurationValues) {

            return $http.patch("backoffice/UmbCheckout/ConfigurationApi/UpdateConfiguration", configurationValues)
                .then(function (response) {
                    return response;
                }
                );
        },
        getShippingRates: function () {

            return $http.get("backoffice/UmbCheckout/ShippingRatesApi/GetShippingRates")
                .then(function (response) {
                    return response;
                }
                );
        },
        getStripeShippingRates: function () {

            return $http.get("backoffice/UmbCheckout/ShippingRatesApi/GetStripeShippingRates")
                .then(function (response) {
                    return response;
                }
                );
        },
        getShippingRate: function (id) {

            return $http.get("backoffice/UmbCheckout/ShippingRatesApi/GetShippingRate?id=" + id)
                .then(function (response) {
                    return response;
                }
                );
        },
        updateShippingRate: function (configurationValues, id) {
            configurationValues.id = id
            return $http.patch("backoffice/UmbCheckout/ShippingRatesApi/UpdateShippingRate", configurationValues)
                .then(function (response) {
                    return response;
                }
                );
        },
        deleteShippingRate: function (id) {

            return $http.delete("backoffice/UmbCheckout/ShippingRatesApi/DeleteShippingRate?id=" + id)
                .then(function (response) {
                    return response;
                }
                );
        },
    }
});
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
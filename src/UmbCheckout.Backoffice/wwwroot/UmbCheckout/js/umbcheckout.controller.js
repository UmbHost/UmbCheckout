function UmbCheckout($scope, editorService, umbCheckoutResources, $routeParams, notificationsService, formHelper) {
    var vm = this;
    vm.saveButtonState = "init";
    vm.checkLicenceButtonState = "init";
    vm.createFolderError = "";
    vm.properties = [];
    vm.LicenseState = {}

    umbCheckoutResources.getConfiguration()
        .then(function (response) {
            vm.properties = response.data
        }
    );

    umbCheckoutResources.getLicenseStatus()
        .then(function (response) {
            vm.LicenseState.Status = response.data;

            if (response.data.status == "Invalid") {
                vm.LicenseState.Valid = false;
                vm.LicenseState.Message = "UmbCheckout is running in unlicensed mode, please <a href=\"#\" target=\"_blank\"  class=\"red bold underline\">purchase a license</a> to support development"
            }
            else if (response.data.status == "Valid") {
                vm.LicenseState.Valid = true;
            }
        }
        );

    function saveConfiguration() {
        vm.saveButtonState = "busy";

        if (formHelper.submitForm({ scope: $scope })) {
            var configurationValues = {};
            angular.forEach(vm.properties, function (value, key) {

                var newKey = value.alias;

                configurationValues[newKey] = value.value
            });

            umbCheckoutResources.updateConfiguration(configurationValues)
                .then(function (response) {
                    vm.properties = response.data
                    notificationsService.success("Configuration saved", "The configuration has been saved successfully");
                    vm.saveButtonState = "success";
                    $scope.configurationForm.$dirty = false;
                })
                .catch(
                    function (response) {
                        notificationsService.error("Configuration failed to save", "There was an issue trying to save the configuration");
                        vm.saveButtonState = "error";
                    }
                );
        }
        else {
            vm.saveButtonState = "error";
        }
    }

    function checkLicense() {
        vm.checkLicenceButtonState = "busy";
        umbCheckoutResources.checkLicense()
            .then(function (response) {

                if (response.data.Accepted == "Accepted") {
                    notificationsService.success("License check requested", "A license check has been requested");

                    setTimeout(function () {
                        window.location.reload(1);
                    }, 5000);
                }

                if (response.data.Accepted == "Wait") {
                    notificationsService.warning("Please to check the license", "You can try again in " + response.data.TimeLeft);
                    vm.checkLicenceButtonState = "error";
                }
            })
            .catch(
                function (response) {
                    notificationsService.error("License check failed to save", "A license check has failed");
                    vm.checkLicenceButtonState = "error";
                }
            );
    }
    vm.checkLicence = checkLicense;
    vm.saveConfiguration = saveConfiguration;
}
angular.module("umbraco").controller("UmbCheckout.Settings.Controller", UmbCheckout);
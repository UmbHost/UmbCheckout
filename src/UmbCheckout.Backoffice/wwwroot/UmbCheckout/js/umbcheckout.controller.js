function UmbCheckout($scope, editorService, umbCheckoutResources, $routeParams, notificationsService, formHelper, localizationService) {
    var vm = this;
    vm.saveButtonState = "init";
    vm.checkLicenceButtonState = "init";
    vm.createFolderError = "";
    vm.properties = [];
    vm.LicenseState = {}
    vm.HasUmbracoApplicationUrlSet = false;
    vm.UmbracoApplicationUrlNotSetMessage = "";

    umbCheckoutResources.getConfiguration()
        .then(function (response) {
            vm.properties = response.data
        }
    );

    umbCheckoutResources.getHasUmbracoApplicationUrlSet()
        .then(function (response) {
            vm.HasUmbracoApplicationUrlSet = response.data;
            localizationService.localize("umbcheckout_umbracoapplicationurlunset").then(function (value) {
                vm.UmbracoApplicationUrlNotSetMessage = value;
            });
        }
        );

    umbCheckoutResources.getLicenseStatus()
        .then(function (response) {
            vm.LicenseState.Status = response.data;

            if (response.data.status == "Invalid" || response.data.status == "Unlicensed" || response.data.status == "Expired") {
                vm.LicenseState.Valid = false;
                localizationService.localize("umbcheckout_unlicensed_warning").then(function (value) {
                    vm.LicenseState.Message = value;
                });
            }
            else if (response.data.status == "Active") {
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
                    localizationService.localize("umbcheckout_configuration_saved_title").then(function (title) {
                        localizationService.localize("umbcheckout_configuration_saved_message").then(function (message) {
                            notificationsService.success(title, message);
                        });
                    });
                    vm.saveButtonState = "success";
                    $scope.configurationForm.$dirty = false;
                })
                .catch(
                    function (response) {
                        localizationService.localize("umbcheckout_configuration_failed_save_title").then(function (title) {
                            localizationService.localize("umbcheckout_configuration_failed_save_message").then(function (message) {
                                notificationsService.error(title, message);
                            });
                        });
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

                    localizationService.localize("umbcheckout_license_check_requested_title").then(function (title) {
                        localizationService.localize("umbcheckout_license_check_requested_message").then(function (message) {
                            notificationsService.success(title, message);
                        });
                    });

                    setTimeout(function () {
                        window.location.reload(1);
                    }, 5000);
                }

                if (response.data.Accepted == "Wait") {

                    localizationService.localize("umbcheckout_license_check_request_wait_title").then(function (title) {
                        localizationService.localize("umbcheckout_license_check_request_wait_message").then(function (message) {
                            notificationsService.warning(title, message + response.data.TimeLeft);
                        });
                    });

                    vm.checkLicenceButtonState = "error";
                }
            })
            .catch(
                function (response) {
                    localizationService.localize("umbcheckout_license_check_request_failed_title").then(function (title) {
                        localizationService.localize("umbcheckout_license_check_request_failed_message").then(function (message) {
                            notificationsService.error(title, message);
                        });
                    });

                    vm.checkLicenceButtonState = "error";
                }
            );
    }
    vm.checkLicence = checkLicense;
    vm.saveConfiguration = saveConfiguration;
}
angular.module("umbraco").controller("UmbCheckout.Settings.Controller", UmbCheckout);
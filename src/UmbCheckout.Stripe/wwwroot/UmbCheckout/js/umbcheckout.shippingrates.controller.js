function UmbCheckout(umbCheckoutResources, $location) {
    var vm = this;
    vm.shippingRates = [];
    vm.LicenseState = {}

    umbCheckoutResources.getLicenseStatus()
        .then(function (response) {
            if (response.data == "Invalid") {
                vm.LicenseState.Valid = false;
                vm.LicenseState.Message = "UmbCheckout is running in unlicensed mode, please <a href=\"#\" target=\"_blank\"  class=\"red bold underline\">purchase a license</a> to support development"
            }
        }
        );

    umbCheckoutResources.getShippingRates()
        .then(function (response) {
            angular.forEach(response.data, function (value, key) {

                value.editPath = "/settings/umbCheckout/shippingRate/" + value.id
            });

            vm.shippingRates = response.data
        }
    );

    vm.options = {
        includeProperties: [
            { alias: "value", header: "Value" }
        ]
    };

    vm.clickItem = clickItem;

    function clickItem(item) {
        $location.path(item.editPath);
    }

    vm.clickCreateButton = clickCreateButton;

    function clickCreateButton() {
        $location.path("/settings/umbCheckout/shippingRate");
    }
}
angular.module("umbraco").controller("UmbCheckout.ShippingRates.Controller", UmbCheckout);
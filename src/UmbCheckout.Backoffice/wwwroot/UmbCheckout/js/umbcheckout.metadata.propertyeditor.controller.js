function UmbCheckout($scope, $timeout) {

    $scope.promptIsVisible = "-1";

    $scope.sortableOptions = {
        axis: 'y',
        containment: 'parent',
        cursor: 'move',
        items: '> div.textbox-wrapper',
        tolerance: 'pointer',
        disabled: $scope.readonly
    };

    if (!$scope.model.value) {
        $scope.model.value = [];
    }

    $scope.add = function ($event) {
        if ($scope.readonly) {
            $event.preventDefault();
            $event.stopPropagation();
            return;
        }

        $scope.model.value.push({ value: "" });

        // Focus new value
        var newItemIndex = $scope.model.value.length - 1;
        $scope.model.value[newItemIndex].hasFocus = true;
        validate();
    };

    $scope.remove = function (index) {
        if ($scope.readonly) return;

        // Make sure not to trigger other prompts when remove is triggered
        $scope.hidePrompt();

        var remainder = [];
        for (var x = 0; x < $scope.model.value.length; x++) {
            if (x !== index) {
                remainder.push($scope.model.value[x]);
            }
        }
        $scope.model.value = remainder;
    };

    $scope.showPrompt = function (idx, item) {
        if ($scope.readonly) return;

        var i = $scope.model.value.indexOf(item);

        // Make the prompt visible for the clicked tag only
        if (i === idx) {
            $scope.promptIsVisible = i;
        }
    };

    $scope.hidePrompt = function () {
        $scope.promptIsVisible = "-1";
    };

    function validate() {
        if ($scope.multipleTextboxForm) {
            var invalid = $scope.model.validation.mandatory && !$scope.model.value.length
            $scope.multipleTextboxForm.mandatory.$setValidity("minCount", !invalid);
        }
    }

    $timeout(function () {
        validate();
    });

    // We always need to ensure we dont submit anything broken
    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {

        // Filter to items with values
        $scope.model.value = $scope.model.value.filter(el => el.value.trim() !== "") || [];
    });

    // When the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
        unsubscribe();
    });
}
angular.module("umbraco").controller("UmbCheckout.MetaData.PropertyEditor.Controller", UmbCheckout);
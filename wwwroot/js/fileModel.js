(function () {
    'use strict';
    var app = angular.module('app');

    /*
     A directive to enable two way binding of file field
     */
    app.directive('fileModel', function ($parse) {
        return {
            restrict: 'A', 

            link: function (scope, element, attrs) {
                var model = $parse(attrs.fileModel),
                    modelSetter = model.assign; 

                element.bind('change', function () {
                    scope.$apply(function () {
                        modelSetter(scope, element[0].files[0]);
                    });
                });
            }
        };
    });
})();
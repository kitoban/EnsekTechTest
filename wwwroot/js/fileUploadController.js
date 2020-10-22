(function () {
    'use strict';
    var app = angular.module('app');
    app.controller('FileUploadController', function ($scope, fileUploadService) {

        function setupUploadStatuses() {
            $scope.uploadStatuses = {
                successful: [],
                invalidLine: [],
                unknownAccount: [],
                duplicateMeterReading: [],
                invalidMeterReading: []
            };
        }

        $scope.getFileLineNumbers = function(arr) {
            return arr.map(function(e) {
                return e.lineIndex;
            });
        }

        $scope.uploadFile = function () {
            var file = $scope.file;
            $scope.uploadStatuses = undefined;

            var uploadUrl = "../upload/UploadFile",
                promise = fileUploadService.uploadFileToUrl(file, uploadUrl);

            promise.then(
                function (response) {
                    setupUploadStatuses();
                    $scope.uploadStatuses.successful = [];
                    $scope.uploadStatuses.invalidLine = [];
                    $scope.uploadStatuses.unknownAccount = [];
                    $scope.uploadStatuses.duplicateMeterReading = [];
                    $scope.uploadStatuses.invalidMeterReading = [];

                    response.data.forEach(function(i) {
                        switch (i.status) {
                            case 'Success':
                                $scope.uploadStatuses.successful.push(i);
                                break;
                            case 'InvalidLine':
                                $scope.uploadStatuses.invalidLine.push(i);
                                break;
                            case 'UnknownAccount':
                                $scope.uploadStatuses.unknownAccount.push(i);
                                break;
                            case 'DuplicateMeterReading':
                                $scope.uploadStatuses.duplicateMeterReading.push(i);
                                break;
                            case 'InvalidMeterReading':
                                $scope.uploadStatuses.invalidMeterReading.push(i);
                                break;
                            default:
                        }
                    });

                },
                function() {
                    $scope.serverResponse = 'An error has occurred';
                });
        };
    });

})();
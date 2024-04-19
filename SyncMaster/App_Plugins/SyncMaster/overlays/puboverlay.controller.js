(function () {
    'use strict';

    function customOverlayController($scope, $http, umbRequestHelper, $location, editorState, contentResource) {

        var stage1 = document.getElementById("sapstage1");
        var stage2 = document.getElementById("sapstage2");
        var stage3 = document.getElementById("sapstage3");
        var sapstage3_2 = document.getElementById("sapstage3_2");
        var vm = this;
        $scope.model.complete = false;
        //stage1.style.display = "block";
        //stage3.style.display = "none";
        vm.step = 1;
        var apiUrl = umbRequestHelper.getApiUrl("publishBaseUrl", "")
        vm.icon = 'icon-box';
        var selNode;
        vm.content = 'A custom overlay.'
        $scope.nodeChanges = [];
        var dsd = document.getElementsByClassName("btn umb-button__button btn-success umb-button-- umb-outline");
        dsd[1].textContent = "Continue";

        // add method to model, so we can call it from parent 
        $scope.model.process = process;
        var selectServer = [];
        vm.$onInit = function () {
            stage1 = document.getElementById("sapstage1");
            stage2 = document.getElementById("sapstage2");
            stage3 = document.getElementById("sapstage3");
            sapstage3_2.style.display = "none"
            stage1.style.display = "flex";
            stage2.style.display = "none";
            stage3.style.display = "none";
            var url = $location.$$url;
            var array = url.split('/');
            var nodeId = array[array.length - 1];
            contentResource.getById(nodeId)
                .then(function (data) {

                    selNode = data;

                });
            vm.checkConnection();
        }


        function process() {
            vm.step++;
            $scope.model.description = 'Step ' + vm.step;

            switch (vm.step) {
                case 2:
                    $scope.model.title = 'Publish to ' + selectServer.name;
                    $scope.model.subtitle = selectServer.Server;
                    stage1.style.display = "none";
                    stage2.style.display = "block";
                    stage3.style.display = "none";
                    dsd[1].textContent = 'Continue';
                    break;
                case 3:
                    $scope.model.title = 'Report';
                    $scope.model.subtitle = 'Details of what will change';
                    vm.findDifference();
                    stage1.style.display = "none";
                    stage2.style.display = "none";
                    stage3.style.display = "block";
                    dsd[1].textContent = 'Continue';
                    break;
                case 4:
                    $scope.model.title = selectServer.name+' Complete';
                    $scope.model.subtitle = ':[' + selectServer.Server + ']';
                    stage1.style.display = "none";
                    stage2.style.display = "none";
                    stage3.style.display = "none";
                    vm.icon = 'icon-check color-green';
                    vm.content = 'We are done now';
                    dsd[1].textContent = 'Done';
                    $scope.model.complete = true;
                    break;
            }


        }

        vm.checkConnection = function () {
            $http({
                url: "/umbraco/Publish/PublishServerConfig/All",
                method: "GET"
            }).then(function (x) {
                $scope.listItems = [];
                x.data.forEach(function (item) {
                    $scope.listItems.push({
                        'url': item.url,
                        'name': item.name
                    });
                });
                vm.success = [];
                if ($scope.listItems.length != 0) {
                    $scope.listItems.forEach(function (item) {
                        var uslTest = item.url.slice(0, -1);
                        $http({
                            url: uslTest + apiUrl + "HeartBeat",
                            method: "GET",
                        }).then(function (response) {
                            if (response.data == 1) {
                                vm.success.push({
                                    Server: item.url,
                                    Status: "0",
                                    name: item.name
                                });
                            }
                        }).catch(error => {
                            vm.success.push({
                                Server: item.url,
                                Status: "1",
                                name: item.name
                            });
                        });
                    });
                }
            });
        };

        vm.GetSelected = function (val) {
            document.getElementById("urlAddress").value = val.Server;
            selectServer = val;
            $scope.model.disableSubmitButton = false;
        }
        vm.showTable = function () {
            var sf = $scope.nodeChanges;
            sapstage3_2 = document.getElementById("sapstage3_2");
            if (sapstage3_2.style.display == "block") {
                sapstage3_2.style.display = "none"
            }
            else {
                sapstage3_2.style.display = "block"
            }

        }
        vm.findDifference = async function () {

            $http({
                url: apiUrl + "GetNode",
                method: "GET",
                params: { "id": selNode.key }
            }).then(function (response1) {

                var entrUrl = document.getElementById("urlAddress").value.trim(' ');
                entrUrl = entrUrl.slice(0, -1);
                $http({
                    url: entrUrl + apiUrl + "GetNode",
                    method: "GET",
                    params: { "id": selNode.key }
                }).then(function (response2) {

                    var dataX = {
                        'X1': response1.data,
                        'X2': response2.data
                    };
                    $http({
                        url: apiUrl + "FindDifferences",
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        data: dataX
                    }).then(function (response3) {

                        if (response3.data.length != 0) {

                            $scope.nodeChanges = [];

                            $scope.nodeChanges.push({
                                'symb': "icon-sync",
                                'change': "Change",
                                'name': selNode.variants[0].name
                            });

                        }
                    });
                });
            })
        }

    }

    angular.module('umbraco')
        .controller('pubOverlayController', customOverlayController);
})();
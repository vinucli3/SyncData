

angular.module('umbraco').controller('PublishTocontroller', function ($filter, $scope, appState, contentResource, $http, umbRequestHelper, editorService, $location) {

    var vm = this;
    var apiUrl = umbRequestHelper.getApiUrl("publishBaseUrl", "")
    vm.url = document.getElementById("urlAddress");
    var stage1 = document.getElementById("stage1");
    var stage2 = document.getElementById("stage2");
    var stage3 = document.getElementById("stage3");
    var stage4 = document.getElementById("stage4");
    var stage2_0 = document.getElementById("stage2_0");
    var stage2_0_1 = document.getElementById("stage2_0_1");
    var stage2_0_2 = document.getElementById("stage2_0_2");
    var diffstage1 = document.getElementById("diffstage1");
    var diffstage2 = document.getElementById("diffstage2");
    var stage2_1 = document.getElementById("stage2_1");
    var stage2_2 = document.getElementById("stage2_2");
    var ttle = document.getElementsByClassName("umb-box-header-title");
    var desc = document.getElementsByClassName("umb-box-header-description");
    vm.desc = "Select a server for publish";
    vm.title = "Select a server";
    vm.selectSite = [];
    allNodes = [];
    vm.Active = "";
    $scope.nodeDifference = [];
    vm.changeTab = function (selectedTab) {
        debugger;
        vm.tabs.forEach(function (tab) {
            tab.active = false;
        });
        selectedTab.active = true;
    };
    var inputs = [];
    var detailTab; var viewTab;

    vm.navigation = [{
        alias: "detail",
        active: "false",
        hasError: "true",
        name: "Detail",
        icon: "icon-car"
    }, {
        alias: "view",
        active: "false",
        hasError: "true",
        name: "View",
        icon: "icon-car"
    }];

    angular.element(document).ready(function () {
        inputs = document.getElementsByTagName('button');
        diffstage2 = document.getElementById("diffstage2");
        detailTab = $filter('filter')(inputs, { 'innerText': "Detail" })[0];
        viewTab = $filter('filter')(inputs, { 'innerText': "View" })[1];
        ttle = $filter('filter')(ttle, { 'innerText': "Select a server" })[0];
        if (diffstage2 != null) { diffstage2.style.display = "none"; }

        if (viewTab != null) { viewTab.classList.remove("is-active"); }
        stage1 = document.getElementById("stage1");
        stage2 = document.getElementById("stage2");
        stage3 = document.getElementById("stage3");
        stage4 = document.getElementById("stage4");
        stage2_0 = document.getElementById("stage2_0");
        stage2_0_1 = document.getElementById("stage2_0_1");
        stage2_0_2 = document.getElementById("stage2_0_2");
        stage2.style.display = "none";
        stage3.style.display = "none";
        stage4.style.display = "none";
        stage2_0.style.display = "none";
        stage2_0_1.style.display = "none";
        stage2_0_2.style.display = "none";
        stage2_1 = document.getElementById("stage2_1");
        var metaData = [];
        var actions = appState.getMenuState("menuActions");
        _.each(actions, function (action) {

            if (action.alias === "publishto") {

                metaData = action.metaData;
                if (metaData.length == 0) {
                    var url = $location.$$url;
                    var array = url.split('/');
                    metaData.data = array[array.length - 1];
                }

                if (metaData != undefined) {
                    if (metaData.data == -1) {
                        contentResource.getChildren(-1).then(function (data) {
                            $scope.allNodes = data.items;
                            stage2.style.display = "none";
                            stage3.style.display = "none";
                            vm.checkConnection();
                        });
                    }
                    else {
                        contentResource.getById(metaData.data)
                            .then(function (data) {
                                $scope.allNodes = [];
                                $scope.allNodes.push(data);
                                stage2.style.display = "none";
                                stage3.style.display = "none";
                                vm.checkConnection();
                            });
                    }
                }
            }
        });

    });
    vm.checkConnection = function () {


        //var result = selNode.key;

        $http({
            url: "/umbraco/Publish/PublishServerConfig/All",
            method: "GET"
        }).then(function (x) {
            var listItems = [];
            x.data.forEach(function (item) {
                listItems.push({
                    'url': item.url,
                    'name': item.name
                });
            });
            vm.success = [];
            if (listItems.length != 0) {
                listItems.forEach(function (item) {
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
        vm.selectSite = val;
    }
    vm.contentDifference = function () {

        if (stage2_1.style.display == "block") {
            ttle.innerText = "Processing..";
            desc.innerText = "push to " + vm.selectSite.name;
            stage2_1.style.display = "none";
            stage2.style.display = "none";
            stage3.style.display = "block";
            document.getElementById("pub-Button").style.display = "none";
            angular.forEach($scope.allNodes, function (selNode) {
                $http({
                    url: apiUrl + "GetNode",
                    method: "GET",
                    params: { "id": selNode.key }
                }).then(function (response1) {
                    var entrUrl = document.getElementById("urlAddress").value.trim(' ');
                    entrUrl = entrUrl.slice(0, -1);
                    var dataX = {
                        'X1': response1.data,
                    };
                    $http({
                        url: entrUrl + apiUrl + "ClearDifferences",
                        method: "POST",
                        headers: {
                            'Content-Type': 'application/json',
                            'Access-Control-Allow-Origin': '*'
                        },
                        data: dataX
                    }).then(function (response2) {
                        stage3.style.display = "none";
                        $scope.pubText = "Publish completed"
                        stage4.style.display = "block";
                    });
                });
            });
            //alert("Published");
            return;
        }

        stage2_2 = document.getElementById("stage2_2");
        stage2_2.style.display = "none"
        ttle.innerText = "Report";
        desc.innerText = "Details of what will change";
        //head.outerText = "Report \nDetails of what will change";
        stage1.style.display = "none";
        var count = 1;
        $scope.noChange = 1;
        var countAllnodes = 0;
        angular.forEach($scope.allNodes, function (selNode) {

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

                    if (response2.data == "New Node") {
                        response2.data = { "Content": "" };
                    }

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
                        countAllnodes++;
                        if (response3.data.length != 0) {
                            $scope.noChange = 0;
                            $scope.nodeDifference = [];
                            $scope.nodeChanges = [];
                            contentResource.getById(selNode.id)
                                .then(function (data) {

                                    $scope.myText = "Change (s) Detected";// response3.data
                                    $scope.mySubText = count + " item(s) was updated";
                                    $scope.myIcon = response3.data[0].PropAction == "Create" ? "icon-add" : "icon-sync";
                                    $scope.myIconColr = response3.data[0].PropAction == "Create" ? { "color": "green" } : { "color": "orange" };
                                    var url = $location.absUrl();
                                    var array = url.split('/');
                                    count++;
                                    angular.forEach(response3.data, function (value) {

                                        $scope.nodeDifference.push({
                                            'action': value.PropAction,
                                            'item': value.PropName,
                                            'old': value.PropOldValue,
                                            'new': value.PropCurrValue,
                                            'node': data.variants[0].name,
                                            'symb': $scope.myIcon,
                                            'symbClr': $scope.myIconColr,
                                            'localUrl': array[0] + "//" + array[2] + data.urls[0].text,
                                            'remoteUrl': entrUrl + data.urls[0].text
                                        });
                                    });

                                    $scope.nodeChanges.push({
                                        'symb': $scope.myIcon,
                                        'symbClr': $scope.myIconColr,
                                        'change': response3.data[0].PropAction,
                                        'name': data.variants[0].name
                                    });
                                });

                            stage2_0.style.display = "block"
                            stage2_1.style.display = "block";
                        }
                        else {
                            if ($scope.allNodes.length == countAllnodes) {
                                if ($scope.noChange == 1) {
                                    $scope.myText = "No changes detected"
                                    document.getElementById("pub-Button").style.display = "none";
                                    stage2_1.style.display = "none";
                                    stage2_0_1.style.display = "block"
                                    stage2_0_2.style.display = "block"
                                }
                            }
                        }
                        stage2.style.display = "block";
                    });
                });
            })

        });

    };
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

                        $scope.nodeDifference = [];
                        angular.forEach(response3.data, function (value) {

                            $scope.nodeDifference.push({
                                'action': value.PropAction,
                                'item': "Property-" + value.PropName,
                                'difference': value.PropCurrValue
                            });
                        });

                    }
                });
            });
        })
    }
    vm.highlight = function (str) {
        const newdiv = document.createElement('div');
        var text = " <span class='imp'>" + str + "</span> ";
        newdiv.innerHTML = text;
        return newdiv;
    }
    vm.showDetail = async function (nodeName) {
        var detail = $scope.nodeDifference.filter(function (obj) {
            if (obj.node === nodeName) {
                return obj;
            }
        });
        debugger;
        var option = {
            data: detail,
            title: 'Create',
            view: "/App_Plugins/cSyncMaster/backoffice/syncAlias/showdiff.html",
            close: function () {
                stage2.style.display = "block";
                editorService.close();
            }
        };
        editorService.open(option);
    }
    vm.selectNavigationItem = function (item) {
        diffstage1.style.display = "none";
        diffstage2.style.display = "none";
        detailTab.classList.remove("is-active");
        viewTab.classList.remove("is-active");

        if (item.name == "Detail") {
            diffstage1.style.display = "block";
            detailTab.classList.add("is-active");
        }
        else {
            diffstage2.style.display = "block";
            viewTab.classList.add("is-active");
        }


    }
    vm.showTable = function () {
        stage2_2 = document.getElementById("stage2_2");
        if (stage2_2.style.display == "block") {
            stage2_2.style.display = "none"
        }
        else {
            stage2_2.style.display = "block"
        }

    }
});

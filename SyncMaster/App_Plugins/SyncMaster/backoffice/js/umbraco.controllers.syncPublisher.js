

(function () {
    'use strict';
    var myApp = angular.module('umbraco');
    myApp.factory('PublishDatacontroller', function dialogManager($timeout, editorService, navigationService) {

        return {
            openCreateDialog: openCreateDialog,
            //openDictionaryDialog: openDictionaryDialog,
            //openItem: openItem,
            //openJob: openJob,
            //openSet: openSet
        };

        function openCreateDialog(options, cb) {
            //var options = {
            //    id: "1139",
            //    submit: function () { /* submit function */ },
            //    close: function () { editorService.close(), navigationService.hideNavigation(); },
            //};
            //editorService.contentEditor(options);
            
            var option = {
                entity: {
                    id: options.entity.id * 1,
                    name: options.entity.name
                },
                languages: options.languages,
                title: 'Create',
                view: "/App_Plugins/cSyncMaster/backoffice/syncAlias/publishto.html",
                size: 'small',
                //submit: function (done) {
                //    debugger;
                //    //editorService.close();
                //    //navigationService.hideNavigation();
                //    //if (cb !== undefined) {
                //    //    cb(true);
                //    //}
                //},
                close: function () {
                    editorService.close();
                    navigationService.hideNavigation();
                    if (cb !== undefined) {
                        cb(false);
                    }
                }
            };
            editorService.open(option);

        };
    });
   

}) ();





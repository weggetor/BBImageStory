(function () {
    
    "use strict";
    
    angular.module("quickSettings", [])
        .factory("quickSettingsService", quickSettingsService)
        .controller("quickSettingsController", quickSettingsController);
 
    quickSettingsService.$inject = ["$http", "serviceRoot"];
    quickSettingsController.$inject = ["$log","quickSettingsService","moduleId", "moduleProperties"];

    function quickSettingsService($http, serviceRoot) {

        var service = {};
        service.loadSettings = loadSettings;
        service.saveSettings = saveSettings;

        function loadSettings() {
            return $http.get(serviceRoot + "settings/loadSettings");
        };

        function saveSettings(settings) {
            return $http.post(serviceRoot + "settings/savesettings", settings);
        };

        return service;
    }

    function quickSettingsController($log, quickSettingsService, moduleId, moduleProperties) {

        var vm = this;
        vm.moduleProperties = JSON.parse(moduleProperties);

        vm.settings = { ImageWidth: 600, Partitioning: "1", View:"view.html", List:"list.html" , ViewTemplates:["view.html"], ListTemplates:["list.html"]};
        vm.saveSettings = saveSettings;

        loadSettings();
        
        function loadSettings() {
            quickSettingsService.loadSettings()
                 .success(function (response) {
                     vm.settings = response;
                     $('#BBImageStory-QuickSettings-' + moduleId).dnnQuickSettings({
                         moduleId: moduleId,
                         onSave: vm.saveSettings
                     });
                 })
                 .error(function (errData) {
                     $log.error('failure loading items', errData);
                 });
        }

        function saveSettings() {
            var prom = quickSettingsService.saveSettings(vm.settings);
            prom.done = function() {
                window.location.reload();
            };
            return prom;
        }
    }
})();
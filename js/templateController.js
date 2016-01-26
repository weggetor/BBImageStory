(function () {
    "use strict";
    
    angular
        .module("imageStoryApp")
        .controller("templateController", templateController);

    templateController.$inject = ["$scope", "$location", "$routeParams", "$window", "$log", "imageStoryService", "ngProgress","ngToast", "moduleProperties"];

    function templateController($scope, $location, $routeParams, $window, $log, imageStoryService, ngProgress, ngToast, moduleProperties) {

        var vm = this;

        vm.moduleProperties = JSON.parse(moduleProperties);
        vm.localize = vm.moduleProperties.Resources;
        vm.settings = vm.moduleProperties.Settings;
        vm.editable = (vm.moduleProperties.Editable == true);
        vm.moduleId = parseInt(vm.moduleProperties.ModuleId);
        vm.userId = parseInt(vm.moduleProperties.UserId);

        if (!vm.moduleProperties.Admin)
            $window.location.href = "#view";

        vm.editorOptions = {
            lineWrapping: true,
            lineNumbers: true,
            matchBrackets: true,
        };
        
        vm.template = {"Type":"view","FileName":"View.html","Content":"","Mode":"text/html"};
        vm.templates = [];
        vm.newMode = false;

        vm.getTemplates = getTemplates;
        vm.loadTemplate = loadTemplate;
        vm.newTemplate = newTemplate;
        vm.saveTemplate = saveTemplate;
        vm.cancelTemplate = cancelTemplate;
        vm.endTemplates = endTemplates;
        vm.selectedTypeChange = selectedTypeChange;
        vm.selectedNameChange = selectedNameChange;

        getTemplates();

        function getTemplates() {
            ngProgress.color('red');
            ngProgress.start();
            imageStoryService.getTemplates(vm.template.Type)
                .success(function (response) {
                    vm.templates = response;
                    vm.template.FileName = (vm.templates[0]);
                    loadTemplate(false);
                    imageStoryService.addEditMenu(vm.moduleId);
                    imageStoryService.setEditMenu(vm.moduleId, false);
                    ngProgress.complete();
                })
                .error(function (errData) {
                    $log.error(vm.localize.GetTemplates_DataError, errData);
                    ngToast.danger({ content: vm.localize.GetTemplates_DataError + ' ' + errData.Message });
                    ngProgress.complete();
                });
        }

        function loadTemplate(def) {
            ngProgress.color('red');
            ngProgress.start();
            imageStoryService.loadTemplate(vm.template.Type,vm.template.FileName, def)
                .success(function (response) {
                    vm.template = response;
                    vm.editorOptions.mode = vm.template.Mode;
                    ngProgress.complete();
                })
                .error(function (errData) {
                    $log.error(vm.localize.GetTemplate_DataError, errData);
                    ngToast.danger({ content: vm.localize.GetTemplate_DataError + ' ' + errData.Message });
                    ngProgress.complete();
                });
        }
        
        function cancelTemplate() {
            vm.template = angular.copy(vm.originalTemplate);
            vm.originalTemplate = {};
            vm.newMode = false;
        }
        
        function newTemplate() {
            vm.originalTemplate = angular.copy(vm.template);
            vm.template.FileName = "new" + vm.template.Type.toUpperCase() + ".html";
            vm.newMode = true;
        }
        
        function saveTemplate() {
            vm.newMode = false;
            if ((vm.template.Type == "view" || vm.template.Type == "list") && vm.template.FileName.lastIndexOf(".html") <= 0) {
                vm.template.FileName += ".html";
            }
            imageStoryService.saveTemplate(vm.template)
                .success(function (response) {
                    if (vm.template.Type == "view" || vm.template.Type == "list")
                        vm.templates.unshift(vm.template.FileName);
                    loadTemplate(false);
                })
                .error(function (errData) {
                    cancelTemplate();
                    $log.error(vm.localize.SaveTemplate_DataError, errData);
                    ngToast.danger({ content: vm.localize.SaveTemplate_DataError + ' ' + errData.Message });
                });
        }
        
        function endTemplates() {
            vm.newMode = false;
            $window.location.href = "#view";
        }
        
        function selectedNameChange() {
            loadTemplate(false);
        }
        
        function selectedTypeChange() {
            getTemplates(vm.template.Type);
        }
    };
})();

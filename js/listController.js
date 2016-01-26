(function () {
    "use strict";
    
    angular
        .module("imageStoryApp")
        .controller("listController", listController);

    listController.$inject = ["$scope", "$routeParams", "$location", "$window", "$log", "$sce", "ngProgress", "ngToast","imageStoryService", "moduleProperties"];

    function listController($scope, $routeParams, $location, $window, $log, $sce, ngProgress, ngToast, imageStoryService, moduleProperties) {

        var vm = this;
        vm.stories = [];
        vm.moduleProperties = JSON.parse(moduleProperties);
        vm.localize = vm.moduleProperties.Resources;
        vm.settings = vm.moduleProperties.Settings;
        vm.editable = (vm.moduleProperties.Editable == true);
        vm.moduleId = parseInt(vm.moduleProperties.ModuleId);
        vm.userId = parseInt(vm.moduleProperties.UserId);

        // Depending on scope setting selecting storydata is filtered by moduleId / portalId or all data
        vm.partModuleId = -1;
        vm.partPortalId = -1;
        if (vm.moduleProperties.Settings.Partitioning) {
            var partition = parseInt(vm.moduleProperties.Settings.Partitioning);
            if (partition == 1) {
                vm.partModuleId = vm.moduleProperties.ModuleId;
            } else if (partition == 2) {
                vm.partPortalId = vm.moduleProperties.PortalId;
            } else if (partition == 3) {
                vm.partPortalId = vm.moduleProperties.PortalId;
                vm.partModuleId = vm.moduleProperties.ModuleId;
            }
        }
        getStories();

        vm.getStories = getStories;
        vm.viewStory = viewStory;
        vm.getHtml = getHtml;

        function getStories() {
            ngProgress.color('red');
            ngProgress.start();
            imageStoryService.getStories(vm.partModuleId, vm.partPortalId)
                .success(function(response) {
                    vm.stories = response;
                    for (var i = 0; i < vm.stories.length; i++) {
                        if (vm.stories[i].StartDate)
                            vm.stories[i].StartDate = new Date(vm.stories[i].StartDate);
                        if (vm.stories[i].EndDate)
                            vm.stories[i].EndDate = new Date(vm.stories[i].EndDate);
                    }
                    imageStoryService.addEditMenu(vm.moduleId);
                    imageStoryService.setEditMenu(vm.moduleId,$location.path().startsWith("/edit"));
                    ngProgress.complete();
                })
                .error(function(errData) {
                    $log.error(vm.localize.GetStories_DataError, errData);
                    ngToast.danger({ content: vm.localize.GetStories_DataError + ' ' + errData.Message });
                    ngProgress.complete();
                });
        }

        function viewStory(story) {
            $window.location.href = "#view/" + story.StoryId;
        }
        
        function getHtml(content) {
            return $sce.trustAsHtml(content);
        }
    }
})();

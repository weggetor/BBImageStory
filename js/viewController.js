(function () {
    "use strict";

    angular
        .module("imageStoryApp")
        .controller("viewController", viewController);

    viewController.$inject = ["$scope", "$routeParams", "$location", "$window", "$log", "$sce", "ngProgress","ngToast", "imageStoryService","moduleProperties"];

    function viewController($scope, $routeParams, $location, $window, $log, $sce, ngProgress, ngToast, imageStoryService, moduleProperties) {

        var vm = this;
        vm.story = null;
        vm.images = [];
        vm.index = 0;
        vm.activeImage = {};
        vm.nextIndex = 0;
        vm.prevIndex = 0;
        vm.bootstrap3 = (typeof $().emulateTransitionEnd == 'function');
        vm.noStory = false;
        
        vm.getImages = getImages;
        vm.getHtml = getHtml;
        vm.goIndex = goIndex;
        
        vm.moduleProperties = JSON.parse(moduleProperties);
        vm.localize = vm.moduleProperties.Resources;
        vm.settings = vm.moduleProperties.Settings;
        vm.editable = (vm.moduleProperties.Editable == true);
        vm.moduleId = parseInt(vm.moduleProperties.ModuleId);
        vm.userId = parseInt(vm.moduleProperties.UserId);

        // Determine the Viewport width
        vm.imageWidth = vm.moduleProperties.Settings.Width ? vm.moduleProperties.Settings.Width : 600;

        // Depending on partitioning setting selecting storydata is filtered by moduleId / portalId or all data
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
        if ($routeParams.StoryId)
            getStory(parseInt($routeParams.StoryId));
        else {
            getStory(0);
        }

        if ($routeParams.Index) {
            vm.index = parseInt($routeParams.Index);
        }

        function goIndex(index) {
            vm.index = index;
            vm.activeImage = vm.images[vm.index];
            vm.nextIndex = vm.index + 1;
            if (vm.nextIndex > vm.images.length - 1)
                vm.nextIndex = 0;
            vm.prevIndex = vm.index - 1;
            if (vm.prevIndex < 0)
                vm.prevIndex = vm.images.length - 1;
        }

        function getStory(storyId) {
            imageStoryService.getStory(storyId, vm.partModuleId, vm.partPortalId, false)
                .success(function (response) {
                    vm.story = response;
                    if (vm.story) {
                        getImages(vm.story.StoryId);
                    } else {
                        vm.noStory = true;
                    }
                    imageStoryService.addEditMenu(vm.moduleId);
                    imageStoryService.setEditMenu(vm.moduleId, false);
                })
                .error(function (errData) {
                    $log.error(vm.localize.GetStory_DataError, errData);
                    ngToast.danger({ content: vm.localize.GetStory_DataError + ' ' + errData.Message });
                });
        }

        function getImages(storyId) {
            imageStoryService.getImages(storyId)
                .success(function(response) {
                    var images = response;
                    vm.images = images;
                    vm.activeImage = vm.images[vm.index];
                    vm.nextIndex = vm.index + 1;
                    if (vm.nextIndex > vm.images.length - 1)
                        vm.nextIndex = 0;
                    vm.prevIndex = vm.index - 1;
                    if (vm.prevIndex < 0)
                        vm.prevIndex = vm.images.length - 1;
                })
                .error(function (errData) {
                    $log.error(vm.localize.GetImages_DataError, errData);
                    ngToast.danger({ content: vm.localize.GetImages_DataError + ' ' + errData.Message });
                });
        }
        
        function getHtml(content) {
            return $sce.trustAsHtml(content);
        }
    }
})();

(function () {
    "use strict";
    
    angular
        .module("imageStoryApp")
        .controller("uploadController", uploadController);

    uploadController.$inject = ["$scope", "$routeParams", "$window", "$log", "imageStoryService", "ngProgress", "FileUploader", "moduleProperties"];

    function uploadController($scope, $routeParams, $window, $log, imageStoryService, ngProgress, FileUploader, moduleProperties) {

        var vm = this;
        vm.storyId = parseInt($routeParams.StoryId);
        vm.moduleProperties = JSON.parse(moduleProperties);
        vm.localize = vm.moduleProperties.Resources;
        vm.settings = vm.moduleProperties.Settings;
        vm.editable = (vm.moduleProperties.Editable == true);
        vm.moduleId = parseInt(vm.moduleProperties.ModuleId);
        vm.userId = parseInt(vm.moduleProperties.UserId);
        
        vm.endUpload = endUpload;

        if (vm.editable == false)
            $window.location.href = "#";

        var sf = $.ServicesFramework(vm.moduleId);
        vm.uploader = new FileUploader({ url: sf.getServiceRoot("BBImageStory_Module") + "image/uploadimage" });
        vm.uploader.filters.push({
            name: 'imageFilter',
            fn: function (item /*{File|FileLikeObject}*/, options) {
                var type = '|' + item.type.slice(item.type.lastIndexOf('/') + 1) + '|';
                return '|jpg|png|jpeg|bmp|gif|'.indexOf(type) !== -1;
            }
        });

        vm.uploader.headers = { "ModuleId": sf.getModuleId(), "TabId": sf.getTabId(), "RequestVerificationToken": sf.getAntiForgeryValue() };
        vm.uploader.formData = [{ "storyId": vm.storyId }];

        vm.uploader.onCompleteAll = function () {
            vm.endUpload();
        };
        
        imageStoryService.addEditMenu(vm.moduleId);
        imageStoryService.setEditMenu(vm.moduleId, true);
        
        function endUpload() {
            $window.location.href = "#edit/" + vm.storyId;
        }
    };
})();

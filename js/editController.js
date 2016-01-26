(function () {
    "use strict";

    angular
        .module("imageStoryApp")
        .controller("editController", editController);

    editController.$inject = ["$scope", "$routeParams", "$location", "$window", "$log", "ngProgress","ngToast", "imageStoryService","moduleProperties"];

    function editController($scope, $routeParams, $location, $window, $log, ngProgress, ngToast, imageStoryService, moduleProperties) {

        var vm = this;

        vm.storyId = parseInt($routeParams.StoryId);
        vm.stories = [];
        vm.story = {};
        vm.editIndex = -1;
        vm.images = [];
        
        vm.moduleProperties = JSON.parse(moduleProperties);
        vm.localize = vm.moduleProperties.Resources;
        vm.settings = vm.moduleProperties.Settings;
        vm.editable = (vm.moduleProperties.Editable == true);
        vm.moduleId = parseInt(vm.moduleProperties.ModuleId);
        vm.userId = parseInt(vm.moduleProperties.UserId);
        vm.sortableOptions = { stop: sortStop, disabled: !vm.editable };

        if (vm.moduleProperties.Editable == false)
            window.location.href = vm.moduleProperties.RawUrl + "#view";

        // Determine the Viewport width
        vm.viewPortWidth = vm.moduleProperties.Settings.Width ? vm.moduleProperties.Settings.Width : 600;

        vm.getStories = getStories;
        vm.newStory = newStory;
        vm.editStory = editStory;
        vm.deleteStory = deleteStory;
        vm.viewStory = viewStory;
        vm.cancelEdit = cancelEdit;
        vm.saveEdit = saveEdit;
        vm.getImages = getImages;
        vm.addImages = addImages;
        vm.deleteImage = deleteImage;
        
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
  
        getStories();
 
        function getStories() {
            ngProgress.color('red');
            ngProgress.start();
            imageStoryService.getStories(vm.partModuleId, vm.partPortalId)
                .success(function (response) {
                    vm.stories = response;
                    for (var i = 0; i < vm.stories.length; i++) {
                        if (vm.stories[i].StartDate)
                            vm.stories[i].StartDate = new Date(vm.stories[i].StartDate);
                        if (vm.stories[i].EndDate)
                            vm.stories[i].EndDate = new Date(vm.stories[i].EndDate);
                        if (vm.storyId > 0 && vm.storyId == vm.stories[i].StoryId) {
                            editStory(vm.stories[i], i);
                            break;
                        }
                    }
                    vm.propsLoaded = true;
                    imageStoryService.addEditMenu(vm.moduleId);
                    imageStoryService.setEditMenu(vm.moduleId, $location.path().startsWith("/edit"));
                    ngProgress.complete();
                })
                .error(function (errData) {
                    $log.error(vm.localize.GetStories_DataError, errData);
                    ngProgress.complete();
                    ngToast.danger({ content: vm.localize.GetStories_DataError + ' ' + errData.Message });
                });
        }

        function newStory() {
            vm.story = {};
            vm.editIndex = 99999999;
            vm.images = [];
        }

        function editStory(story, index) {
            vm.story = angular.copy(story);
            vm.storyId = story.StoryId;
            vm.editIndex = index;
            vm.getImages(story.StoryId);
        }

        function deleteStory(story, index) {
            if (confirm(vm.localize.DeleteStory_Confirm.replace('000', story.Title))) {
                imageStoryService.deleteStory(story)
                    .success(function(response) {
                        vm.stories.splice(index, 1);
                        ngToast.success({ content: vm.localize.DeleteStory_DataOK });
                        $window.location.href = "#edit";
                    })
                    .error(function (errData) {
                        $log.error(vm.localize.DeleteStory_DataError, errData);
                        ngToast.danger({ content: vm.localize.DeleteStory_DataError + ' ' + errData.Message });
                    });
            }
            vm.editIndex = -1;
        }
        
        function viewStory(story) {
            $window.location.href = "#view/" + story.StoryId;
        }

        function saveEdit() {
            vm.story.LastModifiedByUserID = vm.userId;
            vm.story.LastModifiedOnDate = new Date();
            vm.story.ModuleId = null;
            vm.story.PortalId = null;
            
            if (vm.partModuleId > -1)
                vm.story.ModuleId = vm.moduleProperties.ModuleId;
            if (vm.partPortalId > -1)
                vm.story.PortalId = vm.moduleProperties.PortalId;

            if (vm.story.StoryId > 0) {
                imageStoryService.updateStory(vm.story)
                    .success(function(response) {
                        if (vm.editIndex > -1) {
                            vm.stories[vm.editIndex] = vm.story;
                        }
                        ngToast.success({ content: vm.localize.UpdateStory_DataOK });
                        imageStoryService.updateImages(vm.images)
                            .catch(function (errData) {
                                $log.error(vm.localize.UpdateImages_DataError, errData);
                                ngToast.danger({ content: vm.localize.UpdateImages_DataError + ' ' + errData.Message });
                            });
                    })
                    .error(function (errData) {
                        $log.error(vm.localize.SaveStory_DataError, errData);
                        ngToast.danger({ content: vm.localize.SaveStory_DataError + ' ' + errData.Message });
                    });
            } else {
                vm.story.CreatedByUserID = vm.userId;
                vm.story.CreatedOnDate = new Date();
                imageStoryService.newStory(vm.story)
                    .success(function(response) {
                        vm.story.StoryId = parseInt(response);
                        vm.stories.unshift(vm.story);
                        imageStoryService.setEditMenu(vm.moduleId, true);
                        ngToast.success({ content: vm.localize.SaveStory_DataOK });
                        $window.location.href = "#edit/" + vm.story.StoryId;
                    })
                    .error(function(errData) {
                        $log.error(vm.localize.SaveStory_DataError, errData);
                        ngToast.danger({ content: vm.localize.SaveStory_DataError + ' ' + errData.Message });
                    });
            }
        }

        function cancelEdit() {
            vm.images = [];
            vm.story = {};
            vm.editIndex = -1;
            vm.storyId = -1;
            imageStoryService.setEditMenu(vm.moduleId);
            $window.location.href = "#edit";
        }

        function getImages(storyId) {
            imageStoryService.getImages(storyId)
                .success(function(response) {
                    var images = response;
                    for (var i = 0; i < images.length; i++) {
                        images[i].ImageUrl = "/dnnimagehandler.ashx?mode=file&File=" + vm.moduleProperties.HomeDirectory + images[i].Folder + images[i].FileName + "&w=200&ResizeMode=FitSquare&border=5&BackColor=%23FFFFFF&NoCache=1&filetype=png";
                    }
                    vm.images = images;
                })
                .error(function (errData) {
                    $log.error(vm.localize.GetImages_DataError, errData);
                    ngToast.danger({ content: vm.localize.GetImages_DataError + ' ' + errData.Message });
                });
        }
        
        function addImages() {
            $window.location.href = "#upload/" + vm.storyId;
        }

        function deleteImage(image, index) {
            imageStoryService.deleteImage(vm.story.StoryId, image.ImageId)
                .success(function(response) {
                    vm.images.splice(index, 1);
                })
                .error(function (errData) {
                    $log.error(vm.localize.DeleteImage_DataError, errData);
                    ngToast.danger({ content: vm.localize.DeleteImage_DataError + ' ' + errData.Message });
                });
        }

        function sortStop(e, ui) {
            for (var index in vm.images) {
                if (vm.images[index].ImageId) {
                    vm.images[index].ViewOrder = index + 1;
                }
            }
        }
    }
})();

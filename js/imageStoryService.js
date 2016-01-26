(function () {
    "use strict";

    angular
        .module("imageStoryApp")
        .factory("imageStoryService", imageStoryService);

    imageStoryService.$inject = ["$http", "serviceRoot"];
    
    function imageStoryService($http, serviceRoot) {

        var service = {};
        service.addEditMenu = addEditMenu;
        service.setEditMenu = setEditMenu;
        service.getStories = getStories;
        service.getStory = getStory;
        service.deleteStory = deleteStory;
        service.newStory = newStory;
        service.updateStory = updateStory;
        service.getImages = getImages;
        service.deleteImage = deleteImage;
        service.updateImages = updateImages;
        service.getTemplates = getTemplates;
        service.loadTemplate = loadTemplate;
        service.saveTemplate = saveTemplate;
        
        function addEditMenu(moduleId) {
            var element = $("#moduleActions-" + moduleId + " ul.dnn_mact");
            var liNode = element.find(".bbActionMenuEdit a").first();
            if (!liNode[0]) {
                element.prepend('<li class="bbActionMenuEdit"><a href="#view">&nbsp;<span class="glyphicon"></span></a></li>');
            }
        }

        function setEditMenu(moduleId,bool) {
            var onGlyph = "glyphicon-pencil";
            var offGlyph = "glyphicon-pencil";
            var onClass = "bbEditOn";
            var offClass = "bbEditOff";
            var element = $("#moduleActions-" + moduleId + " ul");
            var liNode = element.find(".bbActionMenuEdit a").first();
            if (liNode[0]) {
                var spanNode = liNode.find("span").first();
                if (bool) {
                    liNode.attr("href", "#view");
                    spanNode.removeClass(onClass);
                    spanNode.addClass(offClass);
                    spanNode.removeClass(onGlyph);
                    spanNode.addClass(offGlyph);

                } else {
                    liNode.attr("href", "#edit");
                    spanNode.removeClass(offClass);
                    spanNode.addClass(onClass);
                    spanNode.removeClass(offGlyph);
                    spanNode.addClass(onGlyph);
                }
            }
        }
  
        function getStories(moduleId, portalId, ignoreTime) {
            if (typeof ignoreTime !== 'undefined')
                return $http.get(serviceRoot + "story/list?moid=" + moduleId + "&poid=" + portalId + "&ignoreTime=" + ignoreTime);
            else 
                return $http.get(serviceRoot + "story/list?moid=" + moduleId + "&poid=" + portalId);
        };
        
        function getStory(storyId, moduleId, portalId, ignoreTime) {
            if (typeof ignoreTime !== 'undefined')
                return $http.get(serviceRoot + "story/getStory?stid=" + storyId + "&moid=" + moduleId + "&poid=" + portalId + "&ignoreTime=" + ignoreTime);
            else
                return $http.get(serviceRoot + "story/getStory?stid=" + storyId + "&moid=" + moduleId + "&poid=" + portalId);
        };

        function deleteStory(story) {
            return $http.post(serviceRoot + "story/deleteStory", story);
        };
        function newStory(story) {
            return $http.post(serviceRoot + "story/newStory", story);
        };
        function updateStory(story) {
            return $http.post(serviceRoot + "story/updateStory", story);
        };

        function getImages(storyId) {
            return $http.get(serviceRoot + "image/getimagesbystory?stid=" + storyId);
        };

        function deleteImage(storyId, imageId) {
            return $http.get(serviceRoot + "image/deleteImage?stid="+ storyId + "&imid=" + imageId);
        };
        function updateImages(imglist) {
            return $http.post(serviceRoot + "image/UpdateImages",imglist );
        }

        function getTemplates(type) {
            return $http.get(serviceRoot + "template/gettemplates?type=" + type);
        };
        
        function loadTemplate(type, name, def) {
            return $http.get(serviceRoot + "template/loadtemplate?type=" + type + "&name=" + name + "&def=" + def);
        };

        function saveTemplate(template) {
            return $http.post(serviceRoot + "template/savetemplate", template);
        }

        return service;
   }
})();
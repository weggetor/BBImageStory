(function () {
    "use strict";

    angular.module("imageStoryApp", ["ngRoute", "ngSanitize", "ngProgress", "ngToast", "ui.sortable", "ui.codemirror", "angularFileUpload"])
    .config(['ngToastProvider', function (ngToastProvider) {
        ngToastProvider.configure({
            animation: 'fade',
            horizontalPosition: 'left'
        });
    }]);

})();
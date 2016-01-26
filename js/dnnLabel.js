(function () {
    "use strict";
    
    angular
        .module("imageStoryApp")
        .directive('dnnLabel', function() {
            return {
                restrict: 'EA',
                template: [
                    '<div class="dnnLabel">',
                    '<label for="">{{text}}</label>',
                    '<a class="dnnFormHelp" href="javascript:void(0)"></a>',
                    '<div class="dnnTooltip" style="position: absolute; right: -29%; top: -45px;">',
                    '<div class="dnnFormHelpContent dnnClear">{{help}}</div>',
                    '</div>',
                    '</div>'
                ].join(""),
                scope: {
                    text: '@',
                    help: '@'
                }
            };
        });
})();
(function () {
    'use strict';

    var serviceId = 'datacontext';
    angular.module('app').factory(serviceId, ['common','config', '$http', datacontext]);

    function datacontext(common, config, $http) {
        var $q = common.$q;

        var serviceBase = config.serviceBase;
        var service = {
            getScrapedWiki: getScrapedWiki,
            getMessageCount: getMessageCount,
            getScrapedDoc: getScrapedDoc
        };

        return service;

        function getMessageCount() { return $q.when(72); }

        function getScrapedWiki(model) {
            return $q.when($http.post(serviceBase + 'api/wiki/scrapepage/', model)
                    .then(function (results) {
                        return results.data;
                    }));
        }

        function getScrapedDoc(model) {
            return $q.when($http.post(serviceBase + 'api/docs/scrapedocument/', model).then(function(results) {
                return results.data;
            }))
        }
    }
})();
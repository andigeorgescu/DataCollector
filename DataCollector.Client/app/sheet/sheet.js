(function () {
    'use strict';
    var controllerId = 'sheet';
    angular.module('app').controller(controllerId, ['common', 'datacontext', '$location', '$route', admin]);

    function admin(common, datacontext, $location, $route) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var vm = this;
        vm.title = 'Sheet scraper';
        vm.keywords = [];

        activate();

        function activate() {
            common.activateController([], controllerId)
                .then(function () { log('Activated Crawler View'); });
        }

        vm.scrape = function () {
            var model = {};
            model.fileName = vm.docFile.filename;
            model.fileData = vm.docFile.base64;

            datacontext.getScrapedSheet(model)
                .then(function (response) {
                    vm.data = response;
                    debugger;
                });
        }

        vm.simulateQuery = false;
        vm.isDisabled = false;

        // list of `state` value/display objects
        vm.states = loadAll();
        vm.querySearch = querySearch;
        vm.selectedItemChange = selectedItemChange;
        vm.searchTextChange = searchTextChange;


        // ******************************
        // Internal methods
        // ******************************

        /**
         * Search for states... use $timeout to simulate
         * remote dataservice call.
         */
        function querySearch(query) {
            var results = query ? vm.states.filter(createFilterFor(query)) : vm.states,
                deferred;
            if (vm.simulateQuery) {
                deferred = $q.defer();
                $timeout(function () { deferred.resolve(results); }, Math.random() * 1000, false);
                return deferred.promise;
            } else {
                return results;
            }
        }

        function searchTextChange(text) {
        }

        function selectedItemChange(item) {
        }

        /**
         * Build `states` list of key/value pairs
         */
        function loadAll() {
            var allStates = 'http://www.apanovabucuresti.ro/buletine-de-analiza-a-apei/';

            return allStates.split(/, +/g).map(function (state) {
                return {
                    value: state.toLowerCase(),
                    display: state
                };
            });
        }

        /**
         * Create filter function for a query string
         */
        function createFilterFor(query) {
            var lowercaseQuery = angular.lowercase(query);

            return function filterFn(state) {
                return (state.value.indexOf(lowercaseQuery) === 0);
            };

        }

        vm.onLoad = function (e, reader, file, fileList, fileOjects, fileObj) {
          
        };

        vm.removeDoc = function() {
            vm.docFile = [];
        }
    }
})();
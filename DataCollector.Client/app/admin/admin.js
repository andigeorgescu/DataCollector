(function () {
    'use strict';
    var controllerId = 'admin';
    angular.module('app').controller(controllerId, ['common', admin]);

    function admin(common) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var iframe = document.getElementById("pageViewFrame");

        var vm = this;
        vm.title = 'Document scraper';
        vm.keywords = [];

        activate();

        function activate() {
            common.activateController([], controllerId)
                .then(function () { log('Activated Admin View'); });
        }

        vm.scrape = function (link) {
            if (!link) return;
           // getWikiData(link).then(function () {
                setIframeSrc(link);
           // })
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
            var allStates = 'http://www.apanovabucuresti.ro/!res/fls/41(13).pdf';

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

        function setIframeSrc(url) {
            iframe.src = url;
        }

        vm.addKeyword = function (key) {
            if (vm.keywords.indexOf(key) > -1) {
                vm.keyword = null;
                return;
            }

            vm.keywords.push(key);
            vm.keyword = null;
        }

        vm.removeKeyword = function (key) {
            var keyIndex = vm.keywords.indexOf(key);
            if (keyIndex < 0) return;

            vm.keywords.splice(keyIndex, 1);
        }
    }
})();
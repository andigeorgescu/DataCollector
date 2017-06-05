(function () {
    'use strict';
    var controllerId = 'admin';
    angular.module('app').controller(controllerId, ['common','datacontext', '$routeParams','$timeout', admin]);

    function admin(common, datacontext, $routeParams, $timeout) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var vm = this;
        vm.title = 'Document scraper';
        vm.keywords = [];

        activate();

        function activate() {
            common.activateController([], controllerId)
                .then(function() {
                    getParams();
                    log('Activated Admin View');
                });
        }

        function getParams() {
            if ($routeParams.foundUrl) {
                vm.searchText = $routeParams.foundUrl;
                $timeout(function () {
                    document.getElementById("btnScrape").click();
                })
                
            }
        }

        vm.scrape = function (link) {
            if (!link) return;
            // getWikiData(link).then(function () {
            vm.link = link;
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
            var iframe = document.getElementById("pageViewFrame");
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

        vm.scrapeDoc = function () {
            var model = {};
            model.Keywords = vm.keywords;
            model.Url = vm.link;
            datacontext.getScrapedDoc(model)
                .then(function (response) {
                    vm.scrapedData = response;
                });
        }
    }
})();
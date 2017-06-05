(function () {
    'use strict';
    var controllerId = 'crawler';
    angular.module('app').controller(controllerId, ['common', 'datacontext', '$location', '$route', admin]);

    function admin(common, datacontext, $location, $route) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var vm = this;
        vm.title = 'Web crawler';
        vm.keywords = [];

        activate();

        function activate() {
            common.activateController([], controllerId)
                .then(function () { log('Activated Crawler View'); });
        }

        vm.scrape = function (link) {
            if (!link || !vm.area) return;
            var model = {};
            model.url = link;
            model.location = vm.area;
            model.key = vm.key;

            datacontext.crawl(model)
                 .then(function (response) {
                     if (response.isMatch) {
                         $location.path('doc').search({ foundUrl: response.urls[0] });
                     } else {
                         if (response.urls.length === 0) {
                             toastr.warning("No results were found");
                             return;
                         }

                         vm.data = response;
                     }
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

        vm.scrapeDoc = function () {
            var model = {};
            model.Keywords = vm.keywords;
            model.Url = vm.link;
            datacontext.getScrapedDoc(model)
                .then(function (response) {
                    vm.scrapedData = response;
                });
        }

        vm.preview = function(link) {
            setIframeSrc(link);
        }

        vm.goToScraper = function(link) {
            $location.path('doc').search({ foundUrl: link });
        }

        function setIframeSrc(url) {
            var iframe = document.getElementById("previewFrame");
            iframe.src = url;
        }
    }
})();
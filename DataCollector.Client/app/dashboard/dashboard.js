(function () {
    'use strict';
    var controllerId = 'dashboard';
    angular.module('app').controller(controllerId, ['common', 'datacontext', '$sce', dashboard]);

    function dashboard(common, datacontext, $sce) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var vm = this;
        vm.title = 'Wiki Scraper';
        vm.pageView = "https://ro.wikipedia.org/wiki/Lista_ora%C8%99elor_din_Rom%C3%A2nia";
        

        activate();

        function activate() {
            var promises = [];
            common.activateController(promises, controllerId)   
                .then(function () { log('Activated Dashboard View'); });
        }

        function getMessageCount() {
            return datacontext.getMessageCount().then(function (data) {
                return vm.messageCount = data;
            });
        }

        function getWikiData(link) {
            var model = {};
            model.Url = link;

            return datacontext.getScrapedWiki(model).then(function (data) {
                return vm.data = data.message;
            });
        }

        vm.scrape = function(link) {
            if (!link) return;
            getWikiData(link).then(function() {
                setIframeSrc(link);
            })
        }

        vm.simulateQuery = false;
        vm.isDisabled    = false;

        // list of `state` value/display objects
        vm.states        = loadAll();
        vm.querySearch   = querySearch;
        vm.selectedItemChange = selectedItemChange;
        vm.searchTextChange = searchTextChange;


        // ******************************
        // Internal methods
        // ******************************

        /**
         * Search for states... use $timeout to simulate
         * remote dataservice call.
         */
        function querySearch (query) {
            var results = query ? vm.states.filter( createFilterFor(query) ) : vm.states,
                deferred;
            if (vm.simulateQuery) {
                deferred = $q.defer();
                $timeout(function () { deferred.resolve( results ); }, Math.random() * 1000, false);
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
            var allStates = 'https://ro.wikipedia.org/wiki/Lista_ora%C8%99elor_din_Rom%C3%A2nia';

            return allStates.split(/, +/g).map( function (state) {
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
    }
})();
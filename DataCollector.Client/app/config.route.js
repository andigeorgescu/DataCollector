(function () {
    'use strict';

    var app = angular.module('app');

    // Collect the routes
    app.constant('routes', getRoutes());
    
    // Configure the routes and route resolvers
    app.config(['$routeProvider', 'routes', routeConfigurator]);
    function routeConfigurator($routeProvider, routes) {

        routes.forEach(function (r) {
            $routeProvider.when(r.url, r.config);
        });
        $routeProvider.otherwise({ redirectTo: '/' });
    }

    // Define the routes 
    function getRoutes() {
        return [
            {
                url: '/',
                config: {
                    templateUrl: 'app/dashboard/dashboard.html',
                    title: 'Wiki Scraper',
                    settings: {
                        nav: 1,
                        content: '<i class="fa fa-desktop"></i> Wiki Scraper'
                    }
                }
            }, {
                url: '/doc',
                config: {
                    title: 'Document Scraper',
                    templateUrl: 'app/admin/admin.html',
                    settings: {
                        nav: 2,
                        content: '<i class="fa fa-file-text-o"></i> Document Scraper'
                    }
                }
            },
            {
                url: '/crawl',
                config: {
                    title: 'Web Crawler',
                    templateUrl: 'app/crawler/crawler.html',
                    settings: {
                        nav: 3,
                        content: '<i class="fa fa-arrows"></i> Web Crawler'
                    }
                }
            }
        ];
    }
})();
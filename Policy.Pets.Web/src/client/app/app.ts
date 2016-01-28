/// <reference path="_all.ts" />

module App {
    'use strict';

    angular.module('policyApp', ['auth0', 'angular-storage', 'angular-jwt',
                                 'controllers', 'ngResource', 'ngRoute', 'ui.router'])
        .config(['$stateProvider', ($stateProvider) => {
            App.Configs.RouteConfig.config($stateProvider);
        }]
    ).config((authProvider) => {
        authProvider.init({
            domain: 'derex.auth0.com',
            clientID: 'yL9jlFUmwEXx8J5RtJ1GmDZ8bC9iZHED'
        });   
    })
    .run((auth) => {
        // This hooks al auth events to check everything as soon as the app starts
        auth.hookEvents();
    })
    .config((authProvider, $routeProvider, $httpProvider, jwtInterceptorProvider) => {
            jwtInterceptorProvider.tokenGetter = [ () => {
                // Return the saved token
                console.log('Call: ' + localStorage.getItem('token'));
                return localStorage.getItem('token');
            }];

            $httpProvider.interceptors.push('jwtInterceptor');
            // ...
        }
    );
}

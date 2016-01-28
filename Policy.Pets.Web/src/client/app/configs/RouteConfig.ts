// /// <reference path='../_all.ts' />

module App.Configs {
    'use strict';

	/**
	 * The RouteConfig defines the application routes, $routeProvider 
     * will also be injected into the route config and will be used to 
     * access the parameters.
	 */
    export class RouteConfig {

        public static config($stateProvider: ng.ui.IStateProvider) {
            $stateProvider
                .state('policies', {
                    url : '/policies',
                    controller: App.Controllers.PoliciesController,
                    templateUrl: "app/views/policies/list.html"
                })
                .state('newPolicy', { //state for adding a new policy
                    url: '/policies/new',
                    templateUrl: 'app/views/policies/add.html',
                    controller: App.Controllers.PoliciesController,
                })
                .state("viewPolicy", {
                    url: '/policies/:id',
                    controller: App.Controllers.PoliciesController,
                    templateUrl: "app/views/policies/view.html"
                }).state('editPolicy', { //state for updating a movie
                    url: '/policies/:id/edit',
                    templateUrl: 'app/views/policies/edit.html',
                    controller: App.Controllers.PoliciesController
                })
                .state('pets', {
                    url: '/policies/:policyId/pets',
                    controller: App.Controllers.PetsController,
                    templateUrl: "app/views/pets/list.html"
                })
                .state('addPetToPolicy', { //state for adding a new policy
                    url: '/policies/:policyId/pets/new',
                    templateUrl: 'app/views/pets/add.html',
                    controller: App.Controllers.PetsController,
                })
                .state('viewPet', { //state for adding a new policy
                    url: '/pets/:id',
                    templateUrl: 'app/views/pets/view.html',
                    controller: App.Controllers.PetsController,
                    })
                .state('editPet', { //state for adding a new policy
                    url: '/pets/:id/edit',
                    templateUrl: 'app/views/pets/edit.html',
                    controller: App.Controllers.PetsController,
                })
                .state('login', { 
                    url: '/login',
                    templateUrl: 'app/views/auth/login.html',
                    controller: App.Controllers.LoginController,
                })
                .state('home', {
                    url: '/home',
                    templateUrl: 'app/views/home.html',
                    controller: App.Controllers.LoginController,
                });
        }
    }

}
 
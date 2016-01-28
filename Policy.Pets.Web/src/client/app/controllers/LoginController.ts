/// <reference path="../_all.ts" />
/// <reference path="../services/httpservice.ts" />

module App.Controllers {
    'use strict';

    //Interface to describe the scope
    export interface ILoginScope extends ng.IScope {
        login: rs.IAuthRequest;
        vm: LoginController;
    }

    export class LoginController {

        public static $inject = [
            '$scope', '$state', '$http', 'auth', '$location', 'authService'
        ];

        constructor(private $scope: ILoginScope,
                    private $state,
                    public http,
                    public auth,
                    public $location,
                    public authService: rs.IAuthRequestResource) {
            this.$scope.vm = this;
            this.$scope.login = new authService();
        }

        public login () : void {
            this.auth.signin({
                primaryColor: 'green',
                icon: 'http://kgiaitri.com/wp-content/uploads/2015/02/Kenhgiaitri-logo1.png',
                connections: ["auth0", "github", "google-oauth2"]
            }, (profile, token) => {
                // Success callback
                localStorage.setItem('profile', profile);
                localStorage.setItem('token', token);
                console.log(token);
                this.$location.path('/');
            }, function () {
                // Error callback
            });
        }

        public logout () : void {
            this.auth.signout();
            localStorage.removeItem('profile');
            localStorage.removeItem('token');
        }

        public signin(): void {
            this.$scope.login.grant_type = "password";
            this.$scope.login.$save((jwtToken: rs.IJwtToken) => {
                console.log(jwtToken);
                localStorage.setItem('token', jwtToken.access_token);
                this.$state.go('policies'); 
            });
        }

        public signout(): void {
            localStorage.removeItem('profile');
            localStorage.removeItem('token');
            this.$state.go('home'); 
        }
    }
}
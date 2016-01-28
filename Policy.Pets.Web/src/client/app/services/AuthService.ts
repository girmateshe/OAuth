module App.Services {
    'use strict';

    export interface IAuthRequest extends ng.resource.IResource<IAuthRequest> {
        username: string;
        password: string;
        grant_type: string;
    }

    export interface IJwtToken{
        access_token: string;
        expires_in: number;
        expires_on: number;
        refresh_token: string;
    }

    export interface IAuthRequestResource extends ng.resource.IResourceClass<IAuthRequest> {
        generate();
        validate();
        decode();
    }

    export class AuthService {
        public static factory($resource: ng.resource.IResourceService): IAuthRequestResource {

            var baseUrl = "http://api.petsinsurance.com/oauth/v1/token";

            return <IAuthRequestResource> $resource(baseUrl , {id: '@id'}, {
                generate: { method: 'POST' },
                validate: { method: 'GET' },
                decode: { method: 'GET', url: baseUrl + '/validate' },
            });
        }
    }

} 
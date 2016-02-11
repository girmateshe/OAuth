/// <reference path="../_all.ts" />
/// <reference path="../services/policiesservice.ts" />

module App.Controllers {
    'use strict';

    //Interface to describe the scope
    export interface IPetPolicyScope extends ng.IScope {
        policies: Array<rs.IPetPolicy>;
        policy: rs.IPetPolicy;
        countries: Array<rs.ICountry>;
        vm: PoliciesController;
        table: any;
    }

    export class PoliciesController {

        public init(): void {
            this.$scope.countries = this.country.query();
            console.log(this.$scope.countries);

            this.$scope.table = {
                name: "Registrations",
                summary: [
                    {
                        name: "Total Registrations by TLD",
                        value: [
                            { name: ".COM registrations", value: 23 },
                            { name: ".NGO registrations", value: 23 },
                            { name: ".ONG registrations", value: 23 }
                        ]
                    },
                    {
                        name: "Total New Registrations:",
                        value: 49
                    },
                    {
                        name: "Average Registration Period:",
                        value: 1.22
                    }
                ],
                fields: [
                            { name: "Order ID", title: "A nicer human readable label or title for the field" },
                            { name: "Domain", title: "A nicer human readable label or title for the field" },
                            { name: "Creation Date", title: "A nicer human readable label or title for the field" },
                            { name: "Exp Date", title: "A nicer human readable label or title for the field" },
                            { name: "Years", title: "A nicer human readable label or title for the field" },
                            { name: "Paid Amount", title: "A nicer human readable label or title for the field" },
                            { name: "Tax Amount", title: "A nicer human readable label or title for the field" },
                            { name: "Billed to Customer", title: "A nicer human readable label or title for the field" }
                ],
                rows: [
                    [161939331, '3oniedcrypto3oiterol.ngo', '1/4/2016', '1/5/2017', 1, '$0.00', '$0.00', '$0.00'],
                    [161939331, '3oniedcrypto3oiterol.ngo', '1/4/2016', '1/5/2017', 1, '$0.00', '$0.00', '$0.00'],
                    [161939331, '3oniedcrypto3oiterol.ngo', '1/4/2016', '1/5/2017', 1, null, '$0.00', '$0.00'],
                    [161939331, '3oniedcrypto3oiterol.ngo', '1/4/2016', '1/5/2017', 1, '$0.00', null, '$0.00'],
                    [161939331, '3oniedcrypto3oiterol.ngo', '1/4/2016', '1/5/2017', 1, '$0.00', '$0.00', '$0.00'],
                    [161939331, '3oniedcrypto3oiterol.ngo', '1/4/2016', '1/5/2017', 1, '$0.00', '$0.00', '$0.00']
                ],
                pagination: {
                    currentPage: 1,
                    totalPages: 7,
                    pageSize: 50
                },
                sort: {
                    by: 'domain',
                    order: 'asc'
                }
            };

            console.log(this.$scope.table);
        }

        public static $inject = [
            '$scope', '$state', '$stateParams', 'policyService', 'countryService'
        ];

        constructor(private $scope: IPetPolicyScope,
                    private $state,
                    private $stateParams,
                    public policy: rs.IPetPolicyResource,
                    public country: rs.ICountryResource) {
            this.$scope.vm = this;
            this.init();
            if ($stateParams.id) {
                $scope.policy = policy.get({id: $stateParams.id});
            } else {
                $scope.policy = new policy();
                $scope.policies = policy.query();
            }
        }

        public addPolicy(): void {
            console.log(this.$scope.policy);
            this.$scope.policy.$save(() => {
                this.$state.go('policies'); // on success go back to home i.e. policies state.
            });
        }

        public showPopup(message: string): boolean {
            console.log(message);
            return true;
        }

        public cancelPolicy(policy: rs.IPetPolicy): void {
            if (this.showPopup('Really delete this?')) {
                policy.$delete({id: policy.id}, () => {
                    this.$state.go('policies'); // on success go back to home i.e. policies state.
                });
            }
        }

        /*
        public updatePolicy(): void {
            console.log(this.$scope.policy);
            this.$scope.policy.$update(() => {
                this.$state.go('policies'); // on success go back to home i.e. policies state.
            });
        }
        */
    }
}
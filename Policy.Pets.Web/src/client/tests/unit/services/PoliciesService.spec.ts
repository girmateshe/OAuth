describe("PetPolicyService", () => {

    var $httpBackend: ng.IHttpBackendService;
    var policyService: App.Services.IPetPolicyResource;

    var baseUrl: string = "http://api.petsinsurance.com/api/v1/policies";
    var policiesUrl = baseUrl + '';

    var expect = chai.expect;

    beforeEach(() => {
        bard.appModule("policyApp");
        bard.inject(function (_$httpBackend_, _policyService_) {
            $httpBackend = _$httpBackend_;
            policyService = _policyService_;
        });
    });

    afterEach(() => {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    
    it("should initialize correctly", () => {
        expect(policyService).to.be.not.undefined;
    });
    
    it("should load channels", () => {

        $httpBackend.expectGET(policiesUrl).respond([
            {
                id: 0,
                name: "Test",
                policyNumber: null,
                policyDate: "0001-01-01T00:00:00",
                countryIsoCode: "ETH",
                email: "test@gmail.com",
                pets: null
            },
            {
                id: 0,
                name: "Test",
                policyNumber: null,
                policyDate: "0001-01-01T00:00:00",
                countryIsoCode: "ETH",
                email: "test@gmail.com",
                pets: null
            }
        ]);

            var policies = policyService.query(() => {
                
                expect(policies).to.be.not.undefined;
                expect(policies).not.to.be.equal(null);
                expect(policies.length).to.be.equal(2);

                var policy = policies[0];
                expect(policy.id).to.be.equal(0);
                expect(policy.name).to.be.equal("Test");
                expect(policy.policyNumber).to.be.equal(null);
                expect(policy.policyDate).to.be.equal("0001-01-01T00:00:00");
                expect(policy.countryIsoCode).to.be.equal("ETH");
                expect(policy.email).to.be.equal("test@gmail.com");
                expect(policy.pets).to.be.equal(null);
            });
            
        $httpBackend.flush();
    }); 
});
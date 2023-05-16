using AutoFixture;
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using RestAssured.Request.Logging;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class UnHappyPathRegistrationWithoutDocument
{
    private static readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private static readonly string _endPoint = "/api/registration";
    private static readonly string _userCompanyToken;
    private static string _applicationId;

    private readonly IFixture _fixture;
    
    private readonly string _adminEndPoint = "/api/administration";
    private readonly string _operatorToken;
    private readonly string _userToken;
    private static string _companyName = "Test-Catena-X";
    private readonly RegistrationEndpointHelper _registrationEndpointHelper = new RegistrationEndpointHelper(_userCompanyToken, _baseUrl, _endPoint);
    
    public UnHappyPathRegistrationWithoutDocument()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        //_userCompanyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        //_companyToken = "TestUserToken";
        _operatorToken = configuration.GetValue<string>("Secrets:OperatorToken");
        _userToken = configuration.GetValue<string>("Secrets:UserToken");
        _fixture = new Fixture();
    }

    #region UnHappy Path - new registration without document
    
    [Fact]
    public void Test0_GetBpn_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"https://partners-pool.dev.demo.catena-x.net/api/catena/legal-entities?status=ACTIVE&page=0&size=20")
            .Then()
            .StatusCode(200)
            .Body("");
    }

    // POST api/administration/invitation
    
    /*[Fact]
    public void Test1_ExecuteInvitation_ReturnsExpectedResult()
    {
        DevMailApiRequests devMailApiRequests = new DevMailApiRequests();
        var devUser = devMailApiRequests.GenerateRandomEmailAddress();
        var emailAddress = devUser.Result.Name + "@developermail.com";
        CompanyInvitationData invitationData = new CompanyInvitationData("testuser", "myFirstName", "myLastName",
            emailAddress, "Test-Catena-X-10");
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .ContentType("application/json")
            .Body(invitationData)
            .When()
            .Post($"{_baseUrl}{_adminEndPoint}/invitation")
            .Then()
            .StatusCode(200);
        //Thread.Sleep(2000);
        var messageData = devMailApiRequests.FetchPassword();
        //AuthenticationFlow();
    }*/


    // GET /api/registration/legalEntityAddress/{bpn}

    [Fact]
    public void Test2_GetCompanyBpdmDetailData_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/legalEntityAddress/")
            .Then()
            .StatusCode(200)
            .Body("");
    }

    [Fact]
    public void Test3_SetCompanyDetailData_ReturnsExpectedResult()
    {
        CompanyDetailData companyDetailData = _registrationEndpointHelper.GetCompanyDetailData();
        _companyName = companyDetailData.Name;
        string companyId = companyDetailData.CompanyId.ToString();
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Body("{\"companyId\":" + "\"" + companyId + "\"" +
                  ",\"name\":" + "\"" + _companyName + "\"" + ",\"city\":\"München\",\"streetName\":\"Street\",\"countryAlpha2Code\":\"DE\",\"bpn\":null, \"shortName\":" + "\"" + _companyName + "\"" + ",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{\"type\":\"VAT_ID\",\"value\":\"DE123456788\"}]}")
            //.Body(companyDetailData)
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    [Fact]
    public void Test4_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
    {
        //_applicationId = GetFirstApplicationId();
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .Body(
                "{\"companyRoles\": [\"ACTIVE_PARTICIPANT\", \"APP_PROVIDER\", \"SERVICE_PROVIDER\"], \"agreements\": [{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1011\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1010\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1090\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1013\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1017\",\"consentStatus\":\"ACTIVE\"}]}")
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyRoleAgreementConsents")
            .Then()
            .StatusCode(200);
    }


    // POST /api/registration/application/{applicationId}/submitRegistration
    // => expecting an error because of missing documents
    
    [Fact]
    public void Test5_SubmitRegistration_ReturnsExpectedResult()
    {
        //_applicationId = GetFirstApplicationId();
        //var applicationStatus = GetApplicationStatus();
        var status = Given()
            .RelaxedHttpsValidation()
            .Log(RequestLogLevel.All)
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Post(
                $"{_baseUrl}{_endPoint}/application/{_applicationId}/submitRegistration")
            .Then()
            .StatusCode(403)
            .Body(NHamcrest.Contains.String("Application status is not fitting to the pre-requisite"));
            //.Body("$.errors.[Org.Eclipse.TractusX.Portal.Backend.Registration.Service][0]");
            // Assert.Equal("Application status is not fitting to the pre-requisite", status.ToString());
    }

    #endregion
}
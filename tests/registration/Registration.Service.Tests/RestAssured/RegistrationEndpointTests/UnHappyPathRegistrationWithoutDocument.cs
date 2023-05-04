using AutoFixture;
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.RestAssured;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class UnHappyPathRegistrationWithoutDocument
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private readonly string _companyToken;
    private static string _applicationId;

    private readonly IFixture _fixture;
    // private readonly string pdfFileName = @"TestDocument.pdf";
    
    private readonly string _adminEndPoint = "/api/administration";
    private readonly string _operatorToken;
    private readonly string _userToken;
    private static string _companyName = "Test-Catena-X";
    
    public UnHappyPathRegistrationWithoutDocument()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        _operatorToken = configuration.GetValue<string>("Secrets:OperatorToken");
        _userToken = configuration.GetValue<string>("Secrets:UserToken");
        _applicationId = new (configuration.GetValue<string>("Secrets:ApplicationId"));
        _fixture = new Fixture();
        // CreateFilesToUpload();
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
                $"Bearer {_companyToken}")
            .When()
            .Get($"https://partners-pool.dev.demo.catena-x.net/api/catena/legal-entities?status=ACTIVE&page=0&size=20")
            .Then()
            .StatusCode(200)
            .Body("");
    }

    // POST api/administration/invitation
    
    // [Fact]
    // public void Test1_ExecuteInvitation_ReturnsExpectedResult()
    // {
    //     CompanyInvitationData invitationData = new CompanyInvitationData("user", "myFirstName", "myLastName",
    //         "myEmail", "Test-Catena-X");
    //      Given()
    //         .RelaxedHttpsValidation()
    //         .Header(
    //             "authorization",
    //             $"Bearer {_operatorToken}")
    //         .ContentType("application/json")
    //         .Body(invitationData)
    //         .When()
    //         .Post($"{_baseUrl}{_adminEndPoint}/invitation")
    //         .Then()
    //         .StatusCode(200);
    // }


    // GET /api/registration/legalEntityAddress/{bpn}

    [Fact]
    public void Test2_GetCompanyBpdmDetailData_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/legalEntityAddress/")
            .Then()
            .StatusCode(200)
            .Body("");
    }

    [Fact]
    public void Test3_SetCompanyDetailData_ReturnsExpectedResult()
    {
        CompanyDetailData companyDetailData = _fixture.Create<CompanyDetailData>();

        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .Body(
                "{\"companyId\":\"d6ba04c9-edd0-47b5-a639-c6b5256b1aec\",\"name\":\"TestAutomationReg 01\",\"city\":\"München\",\"streetName\":\"Streetfgh\",\"countryAlpha2Code\":\"DE\",\"bpn\":null,\"shortName\":\"TestAutomationReg 01\",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{\"type\":\"VAT_ID\",\"value\":\"DE123456789\"}]}")
            // .Body(
            //     "{\"companyId\":\"f42b94b5-6003-43dc-a14a-6b88ed7b1e8a\",\"name\":\"TestAutomationReg 02\",\"city\":\"München\",\"streetName\":\"Streetfgh\",\"countryAlpha2Code\":\"DE\",\"bpn\":null,\"shortName\":\"TestAutomationReg 02\",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{\"type\":\"VAT_ID\",\"value\":\"DE123456789\"}]}")
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    [Fact]
    public void Test4_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
    {
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
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

    #endregion
}
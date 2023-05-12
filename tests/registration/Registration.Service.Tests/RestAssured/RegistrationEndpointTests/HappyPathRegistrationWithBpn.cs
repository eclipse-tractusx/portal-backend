using AutoFixture;
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithBpn
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private readonly string _companyToken;
    private static string _applicationId;

    private readonly IFixture _fixture;
    // private readonly string pdfFileName = @"TestDocument.pdf";

    private readonly string _adminEndPoint = "/api/administration";
    private readonly string _operatorToken;
    private static string _companyName = "Test-Catena-X";
    private static string _bpn = "1234";

    public RegistrationEndpointTestsHappyPathRegistrationWithBpn()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        _applicationId = new (configuration.GetValue<string>("Secrets:ApplicationId"));
        _operatorToken = configuration.GetValue<string>("Secrets:OperatorToken");
        _fixture = new Fixture();
        // CreateFilesToUpload();
    }


    #region Happy Path - new registration with BPN

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
            .Get($"https://partners-pool.dev.demo.catena-x.net/api/catena/legal-entities?page=0&size=20")
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

    //GET /api/registration/legalEntityAddress/{bpn}

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
            .Get($"{_baseUrl}{_endPoint}/legalEntityAddress/{_bpn}")
            .Then()
            .StatusCode(200)
            .Body("");
    }

    //POST /api/registration/application/{applicationId}/companyDetailsWithAddress

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
                $"{{\"companyId\":\"d6ba04c9-edd0-47b5-a639-c6b5256b1aec\",\"name\":\"TestAutomationReg 01\",\"city\":\"München\",\"streetName\":\"Streetfgh\",\"countryAlpha2Code\":\"DE\",\"bpn\":{_bpn} \"shortName\":\"TestAutomationReg 01\",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{{\"type\":\"VAT_ID\",\"value\":\"DE123456788\"}}]}}")
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

    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents

    [Fact]
    public void Test5_UploadDocument_WithEmptyTitle_ReturnsExpectedResult()
    {
        _applicationId = GetFirstApplicationId();
        string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
        File.WriteAllText("testfile.pdf", "Some Text");
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("multipart/form-data")
            .MultiPart(new FileInfo("testfile.pdf"), "document")
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
            .Then()
            .Body("1")
            .StatusCode(200);
    }

    //POST /api/registration/application/{applicationId}/submitRegistration
    
    [Fact]
    public void Test6_SubmitRegistration_ReturnsExpectedResult()
    {
        var status = (bool)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .Body("")
            .When()
            .Post(
                $"{_baseUrl}{_endPoint}/application/{_applicationId}/submitRegistration")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(bool));
        Assert.True(status);
    }

    // GET: api/administration/registration/applications?companyName={companyName}

    [Fact]
    public void Test7_GetApplicationDetails_ReturnsExpectedResult()
    {
        //_companyName = "Test-Catena-X-3";
        // Given
        //var data = (Pagination.Response<CompanyApplicationDetails>)
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get(
                $"{_baseUrl}{_adminEndPoint}/registration/applications?companyName={_companyName}&page=0&size=4&companyApplicationStatus=Closed")
            .Then()
            .StatusCode(200)
            .Extract()
            .Body("$.content[0].applicationStatus");
        Assert.Equal("SUBMITTED", data);
        //.As(typeof(Pagination.Response<CompanyApplicationDetails>));
        //Assert.Contains(_companyName, data.Content.Select(content => content.CompanyName.ToString()));
    }

    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test8_GetCompanyWithAddress_ReturnsExpectedResult()
    {
        // Given
        var data = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{_baseUrl}{_adminEndPoint}/registration/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(CompanyDetailData));
        Assert.NotNull(data);
    }

    #endregion

    private string GetFirstApplicationId()
    {
        var applicationIDs = (List<CompanyApplicationData>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<CompanyApplicationData>));

        return applicationIDs[0].ApplicationId.ToString();
    }
}
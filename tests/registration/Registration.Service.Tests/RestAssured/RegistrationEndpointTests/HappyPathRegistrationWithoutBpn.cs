using System.Json;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Xunit;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.RestAssured;

[TestCaseOrderer("Notifications.Service.Tests.RestAssured.AlphabeticalOrderer",
    "Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests")]
public class RegistrationEndpointTestsHappyPathRegistrationWithoutBpn
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
    
    public RegistrationEndpointTestsHappyPathRegistrationWithoutBpn()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        _operatorToken = configuration.GetValue<string>("Secrets:OperatorToken");
        _fixture = new Fixture();
        // CreateFilesToUpload();
    }

    #region Happy Path - new registration without BPN

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
    
    private CompanyDetailData GetCompanyDetailData()
    {
        _applicationId = GetFirstApplicationId();
        // Given
        CompanyDetailData companyDetailData = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(CompanyDetailData));

        return companyDetailData;
    }

    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void Test2_SetCompanyDetailData_ReturnsExpectedResult()
    {
        CompanyDetailData companyDetailData = GetCompanyDetailData();
        string companyId = companyDetailData.CompanyId.ToString();
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .When()
            .Body("{\"companyId\":" + "\"" + companyId + "\"" + ",\"name\":\"TestAutomationReg 01\",\"city\":\"München\",\"streetName\":\"Streetfgh\",\"countryAlpha2Code\":\"DE\",\"bpn\":null, \"shortName\":\"TestAutomationReg 01\",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{\"type\":\"VAT_ID\",\"value\":\"DE123456788\"}]}")
            //.Body(companyDetailData)
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
    }
    
    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents

    [Fact]
    public void Test3_SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
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

    // [Fact]
    // public void Test4_UploadDocument_WithEmptyTitle_ReturnsExpectedResult()
    // {
    //     //this.CreateStubForPdfMultiPartFormData();
    //
    //     string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
    //     var file = FormFileHelper.GetFormFile("this is just a test", "superFile.pdf", "application/pdf");
    //     var pdfFileName = File.Create("testfile.pdf");
    //     Given()
    //         .RelaxedHttpsValidation()
    //         .Header(
    //             "authorization",
    //             $"Bearer {_companyToken}")
    //         .ContentType("application/pdf")
    //         .MultiPart(new FileInfo(file.FileName), file.FileName, new MediaTypeHeaderValue("application/pdf"))
    //         .When()
    //         .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
    //         .Then()
    //         .StatusCode(200);
    // }


    // POST /api/registration/application/{applicationId}/submitRegistration
    // GET: api/administration/registration/applications?companyName={companyName}
    
    [Fact]
    public void Test6_GetApplicationDetails_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{_baseUrl}{_adminEndPoint}/registration/applications?companyName={_companyName}/?page=0&size=10&sorting=DateDesc")
            .Then()
            .StatusCode(200);
    }
    
    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    [Fact]
    public void Test7_GetCompanyWithAddress_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{_baseUrl}{_adminEndPoint}/registration/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
    }
    
    #endregion
}
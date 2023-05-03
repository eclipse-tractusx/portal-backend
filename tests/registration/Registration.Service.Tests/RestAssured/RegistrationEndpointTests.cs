using System.Net.Http.Headers;
using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using MimeKit;
using Npgsql.Internal.TypeHandlers;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.RestAssured;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.Extensions;
using Xunit;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.RestAssured;

public class RegistrationEndpointTests
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private readonly string _companyToken;
    private static string _applicationId;
    private readonly IFixture _fixture;
    private readonly string pdfFileName = @"TestDocument.pdf";
    
    private readonly string _adminEndPoint = "/api/administration";
    private readonly string _operatorToken;
    private static string _companyName = "Test-Catena-X";
    
    public RegistrationEndpointTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        _applicationId = new (configuration.GetValue<string>("Secrets:ApplicationId"));
        _operatorToken = configuration.GetValue<string>("Secrets:OperatorToken");
        _fixture = new Fixture();
    }
    
    
    #region Happy Path - new registration with BPN
    
    //POST api/administration/invitation
    //GET /api/registration/legalEntityAddress/{bpn}
    //POST /api/registration/application/{applicationId}/companyDetailsWithAddress
    
    //POST /api/registration/application/{applicationId}/companyRoleAgreementConsents
    
    //POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents
    //POST /api/registration/application/{applicationId}/submitRegistration
    //GET: api/administration/registration/applications?companyName={companyName}
    //GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    

    #endregion

    #region Happy Path - new registration without BPN

    // POST api/administration/invitation
    
    [Fact]
    public void ExecuteInvitation_ReturnsExpectedResult()
    {
        CompanyInvitationData invitationData = new CompanyInvitationData("user", "myFirstName", "myLastName",
            "irina.meshcheryakova@office365.tngtech.com", "Test-Catena-X");
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
    }
    
    
    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void SetCompanyDetailData_ReturnsExpectedResult()
    {
        CompanyDetailData companyDetailData = _fixture.Create<CompanyDetailData>();
        
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .Body("{\"companyId\":\"f42b94b5-6003-43dc-a14a-6b88ed7b1e8a\",\"name\":\"TestAutomationReg 02\",\"city\":\"München\",\"streetName\":\"Streetfgh\",\"countryAlpha2Code\":\"DE\",\"bpn\":null,\"shortName\":\"TestAutomationReg 02\",\"region\":null,\"streetAdditional\":null,\"streetNumber\":null,\"zipCode\":null,\"countryDe\":\"Deutschland\",\"uniqueIds\":[{\"type\":\"VAT_ID\",\"value\":\"DE123456789\"}]}")
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200);
    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents
    
    [Fact]
    public void SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
    {
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .Body("{\"companyRoles\": [\"ACTIVE_PARTICIPANT\", \"APP_PROVIDER\", \"SERVICE_PROVIDER\"], \"agreements\": [{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1011\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1010\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1090\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1013\",\"consentStatus\":\"ACTIVE\"},{\"agreementId\":\"aa0a0000-7fbc-1f2f-817f-bce0502c1017\",\"consentStatus\":\"ACTIVE\"}]}")
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyRoleAgreementConsents")
            .Then()
            .StatusCode(200);
    }
    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents
    
    [Fact]
    public void UploadDocument_WithEmptyTitle_ReturnsExpectedResult()
    {
        //this.CreateStubForPdfMultiPartFormData();
        
        string documentTypeId = "COMMERCIAL_REGISTER_EXTRACT";
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("multipart/form-data")
            .MultiPart(new FileInfo("TestDocument.pdf"), "TestDocument.pdf", new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf"))
            .When()
            .Post($"{_baseUrl}{_endPoint}/application/{_applicationId}/documentType/{documentTypeId}/documents")
            .Then()
            .StatusCode(200);
    }
    
    /*// <summary>
    /// Creates the stub response for the csv form data example.
    /// </summary>
    private void CreateStubForPdfMultiPartFormData()
    {
        this.Server?.Given(Request.Create().WithPath("/csv-multipart-form-data").UsingPost()
                .WithHeader("Content-Type", new RegexMatcher("multipart/form-data; boundary=.*"))
                .WithBody(new RegexMatcher($".*text/csv.*"))
                .WithBody(new RegexMatcher($".*name=customControl.*")))
            .RespondWith(Pagination.Response<>.Create()
                .WithStatusCode(201));
    }*/
    
    /*
    /// <summary>
    /// Creates the stub response for the csv form data example.
    /// </summary>
    private void CreateStubForCsvMultiPartFormData()
    {
        this.Server?.Given(Request.Create().WithPath("/csv-multipart-form-data").UsingPost()
                .WithHeader("Content-Type", new RegexMatcher("multipart/form-data; boundary=.*"))
                .WithBody(new RegexMatcher($".*text/csv.*"))
                .WithBody(new RegexMatcher($".*name=customControl.*")))
            .RespondWith(Response.Create()
                .WithStatusCode(201));
    }
    */
   
    
    // POST /api/registration/application/{applicationId}/submitRegistration
    // GET: api/administration/registration/applications?companyName={companyName}
    
    [Fact]
    public void GetApplicationDetails_ReturnsExpectedResult()
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
    public void GetCompanyWithAddress_ReturnsExpectedResult()
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
    
    [Fact]
    public void GetCompanyDetailData_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(200)
            .Body("");
    }
    
    #region UnHappy Path - new registration without document

    // POST api/administration/invitation
    // GET /api/registration/legalEntityAddress/{bpn}
    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress
    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents
    // POST /api/registration/application/{applicationId}/submitRegistration
    // => expecting an error because of missing documents
    

    #endregion
    
    [Fact]
    public void GetApplicationStatus_ReturnsExpectedResult()
    {
        // Given
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/status")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(CompanyApplicationStatusId));
    }
    
    [Fact]
    public void GetAgreementConsentStatuses_ReturnsExpectedResult()
    {
        // Given
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyRoleAgreementConsents")
            .Then()
            .StatusCode(200)
            //.Extract()
            //.As(typeof(CompanyRoleAgreementConsents))
            .Body("");
    }
    
    [Fact]
    public void GetInvitedUsers_ReturnsExpectedResult()
    {
        // Given
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/invitedusers")
            .Then()
            .StatusCode(200);
    }

    #region DB / API Test

    [Fact]
    public void GetCompanyRoles_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/company/companyRoles")
            .Then()
            .StatusCode(200)
            .Body("");
    }    
    
    
    [Fact]
    public void GetCompanyRoleAgreementData_ReturnsExpectedResult()
    {
        // Given
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/companyRoleAgreementData")
            .Then()
            .StatusCode(200)
            .Body("");
    }
    
    [Fact]
    public void GetClientRolesComposite_ReturnsExpectedResult()
    {
        // Given
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/rolesComposite")
            .Then()
            .Body("")
            .StatusCode(200)
            .Extract()
            .As(typeof(string[]));
    }
    
    [Fact]
    public void GetApplicationsWithStatus_ReturnsExpectedResult()
    {
        // Given
        var data = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200);
    }
    #endregion
}
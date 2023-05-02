using AutoFixture;
using Microsoft.Extensions.Configuration;
using Npgsql.Internal.TypeHandlers;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.RestAssured;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
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
    public RegistrationEndpointTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        _applicationId = new (configuration.GetValue<string>("Secrets:ApplicationId"));
        _fixture = new Fixture();
    }

    #region Happy Path - new registration with BPN
    
    //POST api/administration/invitation
    //GET /api/registration/legalEntityAddress/{bpn}
    //POST /api/registration/application/{applicationId}/companyDetailsWithAddress
    
    //POST /api/registration/application/{applicationId}/companyRoleAgreementConsents
    
    [Fact]
    public void SubmitCompanyRoleConsentToAgreements_ReturnsExpectedResult()
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
            .Extract()
            .As(typeof(int));
        Assert.Equal(data, '0');
       
    }
    //POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents
    //POST /api/registration/application/{applicationId}/submitRegistration
    //GET: api/administration/registration/applications?companyName={companyName}
    //GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    

    #endregion

    #region Happy Path - new registration without BPN

    // POST api/administration/invitation
    // POST /api/registration/application/{applicationId}/companyDetailsWithAddress

    [Fact]
    public void SetCompanyDetailData_ReturnsExpectedResult()
    {
        /*var companyDetailData = new
            CompanyDetailData(UuidHandler, "filled", "filled", "filled", "filled", "BPN");
        
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_companyToken}")
            .ContentType("application/json")
            .Body(
                "{\"content\": \"test\",\"notificationTypeId\": \"INFO\",\"isRead\": false}")
            // .Body(creationData)
            .When()
            .Post($"{_baseUrl}{_endPoint}/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .StatusCode(201)
            .Extract()
            .As(typeof(string));*/
        

    }

    // POST /api/registration/application/{applicationId}/companyRoleAgreementConsents
    // POST /api/registration/application/{applicationId}/documentType/{documentTypeId}/documents
    // POST /api/registration/application/{applicationId}/submitRegistration
    // GET: api/administration/registration/applications?companyName={companyName}
    
    
    // GET: api/administration/registration/application/{applicationId}/companyDetailsWithAddress
    
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
            .StatusCode(200);
    }

    //var applicationId = _fixture.Create<DocumentTypeData>();
    #endregion

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
            .Extract()
            .As(typeof(CompanyRoleAgreementConsents));
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
            .StatusCode(200);
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
            .StatusCode(200);
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
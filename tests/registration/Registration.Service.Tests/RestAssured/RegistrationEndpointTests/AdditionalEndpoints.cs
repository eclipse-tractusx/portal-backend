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
using Xunit;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Tests.RestAssured;

public class AdditionalEndpoints
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private readonly string _companyToken;
    private static string _applicationId;
    
    public AdditionalEndpoints()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Secrets>()
            .Build();
        _companyToken = configuration.GetValue<string>("Secrets:CompanyToken");
        _applicationId = new (configuration.GetValue<string>("Secrets:ApplicationId"));
    }

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
            .Body("")
            .StatusCode(200);
    }

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
            // .Body("")
            .StatusCode(200);
        //.Extract()
        //.As(typeof(CompanyRoleAgreementConsents))
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
}
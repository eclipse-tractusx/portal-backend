using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

public class RegistrationEndpointHelper
{
    private readonly string _baseUrl = "https://portal-backend.dev.demo.catena-x.net";
    private readonly string _endPoint = "/api/registration";
    private readonly string _adminEndPoint = "/api/administration";
    private readonly string _userCompanyToken;
    private readonly string _operatorToken;
    private static string? _applicationId;

    public RegistrationEndpointHelper(string userCompanyToken, string operatorToken)
    {
        _userCompanyToken = userCompanyToken;
        _operatorToken = operatorToken;
        //_applicationId = GetFirstApplicationId();
    }

    public string GetFirstApplicationId()
    {
        var applicationIDs = (List<CompanyApplicationData>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/applications")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<CompanyApplicationData>));

        _applicationId = applicationIDs[0].ApplicationId.ToString();

        return _applicationId;
    }

    public CompanyDetailData GetCompanyDetailData()
    {
        // Given
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .And()
            .StatusCode(200)
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        var companyDetailData = DeserializeData<CompanyDetailData>(data);
        if (companyDetailData is null)
        {
            throw new Exception("Company detail data was not found.");
        }

        return companyDetailData;
    }

    public List<CompanyRoleConsentViewData> GetCompanyRolesAndConsents()
    {
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .ContentType("application/json")
            .When()
            .Get(
                $"{_baseUrl}{_adminEndPoint}/companydata/companyRolesAndConsents")
            .Then()
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        var companyRolesAndConsents = DeserializeData<List<CompanyRoleConsentViewData>>(data);
        if (companyRolesAndConsents is null)
        {
            throw new Exception("Company roles and consents were not found.");
        }

        return companyRolesAndConsents;
    }

    public CompanyRoleAgreementConsents GetCompanyRolesAndConsentsForSelectedRoles(List<CompanyRoleId> companyRoleIds)
    {
        List<CompanyRoleConsentViewData> availableRolesAndConsents = GetCompanyRolesAndConsents();
        List<CompanyRoleId> selectedCompanyRoleIds = new List<CompanyRoleId>();
        List<AgreementConsentStatus> agreementConsentStatusList = new List<AgreementConsentStatus>();
        foreach (var role in availableRolesAndConsents)
        {
            if (role.CompanyRolesActive && companyRoleIds.Contains(role.CompanyRoleId))
            {
                selectedCompanyRoleIds.Add(role.CompanyRoleId);
                foreach (var agreementId in role.Agreements)
                {
                    AgreementConsentStatus agreementConsentStatus =
                        new AgreementConsentStatus(agreementId.AgreementId, ConsentStatusId.ACTIVE);
                    agreementConsentStatusList.Add(agreementConsentStatus);
                }
            }
        }
        return new CompanyRoleAgreementConsents(selectedCompanyRoleIds, agreementConsentStatusList);
    }

    public void SetApplicationStatus(string applicationStatus)
    {
        var status = (int)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Put(
                $"{_baseUrl}{_endPoint}/application/{_applicationId}/status?status={applicationStatus}")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(int));
        Assert.Equal(1, status);
    }

    public string GetApplicationStatus()
    {
        var applicationStatus = (string)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .ContentType("application/json")
            .When()
            .Get(
                $"{_baseUrl}{_endPoint}/application/{_applicationId}/status")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(string));
        return applicationStatus;
    }

    public List<InvitedUser> GetInvitedUsers()
    {
        var invitedUsers = (List<InvitedUser>)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/invitedusers")
            .Then()
            .StatusCode(200)
            .Extract()
            .As(typeof(List<InvitedUser>));

        return invitedUsers;
    }

    private T? DeserializeData<T>(string jsonString)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, options);
        return deserializedData;
    }
}
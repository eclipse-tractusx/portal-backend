using Org.Eclipse.TractusX.Portal.Backend.Registration.Service.Model;
using Xunit;
using static RestAssured.Dsl;

namespace Registration.Service.Tests.RestAssured.RegistrationEndpointTests;

public class RegistrationEndpointHelper
{
    private readonly string _baseUrl;
    private readonly string _endPoint;
    private readonly string _userCompanyToken;
    private static string? _applicationId;
    
    public RegistrationEndpointHelper(string userCompanyToken, string baseUrl, string endPoint)
    {
        _userCompanyToken = userCompanyToken;
        _baseUrl = baseUrl;
        _endPoint = endPoint;
        //_applicationId = GetFirstApplicationId();
    }
    public string? GetFirstApplicationId()
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
        CompanyDetailData companyDetailData = (CompanyDetailData)Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_userCompanyToken}")
            .When()
            .Get($"{_baseUrl}{_endPoint}/application/{_applicationId}/companyDetailsWithAddress")
            .Then()
            .And()
            .StatusCode(200)
            .Extract().As(typeof(CompanyDetailData));

        return companyDetailData;
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
}
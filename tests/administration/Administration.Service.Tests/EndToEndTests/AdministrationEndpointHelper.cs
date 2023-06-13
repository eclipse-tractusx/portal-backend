using System.Text.Json;
using System.Text.Json.Serialization;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared;
using Tests.Shared.EndToEndTests;
using Tests.Shared.RestAssured.AuthFlow;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.RestAssured;

public static class AdministrationEndpointHelper
{
    private static readonly string BaseUrl = TestResources.BaseUrl;
    private static readonly string EndPoint = "/api/administration";
    private static readonly Secrets Secrets = new();
    private static string? _operatorToken;
    private static readonly string OperatorCompanyName = TestResources.OperatorCompanyName;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task GetOperatorToken()
    {
        _operatorToken = await new AuthFlow(OperatorCompanyName).GetAccessToken(Secrets.OperatorUserName, Secrets.OperatorUserPassword);
    }

    //GET: api/administration/serviceaccount/owncompany/serviceaccounts
    public static List<CompanyServiceAccountData>? GetServiceAccounts()
    {
        var serviceAccounts = new List<CompanyServiceAccountData>();

        var totalPagesStr = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts?size=15")
            .Then()
            .StatusCode(200)
            .Extract()
            .Body("$.meta.totalPages").ToString();

        var failure = int.TryParse(totalPagesStr, out var totalPages);

        if (!failure) return null;
        for (var i = 0; i < totalPages; i++)
        {
            var response = Given()
                .RelaxedHttpsValidation()
                .Header(
                    "authorization",
                    $"Bearer {_operatorToken}")
                .When()
                .Get($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts?page={i}&size=15")
                .Then()
                .StatusCode(200)
                .Extract()
                .Response();
            var data = DeserializeData<Pagination.Response<CompanyServiceAccountData>>(response.Content
                .ReadAsStringAsync()
                .Result);
            serviceAccounts.AddRange(data.Content);
        }

        return serviceAccounts;
    }

    //POST: api/administration/serviceaccount/owncompany/serviceaccounts - create a new service account
    public static ServiceAccountDetails? CreateNewServiceAccount(string[] permissions, string techUserName, string description)
    {
        var allServiceAccountsRoles = GetAllServiceAccountRoles();
        var userRoleIds = new List<Guid>();

        foreach (var p in permissions)
        {
            userRoleIds.AddRange(
                from t in allServiceAccountsRoles where t.UserRoleText.Contains(p) select t.UserRoleId);
        }

        var serviceAccountCreationInfo =
            new ServiceAccountCreationInfo(techUserName, description, IamClientAuthMethod.SECRET, userRoleIds);
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Body(JsonSerializer.Serialize(serviceAccountCreationInfo, JsonSerializerOptions))
            .Post($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts")
            .Then()
            .StatusCode(201)
            .Extract()
            .Response();

        return DeserializeData<ServiceAccountDetails>(response.Content.ReadAsStringAsync().Result);
    }

    //DELETE: api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}
    public static void DeleteServiceAccount(string serviceAccountId)
    {
        Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Delete($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}")
            .Then()
            .StatusCode(200);
    }


    //PUT: api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId} - update the previous created service account details by changing "name" and "description"
    public static ServiceAccountDetails? UpdateServiceAccountDetailsById(string serviceAccountId, string newName,
        string newDescription)
    {
        //var serviceAccountId = "ba021ef6-3cf8-45d1-8923-e9ee19cb5877";
        var serviceAccountDetails = GetServiceAccountDetailsById(serviceAccountId);
        var updateServiceAccountEditableDetails =
            new ServiceAccountEditableDetails(serviceAccountDetails.ServiceAccountId, newName, newDescription,
                serviceAccountDetails.IamClientAuthMethod);

        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Body(JsonSerializer.Serialize(updateServiceAccountEditableDetails, JsonSerializerOptions))
            .Put($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        return DeserializeData<ServiceAccountDetails>(response.Content.ReadAsStringAsync()
            .Result);
    }

    //POST: api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}/resetCredentials - reset service account credentials
    public static ServiceAccountDetails? ResetServiceAccountCredentialsById(string serviceAccountId)
    {
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Post($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}/resetCredentials")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();

        return DeserializeData<ServiceAccountDetails>(response.Content.ReadAsStringAsync().Result);
    }

    //GET: api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}
    public static ServiceAccountDetails? GetServiceAccountDetailsById(string serviceAccountId)
    {
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        return DeserializeData<ServiceAccountDetails>(response.Content.ReadAsStringAsync()
            .Result);
    }
    
    //GET: api/administration/serviceaccount/user/roles
    private static List<UserRoleWithDescription>? GetAllServiceAccountRoles()
    {
        var response = Given()
            .RelaxedHttpsValidation()
            .Header(
                "authorization",
                $"Bearer {_operatorToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/serviceaccount/user/roles")
            .Then()
            .StatusCode(200)
            .Extract()
            .Response();
        return DeserializeData<List<UserRoleWithDescription>>(response.Content.ReadAsStringAsync()
            .Result);
    }

    private static T? DeserializeData<T>(string jsonString)
    {
        var deserializedData = JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);
        return deserializedData;
    }
}
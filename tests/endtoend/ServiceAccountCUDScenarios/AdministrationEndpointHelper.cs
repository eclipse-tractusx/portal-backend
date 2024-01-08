/********************************************************************************
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Castle.Core.Internal;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using RestAssured.Response.Logging;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public static class AdministrationEndpointHelper
{
    private static readonly string BaseUrl = TestResources.BasePortalBackendUrl;
    private static readonly string EndPoint = "/api/administration";
    private static readonly Secrets Secrets = new();

    private static string? PortalUserToken;

    private static readonly string PortalUserCompanyName = TestResources.PortalUserCompanyName;

    public static async Task<bool> GetOperatorToken()
    {
        PortalUserToken = await new AuthFlow(PortalUserCompanyName).GetAccessToken(Secrets.PortalUserName, Secrets.PortalUserPassword);
        return !PortalUserToken.IsNullOrEmpty();
    }

    //GET: api/administration/serviceaccount/owncompany/serviceaccounts
    public static List<CompanyServiceAccountData> GetServiceAccounts()
    {
        var serviceAccounts = new List<CompanyServiceAccountData>();

        var totalPagesStr = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts?size=15")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Body("$.meta.totalPages").ToString();

        var failure = int.TryParse(totalPagesStr, out var totalPages);

        if (!failure)
            return serviceAccounts;
        for (var i = 0; i < totalPages; i++)
        {
            var response = Given()
                .DisableSslCertificateValidation()
                .Header(
                    "authorization",
                    $"Bearer {PortalUserToken}")
                .When()
                .Get($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts?page={i}&size=15")
                .Then()
                .Log(ResponseLogLevel.OnError)
                .StatusCode(200)
                .Extract()
                .Response();
            var data = DataHandleHelper.DeserializeData<Pagination.Response<CompanyServiceAccountData>>(response.Content
                .ReadAsStringAsync()
                .Result);
            if (data != null)
                serviceAccounts.AddRange(data.Content);
        }

        return serviceAccounts;
    }

    //POST: api/administration/serviceaccount/owncompany/serviceaccounts - create a new service account
    public static ServiceAccountDetails CreateNewServiceAccount(string[] permissions)
    {
        var now = DateTime.Now;
        var techUserName = $"NewTechUserName_{now:s}";

        const string Description = "This is a new technical user created via test automation e2e tests";
        var allServiceAccountsRoles = GetAllServiceAccountRoles();
        var userRoleIds = new List<Guid>();

        foreach (var p in permissions)
        {
            userRoleIds.AddRange(from t in allServiceAccountsRoles where t.UserRoleText.Contains(p) select t.UserRoleId);
        }

        var serviceAccountCreationInfo = new ServiceAccountCreationInfo(techUserName, Description, IamClientAuthMethod.SECRET, userRoleIds);
        var endpoint = $"{EndPoint}/serviceaccount/owncompany/serviceaccounts";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Body(DataHandleHelper.SerializeData(serviceAccountCreationInfo))
            .Post(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(201)
            .Extract()
            .Response();

        var serviceAccountDetails = DataHandleHelper.DeserializeData<ServiceAccountDetails>(response.Content.ReadAsStringAsync().Result);
        if (serviceAccountDetails is null)
        {
            throw new Exception($"Could not create service account with {endpoint}, response was null/empty.");
        }
        return serviceAccountDetails;
    }

    //DELETE: api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}
    public static bool DeleteServiceAccount(string serviceAccountId)
    {
        var data = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Delete($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        return !data.Content.ReadAsStringAsync().Result.Equals("0");
    }

    //PUT: api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId} - update the previous created service account details by changing "name" and "description"
    public static void UpdateServiceAccountDetailsById(string serviceAccountId, string newName,
        string newDescription)
    {
        var serviceAccountDetails = GetServiceAccountDetailsById(serviceAccountId);
        var updateServiceAccountEditableDetails =
            new ServiceAccountEditableDetails(serviceAccountDetails.ServiceAccountId, newName, newDescription,
                serviceAccountDetails.IamClientAuthMethod);
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Body(DataHandleHelper.SerializeData(updateServiceAccountEditableDetails))
            .Put($"{BaseUrl}{EndPoint}/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200);
    }

    //POST: api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}/resetCredentials - reset service account credentials
    public static ServiceAccountDetails ResetServiceAccountCredentialsById(string serviceAccountId)
    {
        var endpoint = $"{EndPoint}/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}/resetCredentials";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Post(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();

        var resetServiceAccountCredentialsById = DataHandleHelper.DeserializeData<ServiceAccountDetails>(response.Content.ReadAsStringAsync().Result);
        if (resetServiceAccountCredentialsById is null)
        {
            throw new Exception($"Could not reset service account credentials by id with {endpoint}, response was null/empty.");
        }
        return resetServiceAccountCredentialsById;
    }

    //GET: api/administration/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}
    public static ServiceAccountDetails GetServiceAccountDetailsById(string serviceAccountId)
    {
        var endpoint = $"{EndPoint}/serviceaccount/owncompany/serviceaccounts/{serviceAccountId}";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        return DataHandleHelper.DeserializeData<ServiceAccountDetails>(response.Content.ReadAsStringAsync()
            .Result) ?? throw new Exception($"Could not get service account details by id from {endpoint}, response was null/empty.");
    }

    //GET: api/administration/serviceaccount/user/roles
    private static List<UserRoleWithDescription> GetAllServiceAccountRoles()
    {
        var endpoint = $"{EndPoint}/serviceaccount/user/roles";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {PortalUserToken}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Response();
        var userRoleWithDescriptions = DataHandleHelper.DeserializeData<List<UserRoleWithDescription>>(response.Content.ReadAsStringAsync()
            .Result);
        if (userRoleWithDescriptions is null)
        {
            throw new Exception($"Could not get service account roles from {endpoint}, response was null/empty.");
        }
        return userRoleWithDescriptions;
    }
}

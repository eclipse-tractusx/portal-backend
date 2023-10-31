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

using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using RestAssured.Response.Logging;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using static RestAssured.Dsl;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

[Trait("Category", "Portal")]
[Collection("Portal")]
public class ModifyCoreUserRoleScenario : EndToEndTestBase
{
    private static readonly string BaseUrl = TestResources.BasePortalBackendUrl;
    private static readonly string EndPoint = "/api/notification";
    private static readonly string AdminEndPoint = "/api/administration";
    private string? _companyUserId;
    private string? _portalUserToken;
    private string? _username;
    private static readonly string _portalUserCompanyName = TestResources.PortalUserCompanyName;
    private static readonly string OfferId = TestResources.NotificationOfferId;
    private static readonly Secrets Secrets = new();

    public ModifyCoreUserRoleScenario(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Scenario_HappyPath_AssignAndUnassignCoreUserRoles()
    {
        await GetPortalUserToken();
        _companyUserId = GetCompanyUserId();

        _portalUserToken = await new AuthFlow(_portalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
            Secrets.PortalUserPassword);

        var assignedRoles = GetUserAssignedRoles();
        var roleToModify = GetRandomRoleToModify(assignedRoles);

        ModifyCoreUserRoles_AssignRole(assignedRoles, roleToModify);
        var notificationAddContent = GetNotificationCreated(NotificationTypeId.ROLE_UPDATE_CORE_OFFER);
        notificationAddContent.Should().NotBeNull();
        notificationAddContent!.Username.Should().Be(_username);
        notificationAddContent.AddedRoles.Should().Be(roleToModify,
            $"Notification by assigning the role {roleToModify} to the user was not created correctly");
        notificationAddContent.RemovedRoles.Should().Be("", "Notification has unexpected removeRoles");

        ModifyCoreUserRoles_UnAssignRole(assignedRoles, roleToModify);
        var notificationRemoveContent = GetNotificationCreated(NotificationTypeId.ROLE_UPDATE_CORE_OFFER);
        notificationRemoveContent.Should().NotBeNull();
        notificationRemoveContent!.Username.Should().Be(_username);
        notificationRemoveContent.AddedRoles.Should().Be("", "Notification has unexpected addedRoles");
        notificationRemoveContent.RemovedRoles.Should()
            .Be(roleToModify, "Notification by unassigning the role {roleToModify} to the user was not created correctly");
    }

    private async Task GetPortalUserToken()
    {
        _portalUserToken =
            await new AuthFlow(_portalUserCompanyName).GetAccessToken(Secrets.PortalUserName,
                Secrets.PortalUserPassword);
    }

    //GET: api/administration/user/owncompany/roles/coreoffers
    private string GetRandomRoleToModify(List<string> assignedRoles)
    {
        var newRoles = new List<string>();

        var existingRoles = GetCoreOfferRolesNames();
        foreach (var t in existingRoles)
        {
            newRoles.AddRange(from t1 in assignedRoles where t != t1 select t);
        }

        return newRoles.ElementAt(new Random().Next(0, newRoles.Count - 1));
    }

    private List<string> GetCoreOfferRolesNames()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/user/owncompany/roles/coreoffers")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Response();

        var data = DataHandleHelper.DeserializeData<List<OfferRoleInfos>>(response.Content.ReadAsStringAsync().Result);
        if (data == null)
            throw new Exception("Cannot fetch core user roles");
        var roleNames = new List<string>();
        foreach (var offerRoleInfo in data.Where(t => t.OfferId.ToString() == OfferId))
        {
            roleNames = offerRoleInfo.RoleInfos.Select(t => t.RoleText).ToList();
        }

        return roleNames;
    }

    //PUT: api/administration/user/owncompany/users/{companyUserId}/coreoffers/{offerId}/roles
    private void ModifyCoreUserRoles_AssignRole(List<string> assignedRoles, string roleToModify)
    {
        assignedRoles.Add(roleToModify);

        var body = JsonSerializer.Serialize(assignedRoles);
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Body(body)
            .Put($"{BaseUrl}{AdminEndPoint}/user/owncompany/users/{_companyUserId}/coreoffers/{OfferId}/roles")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
    }

    private void ModifyCoreUserRoles_UnAssignRole(List<string> assignedRoles, string roleToModify)
    {
        assignedRoles.Remove(roleToModify);

        var body = JsonSerializer.Serialize(assignedRoles);
        // Given
        Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Body(body)
            .Put($"{BaseUrl}{AdminEndPoint}/user/owncompany/users/{_companyUserId}/coreoffers/{OfferId}/roles")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
    }

    //GET: api/administration/user/ownUser
    private string GetCompanyUserId()
    {
        var endpoint = $"{AdminEndPoint}/user/ownUser";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Response();

        var data = response.Content.ReadAsStringAsync().Result;
        var companyUserDetails = DataHandleHelper.DeserializeData<CompanyUserDetails>(data);
        if (companyUserDetails is null)
        {
            throw new Exception($"Could not get company user details from {endpoint} should not be null.");
        }
        _username = companyUserDetails.FirstName + " " + companyUserDetails.LastName;
        return companyUserDetails.CompanyUserId.ToString();
    }

    //GET: api/administration/user/owncompany/users/{_companyUserId}
    private List<string> GetUserAssignedRoles()
    {
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get($"{BaseUrl}{AdminEndPoint}/user/owncompany/users/{_companyUserId}")
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .And()
            .Extract()
            .Response();
        var data = response.Content.ReadAsStringAsync().Result;
        var assignedRoles = DataHandleHelper.DeserializeData<CompanyUserDetails>(data)?.AssignedRoles.First().UserRoles
            .ToList();
        return assignedRoles ?? new List<string>();
    }

    private NotificationContent? GetNotificationCreated(NotificationTypeId notificationTypeId)
    {
        var endpoint = $"{EndPoint}/?page=0&size=10&sorting=DateDesc";
        var response = Given()
            .DisableSslCertificateValidation()
            .Header(
                "authorization",
                $"Bearer {_portalUserToken}")
            .When()
            .Get(BaseUrl + endpoint)
            .Then()
            .Log(ResponseLogLevel.OnError)
            .StatusCode(200)
            .Extract()
            .Body("$.content");
        var data = DataHandleHelper.DeserializeData<List<NotificationDetailData>>(response.ToString() ?? "");
        if (response is null || data is null)
        {
            throw new Exception($"Response/data got from {endpoint} should not be null.");
        }
        var content = data.First().Content;
        if (content is null)
        {
            throw new Exception($"Content in response from {endpoint} should not be null.");
        }
        data.First().TypeId.Should().Be(notificationTypeId);
        return DataHandleHelper.DeserializeData<NotificationContent>(content);

    }
}

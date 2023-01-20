/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class UserUploadBusinessLogic : IUserUploadBusinessLogic
{
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly UserSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="settings">Settings</param>
    public UserUploadBusinessLogic(
        IUserProvisioningService userProvisioningService,
        IOptions<UserSettings> settings)
    {
        _userProvisioningService = userProvisioningService;
        _settings = settings.Value;
    }

    public ValueTask<UserCreationStats> UploadOwnCompanyIdpUsersAsync(Guid identityProviderId, IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        CsvParser.ValidateContentTypeTextCSV(document.ContentType);
        return UploadOwnCompanyIdpUsersInternalAsync(identityProviderId, document, iamUserId, cancellationToken);
    }

    private async ValueTask<UserCreationStats> UploadOwnCompanyIdpUsersInternalAsync(Guid identityProviderId, IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        using var stream = document.OpenReadStream();

        var (companyNameIdpAliasData, _) = await _userProvisioningService.GetCompanyNameIdpAliasData(identityProviderId, iamUserId).ConfigureAwait(false);

        var validRoleData = new List<UserRoleData>();

        var (numCreated, numLines, errors) = await CsvParser.ProcessCsvAsync(
            stream,
            line => {
                var csvHeaders = new [] { CsvHeaders.FirstName, CsvHeaders.LastName, CsvHeaders.Email, CsvHeaders.ProviderUserName, CsvHeaders.ProviderUserId, CsvHeaders.Roles }.Select(h => h.ToString());
                CsvParser.ValidateCsvHeaders(line, csvHeaders);
            },
            async line => {
                var parsed = ParseUploadOwnIdpUsersCSVLine(line, companyNameIdpAliasData.IsSharedIdp);
                return new UserCreationRoleDataIdpInfo(
                    parsed.FirstName,
                    parsed.LastName,
                    parsed.Email,
                    await GetUserRoleDatas(parsed.Roles, validRoleData, iamUserId).ConfigureAwait(false),
                    parsed.ProviderUserName,
                    parsed.ProviderUserId);
            },
            lines =>
                _userProvisioningService
                    .CreateOwnCompanyIdpUsersAsync(
                        companyNameIdpAliasData,
                        lines,
                        cancellationToken)
                    .Select(x => (x.CompanyUserId != Guid.Empty, x.Error)),
            cancellationToken).ConfigureAwait(false);

        return new UserCreationStats(numCreated, errors.Count(), numLines, errors.Select(x => $"line: {x.Line}, message: {x.Error.Message}"));
    }

    private static (string FirstName, string LastName, string Email, string ProviderUserName, string ProviderUserId, IEnumerable<string> Roles) ParseUploadOwnIdpUsersCSVLine(string line, bool isSharedIdp)
    {
        var items = line.Split(",").AsEnumerable().GetEnumerator();
        var firstName = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.FirstName);
        var lastName = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.LastName);
        var email = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.Email);
        var providerUserName = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.ProviderUserName);
        var providerUserId = isSharedIdp
            ? CsvParser.NextStringItemIsNotNull(items,CsvHeaders.ProviderUserId)
            : CsvParser.NextStringItemIsNotNullOrWhiteSpace(items,CsvHeaders.ProviderUserId);
        var roles = CsvParser.TrailingStringItemsNotNullOrWhiteSpace(items, CsvHeaders.Roles).ToList();
        return (firstName, lastName, email, providerUserName, providerUserId, roles);
    }

    public ValueTask<UserCreationStats> UploadOwnCompanySharedIdpUsersAsync(IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        CsvParser.ValidateContentTypeTextCSV(document.ContentType);
        return UploadOwnCompanySharedIdpUsersInternalAsync(document, iamUserId, cancellationToken);
    }

    private async ValueTask<UserCreationStats> UploadOwnCompanySharedIdpUsersInternalAsync(IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        using var stream = document.OpenReadStream();

        var (companyNameIdpAliasData, _) = await _userProvisioningService.GetCompanyNameSharedIdpAliasData(iamUserId).ConfigureAwait(false);

        var validRoleData = new List<UserRoleData>();

        var (numCreated, numLines, errors) = await CsvParser.ProcessCsvAsync(
            stream,
            line => {
                var csvHeaders = new [] { CsvHeaders.FirstName, CsvHeaders.LastName, CsvHeaders.Email, CsvHeaders.Roles }.Select(h => h.ToString());
                CsvParser.ValidateCsvHeaders(line, csvHeaders);
            },
            async line => {
                var parsed = ParseUploadSharedIdpUsersCSVLine(line);
                return new UserCreationRoleDataIdpInfo(
                    parsed.FirstName,
                    parsed.LastName,
                    parsed.Email,
                    await GetUserRoleDatas(parsed.Roles, validRoleData, iamUserId).ConfigureAwait(false),
                    parsed.Email,
                    "");
            },
            lines =>
                _userProvisioningService
                    .CreateOwnCompanyIdpUsersAsync(
                        companyNameIdpAliasData,
                        lines,
                        cancellationToken)
                    .Select(x => (x.CompanyUserId != Guid.Empty, x.Error)),
            cancellationToken).ConfigureAwait(false);

        return new UserCreationStats(numCreated, errors.Count(), numLines, errors.Select(x => $"line: {x.Line}, message: {x.Error.Message}"));
    }

    private async ValueTask<IEnumerable<UserRoleData>> GetUserRoleDatas(IEnumerable<string> roles, List<UserRoleData> validRoleData, string iamUserId)
    {
        var unknownRoles = roles.Except(validRoleData.Select(r => r.UserRoleText));
        if (unknownRoles.Any())
        {
            var roleData = await _userProvisioningService.GetOwnCompanyPortalRoleDatas(_settings.Portal.KeyCloakClientID, unknownRoles, iamUserId)
                .ConfigureAwait(false);

            if (roleData != null)
            {
                validRoleData.AddRange(roleData);
            }
        }
        return validRoleData.IntersectBy(roles, r => r.UserRoleText);
    }

    private static (string FirstName, string LastName, string Email, IEnumerable<string> Roles) ParseUploadSharedIdpUsersCSVLine(string line)
    {
        var items = line.Split(",").AsEnumerable().GetEnumerator();
        var firstName = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.FirstName);
        var lastName = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.LastName);
        var email = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.Email);
        var roles = CsvParser.TrailingStringItemsNotNullOrWhiteSpace(items, CsvHeaders.Roles).ToList();
        return (firstName, lastName, email, roles);
    }

    public enum CsvHeaders
    {
        FirstName,
        LastName,
        Email,
        ProviderUserName,
        ProviderUserId,
        Roles
    }
}

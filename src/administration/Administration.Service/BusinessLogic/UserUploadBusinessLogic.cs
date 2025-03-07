/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.IO;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Mailing.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class UserUploadBusinessLogic : IUserUploadBusinessLogic
{
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IIdentityProviderProvisioningService _identityProviderProvisioningService;
    private readonly IMailingProcessCreation _mailingProcessCreation;
    private readonly UserSettings _settings;
    private readonly IIdentityData _identityData;
    private readonly IErrorMessageService _errorMessageService;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="identityProviderProvisioningService">IdentityProvider Provisioning Service</param>
    /// <param name="mailingProcessCreation">The mailingProcessCreation</param>
    /// <param name="identityService">Access to the identity Service</param>
    /// <param name="errorMessageService">ErrorMessage Service</param>
    /// <param name="settings">Settings</param>
    public UserUploadBusinessLogic(
        IUserProvisioningService userProvisioningService,
        IIdentityProviderProvisioningService identityProviderProvisioningService,
        IMailingProcessCreation mailingProcessCreation,
        IIdentityService identityService,
        IErrorMessageService errorMessageService,
        IOptions<UserSettings> settings)
    {
        _userProvisioningService = userProvisioningService;
        _identityProviderProvisioningService = identityProviderProvisioningService;
        _mailingProcessCreation = mailingProcessCreation;
        _identityData = identityService.IdentityData;
        _errorMessageService = errorMessageService;
        _settings = settings.Value;
    }

    public ValueTask<UserCreationStats> UploadOwnCompanyIdpUsersAsync(Guid identityProviderId, IFormFile document, CancellationToken cancellationToken)
    {
        CsvParser.ValidateContentTypeTextCSV(document.ContentType);
        return UploadOwnCompanyIdpUsersInternalAsync(identityProviderId, document, cancellationToken);
    }

    private async ValueTask<UserCreationStats> UploadOwnCompanyIdpUsersInternalAsync(Guid identityProviderId, IFormFile document, CancellationToken cancellationToken)
    {
        using var stream = document.OpenReadStream();

        var (companyNameIdpAliasData, nameCreatedBy) = await _userProvisioningService.GetCompanyNameIdpAliasData(identityProviderId, _identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);

        var validRoleData = new List<UserRoleData>();

        var displayName = companyNameIdpAliasData.IsSharedIdp
            ? null
            : await _identityProviderProvisioningService.GetIdentityProviderDisplayName(companyNameIdpAliasData.IdpAlias).ConfigureAwait(ConfigureAwaitOptions.None) ?? companyNameIdpAliasData.IdpAlias;

        var (numCreated, numLines, errors) = await CsvParser.ProcessCsvAsync(
            stream,
            line =>
            {
                var csvHeaders = new[] { CsvHeaders.FirstName, CsvHeaders.LastName, CsvHeaders.Email, CsvHeaders.ProviderUserName, CsvHeaders.ProviderUserId, CsvHeaders.Roles }.Select(h => h.ToString());
                CsvParser.ValidateCsvHeaders(line, csvHeaders);
            },
            async line =>
            {
                var parsed = ParseUploadOwnIdpUsersCSVLine(line, companyNameIdpAliasData.IsSharedIdp);
                ValidateUserCreationRoles(parsed.Roles);
                return new UserCreationRoleDataIdpInfo(
                    parsed.FirstName,
                    parsed.LastName,
                    parsed.Email,
                    await GetUserRoleDatas(parsed.Roles, validRoleData, _identityData.CompanyId).ConfigureAwait(false),
                    parsed.ProviderUserName,
                    parsed.ProviderUserId,
                    UserStatusId.ACTIVE,
                    true);
            },
            lines =>
                _userProvisioningService
                    .CreateOwnCompanyIdpUsersAsync(
                        companyNameIdpAliasData,
                        lines,
                        creationData =>
                        {
                            if (companyNameIdpAliasData.IsSharedIdp)
                                return;
                            var mailParameters = ImmutableDictionary.CreateRange<string, string>([
                                new("nameCreatedBy", nameCreatedBy),
                                new("url", _settings.Portal.BasePortalAddress),
                                new("idpAlias", displayName ?? throw new UnexpectedConditionException("displayname should never be null here"))
                            ]);
                            _mailingProcessCreation.CreateMailProcess(creationData.UserCreationInfo.Email, "NewUserExternalIdpTemplate", mailParameters);
                        },
                        cancellationToken)
                    .Select(x => (x.CompanyUserId != Guid.Empty, x.Error)),
            cancellationToken).ConfigureAwait(false);

        return new UserCreationStats(
            numCreated,
            errors.Count(),
            numLines,
            errors.Select(error => CreateUserCreationError(error.Line, error.Error)));
    }

    private static void ValidateUserCreationRoles(IEnumerable<string> roles)
    {
        if (!roles.Any())
        {
            throw new ControllerArgumentException("at least one role must be specified");
        }
    }

    private static (string FirstName, string LastName, string Email, string ProviderUserName, string ProviderUserId, IEnumerable<string> Roles) ParseUploadOwnIdpUsersCSVLine(string line, bool isSharedIdp)
    {
        var items = line.Split(",").AsEnumerable().GetEnumerator();
        var firstName = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.FirstName);
        var lastName = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.LastName);
        var email = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.Email);
        var providerUserName = CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.ProviderUserName);
        var providerUserId = isSharedIdp
            ? CsvParser.NextStringItemIsNotNull(items, CsvHeaders.ProviderUserId)
            : CsvParser.NextStringItemIsNotNullOrWhiteSpace(items, CsvHeaders.ProviderUserId);
        var roles = CsvParser.TrailingStringItemsNotNullOrWhiteSpace(items, CsvHeaders.Roles).ToList();
        return (firstName, lastName, email, providerUserName, providerUserId, roles);
    }

    public ValueTask<UserCreationStats> UploadOwnCompanySharedIdpUsersAsync(IFormFile document, CancellationToken cancellationToken)
    {
        CsvParser.ValidateContentTypeTextCSV(document.ContentType);
        return UploadOwnCompanySharedIdpUsersInternalAsync(document, cancellationToken);
    }

    private async ValueTask<UserCreationStats> UploadOwnCompanySharedIdpUsersInternalAsync(IFormFile document, CancellationToken cancellationToken)
    {
        using var stream = document.OpenReadStream();

        var (companyNameIdpAliasData, nameCreatedBy) = await _userProvisioningService.GetCompanyNameSharedIdpAliasData(_identityData.IdentityId).ConfigureAwait(ConfigureAwaitOptions.None);
        var displayName = await _identityProviderProvisioningService.GetIdentityProviderDisplayName(companyNameIdpAliasData.IdpAlias).ConfigureAwait(ConfigureAwaitOptions.None) ?? companyNameIdpAliasData.IdpAlias;

        var validRoleData = new List<UserRoleData>();

        var (numCreated, numLines, errors) = await CsvParser.ProcessCsvAsync(
            stream,
            line =>
            {
                var csvHeaders = new[] { CsvHeaders.FirstName, CsvHeaders.LastName, CsvHeaders.Email, CsvHeaders.Roles }.Select(h => h.ToString());
                CsvParser.ValidateCsvHeaders(line, csvHeaders);
            },
            async line =>
            {
                var parsed = ParseUploadSharedIdpUsersCSVLine(line);
                ValidateUserCreationRoles(parsed.Roles);
                return new UserCreationRoleDataIdpInfo(
                    parsed.FirstName,
                    parsed.LastName,
                    parsed.Email,
                    await GetUserRoleDatas(parsed.Roles, validRoleData, _identityData.CompanyId).ConfigureAwait(false),
                    parsed.Email,
                    "",
                    UserStatusId.ACTIVE,
                    true);
            },
            lines =>
                _userProvisioningService
                    .CreateOwnCompanyIdpUsersAsync(
                        companyNameIdpAliasData,
                        lines,
                        creationData =>
                        {
                            var mailParameters = ImmutableDictionary.CreateRange<string, string>([
                                new("password", creationData.Password ?? ""),
                                new("companyName", displayName),
                                new("nameCreatedBy", nameCreatedBy),
                                new("url", _settings.Portal.BasePortalAddress),
                                new("passwordResendUrl", _settings.Portal.PasswordResendAddress),
                            ]);
                            _mailingProcessCreation.CreateMailProcess(creationData.UserCreationInfo.Email, "NewUserTemplate", mailParameters);
                            _mailingProcessCreation.CreateMailProcess(creationData.UserCreationInfo.Email, "NewUserPasswordTemplate", mailParameters);
                        },
                        cancellationToken)
                    .Select(x => (x.Error == null, x.Error)),
            cancellationToken).ConfigureAwait(false);

        return new UserCreationStats(
            numCreated,
            errors.Count(),
            numLines,
            errors.Select(error => CreateUserCreationError(error.Line, error.Error)));
    }

    private UserCreationError CreateUserCreationError(int line, Exception error) =>
        error switch
        {
            DetailException detailException when detailException.HasDetails => new UserCreationError(line, detailException.GetErrorMessage(_errorMessageService), detailException.GetErrorDetails(_errorMessageService)),
            _ => new UserCreationError(line, error.Message, Enumerable.Empty<ErrorDetails>())
        };

    private async ValueTask<IEnumerable<UserRoleData>> GetUserRoleDatas(IEnumerable<string> roles, List<UserRoleData> validRoleData, Guid companyId)
    {
        if (roles.Except(validRoleData.Select(r => r.UserRoleText)).IfAny(
            unknownRoles => _userProvisioningService.GetOwnCompanyPortalRoleDatas(_settings.Portal.KeycloakClientID, unknownRoles, companyId),
            out var roleDataTask))
        {
            var roleData = await roleDataTask.ConfigureAwait(ConfigureAwaitOptions.None);
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

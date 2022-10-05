/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the CatenaX (ng) GitHub Organisation.
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

using Org.CatenaX.Ng.Portal.Backend.Administration.Service.Models;
using Org.CatenaX.Ng.Portal.Backend.Framework.ErrorHandling;
using Org.CatenaX.Ng.Portal.Backend.Framework.IO;
using Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Models;
using Org.CatenaX.Ng.Portal.Backend.Provisioning.Library.Service;
using Microsoft.Extensions.Options;
using System.Text;

namespace Org.CatenaX.Ng.Portal.Backend.Administration.Service.BusinessLogic;

public class UserUploadBusinessLogic : IUserUploadBusinessLogic
{
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly IPortalRepositories _portalRepositories;
    private readonly UserSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="userProvisioningService">User Provisioning Service</param>
    /// <param name="settings">Settings</param>
    /// <param name="portalRepositories">Portal Repositories</param>
    public UserUploadBusinessLogic(
        IUserProvisioningService userProvisioningService,
        IPortalRepositories portalRepositories,
        IOptions<UserSettings> settings)
    {
        _userProvisioningService = userProvisioningService;
        _portalRepositories = portalRepositories;
        _settings = settings.Value;
    }

    public ValueTask<IdentityProviderUserCreationStats> UploadOwnCompanyIdpUsersAsync(Guid identityProviderId, IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        ValidateContentTypeTextCSV(document);

        return UploadIdpUsersInternalAsync(
            document,
            () => _userProvisioningService.GetCompanyNameIdpAliasData(identityProviderId, iamUserId),
            line => {
                var csvHeaders = new [] { CSVHeaders.FirstName, CSVHeaders.LastName, CSVHeaders.Email, CSVHeaders.ProviderUserName, CSVHeaders.ProviderUserId, CSVHeaders.Roles }.Select(h => h.ToString());
                ValidateUploadCSVHeaders(line, csvHeaders);
            },
            (line,isSharedIdp) => {
                var parsed = ParseUploadOwnIdpUsersCSVLine(line, isSharedIdp);
                return new UserCreationInfoIdp(parsed.FirstName, parsed.LastName, parsed.Email, parsed.Roles, parsed.Email, "");
            },
            cancellationToken);
    }

    private static (string FirstName, string LastName, string Email, string ProviderUserName, string ProviderUserId, IEnumerable<string> Roles) ParseUploadOwnIdpUsersCSVLine(string line, bool isSharedIdp)
    {
        var items = line.Split(",").AsEnumerable().GetEnumerator();
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for {CSVHeaders.FirstName} type string expected");
        }
        var firstName = items.Current;
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for {CSVHeaders.LastName} type string expected");
        }
        var lastName = items.Current;
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for {CSVHeaders.Email} type string expected");
        }
        var email = items.Current;
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for {CSVHeaders.ProviderUserName} type string expected");
        }
        var providerUserName = items.Current;
        if(!items.MoveNext() || (!isSharedIdp && string.IsNullOrWhiteSpace(items.Current)))
        {
            throw new ControllerArgumentException($"value for {CSVHeaders.ProviderUserId} type string expected");
        }
        var providerUserId = items.Current;
        var roles = ParseUploadOwnIdpUsersRoles(items).ToList();
        return (firstName, lastName, email, providerUserName, providerUserId, roles);
    }

    public ValueTask<IdentityProviderUserCreationStats> UploadOwnCompanySharedIdpUsersAsync(IFormFile document, string iamUserId, CancellationToken cancellationToken)
    {
        ValidateContentTypeTextCSV(document);

        return UploadIdpUsersInternalAsync(
            document,
            () => _userProvisioningService.GetCompanyNameSharedIdpAliasData(iamUserId),
            line => {
                var csvHeaders = new [] { CSVHeaders.FirstName, CSVHeaders.LastName, CSVHeaders.Email, CSVHeaders.Roles }.Select(h => h.ToString());
                ValidateUploadCSVHeaders(line, csvHeaders);
            },
            (line,_) => {
                var parsed = ParseUploadSharedIdpUsersCSVLine(line);
                return new UserCreationInfoIdp(parsed.FirstName, parsed.LastName, parsed.Email, parsed.Roles, parsed.Email, "");
            },
            cancellationToken);
    }

    private static (string FirstName, string LastName, string Email, IEnumerable<string> Roles) ParseUploadSharedIdpUsersCSVLine(string line)
    {
        var items = line.Split(",").AsEnumerable().GetEnumerator();
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for {CSVHeaders.FirstName} type string expected");
        }
        var firstName = items.Current;
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for {CSVHeaders.LastName} type string expected");
        }
        var lastName = items.Current;
        if(!items.MoveNext() || string.IsNullOrWhiteSpace(items.Current))
        {
            throw new ControllerArgumentException($"value for {CSVHeaders.Email} type string expected");
        }
        var email = items.Current;
        var roles = ParseUploadOwnIdpUsersRoles(items).ToList();
        return (firstName, lastName, email, roles);
    }

    private static IEnumerable<string> ParseUploadOwnIdpUsersRoles(IEnumerator<string> items)
    {
        while (items.MoveNext())
        {
            if(string.IsNullOrWhiteSpace(items.Current))
            {
                throw new ControllerArgumentException($"value for Role type string expected");
            }
            yield return items.Current;
        }
    }

    private void ValidateContentTypeTextCSV(IFormFile document)
    {
        if (!document.ContentType.Equals("text/csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnsupportedMediaTypeException($"Only contentType text/csv files are allowed.");
        }
    }

    private static void ValidateUploadCSVHeaders(string firstLine, IEnumerable<string> csvHeaders)
    {
        var headers = firstLine.Split(",").GetEnumerator();
        foreach (var csvHeader in csvHeaders)
        {
            if (!headers.MoveNext())
            {
                throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got ''");
            }
            if ((string)headers.Current != csvHeader)
            {
                throw new ControllerArgumentException($"invalid format: expected '{csvHeader}', got '{headers.Current}'");
            }
        }
    }

    private async ValueTask<IdentityProviderUserCreationStats> UploadIdpUsersInternalAsync(IFormFile document, Func<Task<CompanyNameIdpAliasData>> getCompanyNameIdpAliasDataAsync, Action<string> validateFirstLine, Func<string,bool,UserCreationInfoIdp> parseUserCreationInfo, CancellationToken cancellationToken)
    {
        var companyNameIdpAliasData = await getCompanyNameIdpAliasDataAsync().ConfigureAwait(false);

        using var stream = document.OpenReadStream();
        var reader = new StreamReader(new CancellableStream(stream, cancellationToken), Encoding.UTF8);

        int numCreated = 0;
        var errors = new List<String>();
        int numLines = 0;

        try
        {
            await ValidateFirstLineAsync(reader, validateFirstLine).ConfigureAwait(false);

            await foreach (var result in _userProvisioningService.CreateOwnCompanyIdpUsersAsync(
                companyNameIdpAliasData,
                _settings.Portal.KeyCloakClientID,
                ParseUploadIdpUsersCSVLines(reader, parseUserCreationInfo, companyNameIdpAliasData.IsShardIdp)))
            {
                numLines++;
                if (result.Error != null)
                {
                    errors.Add($"line: {numLines}, message: {result.Error.Message}");
                }
                else
                {
                    numCreated++;
                }
            }
        }
        catch(TaskCanceledException tce)
        {
            errors.Add($"line: {numLines}, message: {tce.Message}");
        }
        return new IdentityProviderUserCreationStats(numCreated, errors.Count, numLines, errors);
    }

    private static async IAsyncEnumerable<UserCreationInfoIdp> ParseUploadIdpUsersCSVLines(StreamReader reader, Func<string,bool,UserCreationInfoIdp> parseUserCreationInfo, bool isSharedIdp)
    {
        var nextLine = await reader.ReadLineAsync().ConfigureAwait(false);

        while (nextLine != null)
        {
            yield return parseUserCreationInfo(nextLine, isSharedIdp);
            nextLine = await reader.ReadLineAsync().ConfigureAwait(false);
        }
    }

    private static async ValueTask ValidateFirstLineAsync(StreamReader reader, Action<string> validateFirstLine)
    {
        var firstLine = await reader.ReadLineAsync().ConfigureAwait(false);
        if (firstLine == null)
        {
            throw new ControllerArgumentException("uploaded file contains no lines");
        }
        validateFirstLine(firstLine);
    }

    private enum CSVHeaders
    {
        FirstName,
        LastName,
        Email,
        ProviderUserName,
        ProviderUserId,
        Roles
    }
}

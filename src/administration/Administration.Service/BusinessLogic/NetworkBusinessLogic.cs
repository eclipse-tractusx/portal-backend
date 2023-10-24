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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class NetworkBusinessLogic : INetworkBusinessLogic
{
    private static readonly Regex Name = new(ValidationExpressions.Name, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex Email = new(ValidationExpressions.Email, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex BpnRegex = new(ValidationExpressions.Bpn, RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityService _identityService;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly INetworkRegistrationProcessHelper _processHelper;
    private readonly PartnerRegistrationSettings _settings;

    public NetworkBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IUserProvisioningService userProvisioningService, INetworkRegistrationProcessHelper processHelper, IOptions<PartnerRegistrationSettings> options)
    {
        _portalRepositories = portalRepositories;
        _identityService = identityService;
        _userProvisioningService = userProvisioningService;
        _processHelper = processHelper;
        _settings = options.Value;
    }

    public async Task HandlePartnerRegistration(PartnerRegistrationData data)
    {
        var ownerCompanyId = _identityService.IdentityData.CompanyId;
        var networkRepository = _portalRepositories.GetInstance<INetworkRepository>();
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();

        var (roleData, identityProviderIdAliase, singleIdentityProviderIdAlias, allIdentityProviderIds) = await ValidatePartnerRegistrationData(data, networkRepository, identityProviderRepository, ownerCompanyId).ConfigureAwait(false);

        var companyId = CreatePartnerCompany(companyRepository, data);

        var applicationId = _portalRepositories.GetInstance<IApplicationRepository>().CreateCompanyApplication(companyId, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.EXTERNAL,
            ca =>
            {
                ca.OnboardingServiceProviderId = ownerCompanyId;
            }).Id;

        var processId = processStepRepository.CreateProcess(ProcessTypeId.PARTNER_REGISTRATION).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.SYNCHRONIZE_USER, ProcessStepStatusId.TODO, processId);

        networkRepository.CreateNetworkRegistration(data.ExternalId, companyId, processId, ownerCompanyId, applicationId);

        identityProviderRepository.CreateCompanyIdentityProviders(allIdentityProviderIds.Select(identityProviderId => (companyId, identityProviderId)));

        Guid GetIdpId(Guid? identityProviderId) =>
            identityProviderId == null
                ? singleIdentityProviderIdAlias?.IdentityProviderId ?? throw new UnexpectedConditionException("singleIdentityProviderIdAlias should never be null here")
                : identityProviderId.Value;

        string GetIdpAlias(Guid? identityProviderId) =>
            identityProviderId == null
                ? singleIdentityProviderIdAlias?.Alias ?? throw new UnexpectedConditionException("singleIdentityProviderIdAlias should never be null here")
                : identityProviderIdAliase?[identityProviderId.Value] ?? throw new UnexpectedConditionException("identityProviderIdAliase should never be null here and should always contain an entry for identityProviderId");

        async IAsyncEnumerable<(Guid CompanyUserId, Exception? Error)> CreateUsers()
        {
            var userRepository = _portalRepositories.GetInstance<IUserRepository>();
            await foreach (var (aliasData, creationInfos) in GetUserCreationData(companyId, GetIdpId, GetIdpAlias, data, roleData).ToAsyncEnumerable())
            {
                foreach (var creationInfo in creationInfos)
                {
                    var identityId = Guid.Empty;
                    Exception? error = null;
                    try
                    {
                        var (_, companyUserId) = await _userProvisioningService.GetOrCreateCompanyUser(userRepository, aliasData.IdpAlias,
                            creationInfo, companyId, aliasData.IdpId, data.Bpn).ConfigureAwait(false);
                        identityId = companyUserId;
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }

                    yield return (identityId, error);
                }
            }
        }

        var userCreationErrors = await CreateUsers().Where(x => x.Error != null).Select(x => x.Error!).ToListAsync();
        userCreationErrors.IfAny(errors => throw new ServiceException($"Errors occured while saving the users: ${string.Join("", errors.Select(x => x.Message))}", errors.First()));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private Guid CreatePartnerCompany(ICompanyRepository companyRepository, PartnerRegistrationData data)
    {
        var address = companyRepository.CreateAddress(data.City, data.StreetName,
            data.CountryAlpha2Code,
            a =>
            {
                a.Streetnumber = data.StreetNumber;
                a.Region = data.Region;
                a.Zipcode = data.ZipCode;
            });

        var company = companyRepository.CreateCompany(data.Name, c =>
        {
            c.AddressId = address.Id;
            c.BusinessPartnerNumber = data.Bpn;
        });

        companyRepository.CreateUpdateDeleteIdentifiers(company.Id, Enumerable.Empty<(UniqueIdentifierId, string)>(), data.UniqueIds.Select(x => (x.UniqueIdentifierId, x.Value)));
        _portalRepositories.GetInstance<ICompanyRolesRepository>().CreateCompanyAssignedRoles(company.Id, data.CompanyRoles);

        return company.Id;
    }

    private static IEnumerable<(CompanyNameIdpAliasData AliasData, IEnumerable<UserCreationRoleDataIdpInfo> CreationInfos)> GetUserCreationData(Guid companyId, Func<Guid?, Guid> getIdentityProviderId, Func<Guid?, string> getIdentityProviderAlias, PartnerRegistrationData data, IEnumerable<UserRoleData> roleData) =>
        data.UserDetails
            .GroupBy(x => x.IdentityProviderId)
            .Select(group =>
            {
                var companyNameIdpAliasData = new CompanyNameIdpAliasData(
                    companyId,
                    data.Name,
                    data.Bpn,
                    getIdentityProviderAlias(group.Key),
                    getIdentityProviderId(group.Key),
                    false
                );

                var userCreationInfos = group.Select(user =>
                    new UserCreationRoleDataIdpInfo(
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        roleData,
                        user.Username,
                        user.ProviderId,
                        UserStatusId.PENDING,
                        false
                    )
                );

                return (AliasData: companyNameIdpAliasData, CreationInfos: userCreationInfos);
            });

    public Task RetriggerProcessStep(Guid externalId, ProcessStepTypeId processStepTypeId) =>
        _processHelper.TriggerProcessStep(externalId, processStepTypeId);

    private async Task<(IEnumerable<UserRoleData> RoleData, IDictionary<Guid, string>? IdentityProviderIdAliase, (Guid IdentityProviderId, string Alias)? SingleIdentityProviderIdAlias, IEnumerable<Guid> AllIdentityProviderIds)> ValidatePartnerRegistrationData(PartnerRegistrationData data, INetworkRepository networkRepository, IIdentityProviderRepository identityProviderRepository, Guid ownerCompanyId)
    {
        if (data.Bpn != null)
        {
            if (!BpnRegex.IsMatch(data.Bpn))
            {
                throw new ControllerArgumentException("BPN must contain exactly 16 characters and must be prefixed with BPNL", nameof(data.Bpn));
            }

            if (await _portalRepositories.GetInstance<ICompanyRepository>().CheckBpnExists(data.Bpn).ConfigureAwait(false))
            {
                throw new ControllerArgumentException($"The Bpn {data.Bpn} already exists", nameof(data.Bpn));
            }
        }

        if (!data.CompanyRoles.Any())
        {
            throw new ControllerArgumentException("At least one company role must be selected", nameof(data.CompanyRoles));
        }

        foreach (var user in data.UserDetails)
        {
            ValidateUsers(user);
        }

        if (await networkRepository.CheckExternalIdExists(data.ExternalId, ownerCompanyId)
                .ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"ExternalId {data.ExternalId} already exists", nameof(data.ExternalId));
        }

        if (!await _portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(data.CountryAlpha2Code).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Location {data.CountryAlpha2Code} does not exist", nameof(data.CountryAlpha2Code));
        }

        var idpResult = await ValidateIdps(data, identityProviderRepository, ownerCompanyId).ConfigureAwait(false);

        IEnumerable<UserRoleData> roleData;
        try
        {
            roleData = await _userProvisioningService.GetRoleDatas(_settings.InitialRoles).ToListAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ConfigurationException($"{nameof(_settings.InitialRoles)}: {e.Message}");
        }

        return (roleData, idpResult.IdentityProviderIdAliasData, idpResult.SingleIdp, idpResult.AllIdentityProviderIds);
    }

    private static async Task<(IDictionary<Guid, string>? IdentityProviderIdAliasData, (Guid IdentityProviderId, string Alias)? SingleIdp, IEnumerable<Guid> AllIdentityProviderIds)> ValidateIdps(PartnerRegistrationData data, IIdentityProviderRepository identityProviderRepository, Guid ownerCompanyId)
    {
        var identityProviderIds = data.UserDetails
                    .Select(x => x.IdentityProviderId);

        (Guid IdentityProviderId, string Alias)? singleIdpAlias;

        if (identityProviderIds.Any(id => id == null))
        {
            try
            {
                var single = await identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(ownerCompanyId).ConfigureAwait(false);
                if (single.IdentityProviderId == Guid.Empty)
                    throw new ConflictException($"company {ownerCompanyId} has no managed identityProvider");
                singleIdpAlias = (single.IdentityProviderId, single.Alias ?? throw new ConflictException($"identityProvider {single.IdentityProviderId} has no alias"));
            }
            catch (InvalidOperationException)
            {
                throw new ControllerArgumentException($"Company {ownerCompanyId} has more than one identity provider linked, therefore identityProviderId must be set for all users", nameof(data.UserDetails));
            }
        }
        else
        {
            singleIdpAlias = null;
        }

        var idpAliase = identityProviderIds
                    .Where(id => id != null)
                    .Select(id => id!.Value)
                    .IfAny(async ids =>
                        {
                            var distinctIds = ids.Distinct();
                            var idpAliasData = await identityProviderRepository
                                .GetManagedIdentityProviderAliasDataUntracked(ownerCompanyId, distinctIds)
                                .ToDictionaryAsync(
                                    x => x.IdentityProviderId,
                                    x => x.Alias ?? throw new ConflictException($"identityProvider {x.IdentityProviderId} has no alias")).ConfigureAwait(false);
                            distinctIds.Except(idpAliasData.Keys).IfAny(invalidIds =>
                                throw new ControllerArgumentException($"Idps {string.Join("", invalidIds)} do not exist"));
                            return idpAliasData;
                        },
                        out var idpAliasDataTask)
                ? await idpAliasDataTask!.ConfigureAwait(false)
                : (IDictionary<Guid, string>?)null;

        var idpIds = idpAliase?.Keys ?? Enumerable.Empty<Guid>();
        var allIdpIds = singleIdpAlias == null
            ? idpIds
            : idpIds.Append(singleIdpAlias.Value.IdentityProviderId).Distinct();

        return (idpAliase, singleIdpAlias, allIdpIds);
    }

    private static void ValidateUsers(UserDetailData user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) || !Email.IsMatch(user.Email))
        {
            throw new ControllerArgumentException("User must have a valid email address");
        }

        if (string.IsNullOrWhiteSpace(user.FirstName) || !Name.IsMatch(user.FirstName))
        {
            throw new ControllerArgumentException("Firstname does not match expected format");
        }

        if (string.IsNullOrWhiteSpace(user.LastName) || !Name.IsMatch(user.LastName))
        {
            throw new ControllerArgumentException("Lastname does not match expected format");
        }
    }
}

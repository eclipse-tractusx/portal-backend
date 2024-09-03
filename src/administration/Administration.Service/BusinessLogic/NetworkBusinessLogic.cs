/********************************************************************************
 * Copyright (c) 2023 Contributors to the Eclipse Foundation
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Extensions;
using Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Identity;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Processes.NetworkRegistration.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using Org.Eclipse.TractusX.Portal.Backend.Registration.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class NetworkBusinessLogic(
    IPortalRepositories portalRepositories,
    IIdentityService identityService,
    IUserProvisioningService userProvisioningService,
    INetworkRegistrationProcessHelper processHelper,
    IOptions<PartnerRegistrationSettings> options)
    : INetworkBusinessLogic
{
    private static readonly Regex Name = new(ValidationExpressions.Name, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex ExternalId = new("^[A-Za-z0-9\\-+_/,.]{6,36}$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private readonly IIdentityData _identityData = identityService.IdentityData;
    private readonly PartnerRegistrationSettings _settings = options.Value;

    public async Task HandlePartnerRegistration(PartnerRegistrationData data)
    {
        if (!data.Name.IsValidCompanyName())
        {
            throw ControllerArgumentException.Create(ValidationExpressionErrors.INCORRECT_COMPANY_NAME, [new ErrorParameter("name", "OrganisationName")]);
        }

        var ownerCompanyId = _identityData.CompanyId;
        var networkRepository = portalRepositories.GetInstance<INetworkRepository>();
        var companyRepository = portalRepositories.GetInstance<ICompanyRepository>();
        var processStepRepository = portalRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();
        var identityProviderRepository = portalRepositories.GetInstance<IIdentityProviderRepository>();

        var (roleData, identityProviderIdAliase, singleIdentityProviderIdAlias, allIdentityProviderIds) = await ValidatePartnerRegistrationData(data, networkRepository, identityProviderRepository, ownerCompanyId).ConfigureAwait(ConfigureAwaitOptions.None);

        var companyId = CreatePartnerCompany(companyRepository, data);

        var applicationId = portalRepositories.GetInstance<IApplicationRepository>().CreateCompanyApplication(companyId, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.EXTERNAL,
            ca =>
            {
                ca.OnboardingServiceProviderId = ownerCompanyId;
            }).Id;

        var processId = processStepRepository.CreateProcess(ProcessTypeId.PARTNER_REGISTRATION).Id;
        processStepRepository.CreateProcessStepRange(new[]
        {
            (ProcessStepTypeId.SYNCHRONIZE_USER, ProcessStepStatusId.TODO, processId),
            (ProcessStepTypeId.MANUAL_DECLINE_OSP, ProcessStepStatusId.TODO, processId)
        });

        networkRepository.CreateNetworkRegistration(data.ExternalId, companyId, processId, ownerCompanyId, applicationId);

        identityProviderRepository.CreateCompanyIdentityProviders(allIdentityProviderIds.Select(identityProviderId => (companyId, identityProviderId)));

        Guid GetIdpId(Guid? identityProviderId) =>
            identityProviderId ?? (singleIdentityProviderIdAlias?.IdentityProviderId ?? throw new UnexpectedConditionException("singleIdentityProviderIdAlias should never be null here"));

        string GetIdpAlias(Guid? identityProviderId) =>
            identityProviderId == null
                ? singleIdentityProviderIdAlias?.Alias ?? throw new UnexpectedConditionException("singleIdentityProviderIdAlias should never be null here")
                : identityProviderIdAliase?[identityProviderId.Value] ?? throw new UnexpectedConditionException("identityProviderIdAliase should never be null here and should always contain an entry for identityProviderId");

        async IAsyncEnumerable<(Guid CompanyUserId, Exception? Error)> CreateUsers()
        {
            var userRepository = portalRepositories.GetInstance<IUserRepository>();
            await foreach (var (aliasData, creationInfos) in GetUserCreationData(companyId, GetIdpId, GetIdpAlias, data, roleData).ToAsyncEnumerable())
            {
                foreach (var creationInfo in creationInfos)
                {
                    var identityId = Guid.Empty;
                    Exception? error = null;
                    try
                    {
                        var (_, companyUserId) = await userProvisioningService.GetOrCreateCompanyUser(userRepository, aliasData.IdpAlias,
                            creationInfo, companyId, aliasData.IdpId, data.BusinessPartnerNumber?.ToUpper()).ConfigureAwait(ConfigureAwaitOptions.None);
                        identityId = companyUserId;
                        portalRepositories.GetInstance<IApplicationRepository>().CreateInvitation(applicationId, companyUserId);
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

        await portalRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
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
            c.BusinessPartnerNumber = data.BusinessPartnerNumber?.ToUpper();
        });

        companyRepository.CreateUpdateDeleteIdentifiers(company.Id, Enumerable.Empty<(UniqueIdentifierId, string)>(), data.UniqueIds.Select(x => (x.UniqueIdentifierId, x.Value)));
        portalRepositories.GetInstance<ICompanyRolesRepository>().CreateCompanyAssignedRoles(company.Id, data.CompanyRoles);

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
                    data.BusinessPartnerNumber,
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

    public Task RetriggerProcessStep(string externalId, ProcessStepTypeId processStepTypeId) =>
        processHelper.TriggerProcessStep(externalId, processStepTypeId);

    private async Task<(IEnumerable<UserRoleData> RoleData, IDictionary<Guid, string>? IdentityProviderIdAliase, (Guid IdentityProviderId, string Alias)? SingleIdentityProviderIdAlias, IEnumerable<Guid> AllIdentityProviderIds)> ValidatePartnerRegistrationData(PartnerRegistrationData data, INetworkRepository networkRepository, IIdentityProviderRepository identityProviderRepository, Guid ownerCompanyId)
    {
        var countryRepository = portalRepositories.GetInstance<ICountryRepository>();
        data.ValidateData();
        await data.ValidateDatabaseData(
            bpn => portalRepositories.GetInstance<ICompanyRepository>().CheckBpnExists(bpn),
            alpha2Code => countryRepository.CheckCountryExistsByAlpha2CodeAsync(alpha2Code),
            (countryAlpha2Code, uniqueIdentifierIds) => countryRepository.GetCountryAssignedIdentifiers(countryAlpha2Code, uniqueIdentifierIds),
            true).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!data.CompanyRoles.Any())
        {
            throw new ControllerArgumentException("At least one company role must be selected", nameof(data.CompanyRoles));
        }

        foreach (var user in data.UserDetails)
        {
            ValidateUsers(user);
        }

        if (!ExternalId.IsMatch(data.ExternalId))
        {
            throw new ControllerArgumentException("ExternalId must be between 6 and 36 characters");
        }

        if (await networkRepository.CheckExternalIdExists(data.ExternalId, ownerCompanyId)
                .ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ControllerArgumentException($"ExternalId {data.ExternalId} already exists", nameof(data.ExternalId));
        }

        var idpResult = await ValidateIdps(data, identityProviderRepository, ownerCompanyId).ConfigureAwait(ConfigureAwaitOptions.None);

        IEnumerable<UserRoleData> roleData;
        try
        {
            roleData = await userProvisioningService.GetRoleDatas(_settings.InitialRoles).ToListAsync().ConfigureAwait(false);
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
                var single = await identityProviderRepository.GetSingleManagedIdentityProviderAliasDataUntracked(ownerCompanyId).ConfigureAwait(ConfigureAwaitOptions.None);
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
                ? await idpAliasDataTask.ConfigureAwait(ConfigureAwaitOptions.None)
                : default(IDictionary<Guid, string>?);

        var idpIds = idpAliase?.Keys ?? Enumerable.Empty<Guid>();
        var allIdpIds = singleIdpAlias == null
            ? idpIds
            : idpIds.Append(singleIdpAlias.Value.IdentityProviderId).Distinct();

        return (idpAliase, singleIdpAlias, allIdpIds);
    }

    private static void ValidateUsers(UserDetailData user)
    {
        if (string.IsNullOrWhiteSpace(user.Email) || !new EmailAddressAttribute().IsValid(user.Email))
        {
            throw new ControllerArgumentException($"Mail {user.Email} must not be empty and have valid format");
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

    public Task<Pagination.Response<CompanyDetailsOspOnboarding>> GetOspCompanyDetailsAsync(int page, int size, CompanyApplicationStatusFilter? companyApplicationStatusFilter, string? companyName, string? externalId, DateCreatedOrderFilter? dateCreatedOrderFilter)
    {
        if (companyName != null && !companyName.IsValidCompanyName())
        {
            throw ControllerArgumentException.Create(ValidationExpressionErrors.INCORRECT_COMPANY_NAME, [new ErrorParameter("name", "CompanyName")]);
        }

        var applicationsQuery = portalRepositories.GetInstance<IApplicationRepository>()
            .GetExternalCompanyApplicationsFilteredQuery(_identityData.CompanyId,
                companyName?.Length >= 3 ? companyName : null, externalId,
                companyApplicationStatusFilter.GetCompanyApplicationStatusIds());

        var orderedQuery = dateCreatedOrderFilter == null || dateCreatedOrderFilter.Value == DateCreatedOrderFilter.DESC
            ? applicationsQuery.AsSplitQuery().OrderByDescending(application => application.DateCreated)
            : applicationsQuery.AsSplitQuery().OrderBy(application => application.DateCreated);

        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<CompanyDetailsOspOnboarding>(
                applicationsQuery.CountAsync(),
                orderedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(application => new CompanyDetailsOspOnboarding(
                        application.CompanyId,
                        application.NetworkRegistration!.ExternalId,
                        application.Id,
                        application.ApplicationStatusId,
                        application.DateCreated,
                        application.Company!.DateCreated,
                        application.DateLastChanged,
                        application.Company!.Name,
                        application.Company.CompanyAssignedRoles.Select(companyAssignedRoles => companyAssignedRoles.CompanyRoleId),
                        application.Company.IdentityProviders.Select(x => new IdentityProvidersDetails(x.Id, x.IamIdentityProvider!.IamIdpAlias)),
                        application.Company.BusinessPartnerNumber,
                        application.Company.Identities.Count(x => x.CompanyUser!.Identity!.UserStatusId != UserStatusId.DELETED)))
                    .AsAsyncEnumerable()));
    }
}

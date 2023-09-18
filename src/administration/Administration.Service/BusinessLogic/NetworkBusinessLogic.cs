/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
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
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library;
using Org.Eclipse.TractusX.Portal.Backend.OnboardingServiceProvider.Library.Models;
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
    private readonly IMailingService _mailingService;
    private readonly IOnboardingServiceProviderBusinessLogic _onboardingServiceProviderBusinessLogic;
    private readonly PartnerRegistrationSettings _settings;

    public NetworkBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IUserProvisioningService userProvisioningService, INetworkRegistrationProcessHelper processHelper, IOnboardingServiceProviderBusinessLogic onboardingServiceProviderBusinessLogic, IMailingService mailingService, IOptions<PartnerRegistrationSettings> options)
    {
        _portalRepositories = portalRepositories;
        _identityService = identityService;
        _userProvisioningService = userProvisioningService;
        _processHelper = processHelper;
        _mailingService = mailingService;
        _onboardingServiceProviderBusinessLogic = onboardingServiceProviderBusinessLogic;
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

        var (_, companyName) = await companyRepository.GetCompanyNameUntrackedAsync(ownerCompanyId).ConfigureAwait(false);

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

        async IAsyncEnumerable<(Guid CompanyUserId, string UserName, string? Password, Exception? Error)> CreateUsers()
        {
            foreach (var user in GetUserCreationData(companyId, GetIdpId, GetIdpAlias, data, roleData))
            {
                await foreach (var result in _userProvisioningService.CreateOwnCompanyIdpUsersAsync(user.AliasData, user.CreationInfos.ToAsyncEnumerable()))
                {
                    yield return result;
                }
            }
        }

        var userCreationErrors = await CreateUsers().Where(x => x.Error != null).Select(x => x.Error!).ToListAsync();
        userCreationErrors.IfAny(errors => throw new ServiceException($"Errors occured while saving the users: ${string.Join("", errors.Select(x => x.Message))}", errors.First()));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        await SendMails(data.UserDetails.Select(x => new ValueTuple<string, string?, string?>(x.Email, x.FirstName, x.LastName)), companyName).ConfigureAwait(false);
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

    private async Task SendMails(IEnumerable<(string Email, string? FirstName, string? LastName)> companyUserWithRoleIdForCompany, string ospName)
    {
        foreach (var (receiver, firstName, lastName) in companyUserWithRoleIdForCompany)
        {
            var userName = string.Join(" ", firstName, lastName);
            var mailParameters = new Dictionary<string, string>
            {
                { "userName", !string.IsNullOrWhiteSpace(userName) ?  userName : receiver },
                { "hostname", _settings.BasePortalAddress },
                { "osp", ospName },
                { "url", _settings.BasePortalAddress }
            };
            await _mailingService.SendMails(receiver, mailParameters, Enumerable.Repeat("OspWelcomeMail", 1)).ConfigureAwait(false);
        }
    }

    public Task RetriggerSynchronizeUser(Guid externalId, ProcessStepTypeId processStepTypeId) =>
        _processHelper.TriggerProcessStep(externalId, processStepTypeId);

    private async Task<(IEnumerable<UserRoleData> RoleData, IDictionary<Guid, string>? IdentityProviderIdAliase, (Guid IdentityProviderId, string Alias)? SingleIdentityProviderIdAlias, IEnumerable<Guid> AllIdentityProviderIds)> ValidatePartnerRegistrationData(PartnerRegistrationData data, INetworkRepository networkRepository, IIdentityProviderRepository identityProviderRepository, Guid ownerCompanyId)
    {
        if (string.IsNullOrWhiteSpace(data.Bpn) || !BpnRegex.IsMatch(data.Bpn))
        {
            throw new ControllerArgumentException("BPN must contain exactly 16 characters and must be prefixed with BPNL", nameof(data.Bpn));
        }

        if (await _portalRepositories.GetInstance<ICompanyRepository>().CheckBpnExists(data.Bpn).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"The Bpn {data.Bpn} already exists", nameof(data.Bpn));
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
    
    public async Task Submit(IEnumerable<CompanyRoleConsentDetails> companyRoleConsentDetails, CancellationToken cancellationToken)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        var userId = _identityService.IdentityData.UserId;
        var userRoleIds = await _portalRepositories.GetInstance<IUserRolesRepository>()
            .GetUserRoleIdsUntrackedAsync(_settings.InitialRoles)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var data = await _portalRepositories.GetInstance<INetworkRepository>()
            .GetSubmitData(companyId, userId, userRoleIds)
            .ConfigureAwait(false);
        if (!data.Exists)
        {
            throw new NotFoundException($"Company {companyId} not found");
        }

        if (!data.IsUserInRole)
        {
            throw new ForbiddenException($"User must be in role {string.Join(",", _settings.InitialRoles.SelectMany(x => x.UserRoleNames))}");
        }

        if (data.CompanyApplications.Count() != 1)
        {
            throw new ConflictException($"Company {companyId} has no or more than one application");
        }

        if (data.ProcessId == null)
        {
            throw new ConflictException("There must be an process");
        }

        var companyApplication = data.CompanyApplications.Single();
        if (companyApplication.StatusId != CompanyApplicationStatusId.CREATED)
        {
            throw new ConflictException($"Application {companyApplication.Id} is not in state CREATED");
        }
        
        var allCompanyRolesMatched = data.CompanyRoleIds
            .Select(companyRole => companyRoleConsentDetails
                .Any(role =>
                    role.CompanyRole == companyRole.CompanyRoleId &&
                    companyRole.AgreementIds.All(agreementId =>
                        role.Agreements.Any(agreement =>
                            agreement.AgreementId == agreementId &&
                            agreement.ConsentStatus == ConsentStatusId.ACTIVE))))
            .All(allAgreementsActive => allAgreementsActive);
        if (!allCompanyRolesMatched)
        {
            throw new ConflictException("All Agreements for the company roles must be agreed to");
        }

        foreach (var (agreementId, consentStatus) in companyRoleConsentDetails.SelectMany(x => x.Agreements)
                     .DistinctBy(a => a.AgreementId).Select(x => (x.AgreementId, x.ConsentStatus)))
        {
            _portalRepositories.GetInstance<IConsentRepository>()
                .CreateConsent(agreementId, companyId, userId, consentStatus);
        }

        _portalRepositories.GetInstance<IApplicationRepository>().AttachAndModifyCompanyApplication(companyApplication.Id,
            ca =>
            {
                ca.ApplicationStatusId = CompanyApplicationStatusId.SUBMITTED;
            });
        _portalRepositories.GetInstance<IProcessStepRepository>().CreateProcessStepRange(Enumerable.Repeat(new ValueTuple<ProcessStepTypeId, ProcessStepStatusId, Guid>(ProcessStepTypeId.TRIGGER_CALLBACK_OSP_SUBMITTED, ProcessStepStatusId.TODO, data.ProcessId.Value), 1));

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }
}

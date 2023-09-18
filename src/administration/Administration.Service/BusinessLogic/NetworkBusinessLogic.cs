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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Mailing.SendMail;
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
    private readonly PartnerRegistrationSettings _settings;

    public NetworkBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IUserProvisioningService userProvisioningService, INetworkRegistrationProcessHelper processHelper, IMailingService mailingService, IOptions<PartnerRegistrationSettings> options)
    {
        _portalRepositories = portalRepositories;
        _identityService = identityService;
        _userProvisioningService = userProvisioningService;
        _processHelper = processHelper;
        _mailingService = mailingService;
        _settings = options.Value;
    }

    public async Task HandlePartnerRegistration(PartnerRegistrationData data)
    {
        var ownerCompanyId = _identityService.IdentityData.CompanyId;
        var networkRepository = _portalRepositories.GetInstance<INetworkRepository>();
        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
        var roleData = await ValidatePartnerRegistrationData(data, networkRepository, companyRepository).ConfigureAwait(false);
        var (_, companyName) = await companyRepository.GetCompanyNameUntrackedAsync(ownerCompanyId).ConfigureAwait(false);

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

        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.PARTNER_REGISTRATION).Id;
        processStepRepository.CreateProcessStepRange(Enumerable.Repeat(new ValueTuple<ProcessStepTypeId, ProcessStepStatusId, Guid>(ProcessStepTypeId.SYNCHRONIZE_USER, ProcessStepStatusId.TODO, processId), 1));

        var applicationId = _portalRepositories.GetInstance<IApplicationRepository>().CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.EXTERNAL,
            ca =>
            {
                ca.OnboardingServiceProviderId = ownerCompanyId;
            }).Id;

        networkRepository.CreateNetworkRegistration(data.ExternalId, company.Id, processId, ownerCompanyId, applicationId);

        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();

        var idps = await identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(ownerCompanyId).ToListAsync().ConfigureAwait(false);
        var userData = data.UserDetails
            .SelectMany(x => x.IdentityProviderLinks
                .Select(idp => new UserTransferData(
                    x.FirstName,
                    x.LastName,
                    x.Email,
                    idp.IdentityProviderId ?? idps.Single().IdentityProviderId,
                    idp.ProviderId,
                    idp.Username)))
            .GroupBy(x => x.IdentityProviderId)
            .Select(group =>
            {
                var idpAlias = idps.Single(x => x.IdentityProviderId == group.Key).Alias;
                if (idpAlias == null)
                {
                    return new ValueTuple<bool, CompanyNameIdpAliasData, IEnumerable<UserCreationRoleDataIdpInfo>>();
                }

                var companyNameIdpAliasData = new CompanyNameIdpAliasData(
                    company.Id,
                    data.Name,
                    data.Bpn,
                    idpAlias,
                    group.Key,
                    false
                );

                var userCreationInfos = group.Select(user =>
                    new UserCreationRoleDataIdpInfo(
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        roleData,
                        user.Username,
                        user.ProviderId
                    )
                );

                return new ValueTuple<bool, CompanyNameIdpAliasData, IEnumerable<UserCreationRoleDataIdpInfo>>(true, companyNameIdpAliasData, userCreationInfos);
            });

        var errors = new List<Exception>();
        foreach (var user in userData.Where(x => x.Item1))
        {
            var result = await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(user.Item2, user.Item3.ToAsyncEnumerable()).ToListAsync().ConfigureAwait(false);
            var exceptions = result.Where(x => x.Error != null).Select(x => x.Error!);
            errors.AddRange(exceptions);
        }

        if (errors.Any())
        {
            throw new UnexpectedConditionException($"Errors occured while saving the users: ${string.Join("", errors.Select(x => x.Message))}", errors.First());
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
        await SendMails(data.UserDetails.Select(x => new ValueTuple<string, string?, string?>(x.Email, x.FirstName, x.LastName)), companyName).ConfigureAwait(false);
    }

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

    private async Task<IEnumerable<UserRoleData>> ValidatePartnerRegistrationData(PartnerRegistrationData data, INetworkRepository networkRepository, ICompanyRepository companyRepository)
    {
        if (string.IsNullOrWhiteSpace(data.Bpn) || !BpnRegex.IsMatch(data.Bpn))
        {
            throw new ControllerArgumentException("BPN must contain exactly 16 characters and must be prefixed with BPNL", nameof(data.Bpn));
        }

        if (!data.CompanyRoles.Any())
        {
            throw new ControllerArgumentException("At least one company role must be selected", nameof(data.CompanyRoles));
        }

        foreach (var user in data.UserDetails)
        {
            ValidateUsers(user);
        }

        if (await networkRepository.CheckExternalIdExists(data.ExternalId)
                .ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"ExternalId {data.ExternalId} already exists", nameof(data.ExternalId));
        }

        if (!await _portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(data.CountryAlpha2Code).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Location {data.CountryAlpha2Code} does not exist", nameof(data.CountryAlpha2Code));
        }

        await ValidateIdps(data, companyRepository).ConfigureAwait(false);

        try
        {
            return await _userProvisioningService.GetRoleDatas(_settings.InitialRoles).ToListAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ConfigurationException($"{nameof(_settings.InitialRoles)}: {e.Message}");
        }
    }

    private async Task ValidateIdps(PartnerRegistrationData data, ICompanyRepository companyRepository)
    {
        var identityProviderIds = data.UserDetails
            .SelectMany(x => x.IdentityProviderLinks.Select(y => y.IdentityProviderId)).Distinct();
        var idps = await companyRepository.GetLinkedIdpIds(_identityService.IdentityData.CompanyId)
            .ToListAsync()
            .ConfigureAwait(false);
        if (identityProviderIds.Any(x => x == null) && idps.Count != 1)
        {
            throw new ControllerArgumentException(
                "Company has more than one identity provider linked, therefor identityProviderId must be set for all users",
                nameof(data.UserDetails));
        }

        if (idps.All(x => !identityProviderIds.Where(y => y != null).Contains(x)))
        {
            throw new ControllerArgumentException(
                $"Idps {string.Join("", idps.Where(x => !identityProviderIds.Any(i => i == x)))} do not exist");
        }
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

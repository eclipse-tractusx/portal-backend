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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Async;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Identities;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Models;
using Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Service;
using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.BusinessLogic;

public class NetworkBusinessLogic : INetworkBusinessLogic
{
    private static readonly Regex name = new(@"^(([A-Za-zÀ-ÿ]{1,40}?([-,.'\s]?[A-Za-zÀ-ÿ]{1,40}?)){1,8})$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex email = new(@"^(([^<>()[\]\\.,;:\s@""]+(\.[^<>()[\]\\.,;:\s@""]+)*)|("".+""))@((\[\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\])|(([a-z0-9-]+\.)+[a-z]{2,}))$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    private static readonly Regex bpnRegex = new(@"^BPNL[\w|\d]{12}$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private readonly IPortalRepositories _portalRepositories;
    private readonly IIdentityService _identityService;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly PartnerRegistrationSettings _settings;

    public NetworkBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IUserProvisioningService userProvisioningService, IOptions<PartnerRegistrationSettings> options)
    {
        _portalRepositories = portalRepositories;
        _identityService = identityService;
        _userProvisioningService = userProvisioningService;
        _settings = options.Value;
    }

    public async Task HandlePartnerRegistration(PartnerRegistrationData data)
    {
        var companyId = _identityService.IdentityData.CompanyId;
        await ValidatePartnerRegistrationData(data).ConfigureAwait(false);

        IEnumerable<UserRoleData> roleDatas;
        try
        {
            roleDatas = await _userProvisioningService.GetRoleDatas(_settings.InitialRoles).ToListAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ConfigurationException($"{nameof(_settings.InitialRoles)}: {e.Message}");
        }

        var companyRepository = _portalRepositories.GetInstance<ICompanyRepository>();
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

        companyRepository.CreateUpdateDeleteIdentifiers(company.Id, data.UniqueIds, Enumerable.Empty<(UniqueIdentifierId, string)>());
        _portalRepositories.GetInstance<ICompanyRolesRepository>().CreateCompanyAssignedRoles(company.Id, data.CompanyRoles);

        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        applicationRepository.CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.EXTERNAL,
            ca =>
            {
                ca.OnboardingServiceProviderId = companyId;
            });

        var processStepRepository = _portalRepositories.GetInstance<IProcessStepRepository>();
        var process = processStepRepository.CreateProcess(ProcessTypeId.PARTNER_REGISTRATION);

        var userRepository = _portalRepositories.GetInstance<IUserRepository>();
        var userRolesRepository = _portalRepositories.GetInstance<IUserRolesRepository>();
        var businessPartnerRepository = _portalRepositories.GetInstance<IUserBusinessPartnerRepository>();

        var idps = await identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(companyId).ToListAsync().ConfigureAwait(false);
        foreach (var user in data.UserDetails)
        {

            var identity = userRepository.CreateIdentity(companyId, UserStatusId.PENDING, IdentityTypeId.COMPANY_USER);
            var companyUserId = userRepository.CreateCompanyUser(identity.Id, user.FirstName, user.LastName, user.Email).Id;
            if (data.Bpn != null)
            {
                businessPartnerRepository.CreateCompanyUserAssignedBusinessPartner(companyUserId, data.Bpn);
            }

            foreach (var role in roleDatas)
            {
                userRolesRepository.CreateIdentityAssignedRole(companyUserId, role.UserRoleId);
            }

            foreach (var idpLink in user.IdentityProviderLinks)
            {
                var idpData = idps.Single(x => idpLink.IdentityProviderId == null || x.IdentityProviderId == idpLink.IdentityProviderId.Value);
                var processStepId = processStepRepository.CreateProcessStepRange(Enumerable.Repeat(new ValueTuple<ProcessStepTypeId, ProcessStepStatusId, Guid>(ProcessStepTypeId.SYNCHRONIZE_USER, ProcessStepStatusId.TODO, process.Id), 1)).Single().Id;
                userRepository.AddCompanyUserAssignedIdentityProvider(companyUserId, idpData.IdentityProviderId, idpLink.ProviderId, idpLink.Username, processStepId);
            }
        }

        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    private async Task ValidatePartnerRegistrationData(PartnerRegistrationData data)
    {
        if (!string.IsNullOrWhiteSpace(data.Bpn) && !bpnRegex.IsMatch(data.Bpn))
        {
            throw new ControllerArgumentException("BPN must contain exactly 16 characters and must be prefixed with BPNL", nameof(data.Bpn));
        }

        if (!await _portalRepositories.GetInstance<ICountryRepository>()
                .CheckCountryExistsByAlpha2CodeAsync(data.CountryAlpha2Code).ConfigureAwait(false))
        {
            throw new ControllerArgumentException($"Location {data.CountryAlpha2Code} does not exist", nameof(data.CountryAlpha2Code));
        }

        foreach (var user in data.UserDetails)
        {
            if (!email.IsMatch(user.Email))
            {
                throw new ControllerArgumentException("BPN must contain exactly 16 characters and must be prefixed with BPNL", nameof(data.Bpn));
            }

            if (!name.IsMatch(user.FirstName))
            {
                throw new ControllerArgumentException("Firstname does not match expected format");
            }

            if (!name.IsMatch(user.LastName))
            {
                throw new ControllerArgumentException("Lastname does not match expected format");
            }
        }

        var identityProviderIds = data.UserDetails.SelectMany(x => x.IdentityProviderLinks.Select(y => y.IdentityProviderId)).Distinct();
        var idps = await _portalRepositories
            .GetInstance<ICompanyRepository>().GetLinkedIdpCount(_identityService.IdentityData.CompanyId, identityProviderIds)
            .ToListAsync()
            .ConfigureAwait(false);
        if (idps.Count != identityProviderIds.Count(x => x != null))
        {
            throw new ControllerArgumentException($"Idps {string.Join("", idps.Where(x => !identityProviderIds.Any(i => i == x)))} do not exist");
        }

        if (data.CompanyRoles.Any())
        {
            throw new ControllerArgumentException("At least one company role must be selected", nameof(data.CompanyRoles));
        }
    }
}

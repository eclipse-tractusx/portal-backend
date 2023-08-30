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
    private readonly IProvisioningManager _provisioningManager;
    private readonly IUserProvisioningService _userProvisioningService;
    private readonly PartnerRegistrationSettings _settings;

    public NetworkBusinessLogic(IPortalRepositories portalRepositories, IIdentityService identityService, IProvisioningManager provisioningManager, IUserProvisioningService userProvisioningService, IOptions<PartnerRegistrationSettings> options)
    {
        _portalRepositories = portalRepositories;
        _identityService = identityService;
        _provisioningManager = provisioningManager;
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

        // TODO (PS): clarify how to handle userDetails
        var identityProviderIds = data.UserDetails.GroupBy(x => x.IdentityProviderId).Select(x => x.Key).Distinct();
        var identityProviderRepository = _portalRepositories.GetInstance<IIdentityProviderRepository>();
        foreach (var idp in identityProviderIds)
        {
            var identityProviderId = idp;
            if (idp == null)
            {
                var idpData = await identityProviderRepository.GetCompanyIdentityProviderCategoryDataUntracked(companyId).SingleAsync().ConfigureAwait(false);
                identityProviderId = idpData.IdentityProviderId;
            }
            
            var users = data.UserDetails
                .Where(x => x.IdentityProviderId == identityProviderId)
                .Select(user => new UserCreationRoleDataIdpInfo(
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    roleDatas,
                    user.Email,
                    user.ProviderId
                )).ToAsyncEnumerable();
            await _userProvisioningService.CreateOwnCompanyIdpUsersAsync(new CompanyNameIdpAliasData(company.Id, company.Name, company.BusinessPartnerNumber, "idp", false), users).AwaitAll().ConfigureAwait(false); // TODO (PS): clarify idp alias

            var identityProvider = identityProviderRepository.CreateIdentityProvider(IdentityProviderCategoryId.KEYCLOAK_OIDC, IdentityProviderTypeId.MANAGED, companyId);
            identityProvider.Companies.Add(company);
            identityProviderRepository.CreateIamIdentityProvider(identityProvider.Id, "idpName"); // TODO (PS): clarify idpName
        }

        var applicationRepository = _portalRepositories.GetInstance<IApplicationRepository>();
        applicationRepository.CreateCompanyApplication(company.Id, CompanyApplicationStatusId.CREATED, CompanyApplicationTypeId.EXTERNAL,
            ca =>
            {
                ca.OnboardingServiceProviderId = companyId;
            });

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

        if (data.UserDetails.Any(x => x.IdentityProviderId == null) &&
            await _portalRepositories
                .GetInstance<ICompanyRepository>().GetLinkedIdpCount(_identityService.IdentityData.CompanyId)
                .ConfigureAwait(false) != 1)
        {
            throw new ControllerArgumentException(
                "Company has more than one identity provider linked, therefor identityProviderId must be set for all users",
                nameof(data.UserDetails));
        }

        if (data.CompanyRoles.Any())
        {
            throw new ControllerArgumentException("At least one company role must be selected", nameof(data.CompanyRoles));
        }
    }
}

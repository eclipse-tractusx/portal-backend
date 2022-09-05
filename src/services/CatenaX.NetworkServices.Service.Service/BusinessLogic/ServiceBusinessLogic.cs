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

using CatenaX.NetworkServices.Framework.ErrorHandling;
using CatenaX.NetworkServices.Framework.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CatenaX.NetworkServices.Service.Service.BusinessLogic;

/// <summary>
/// Implementation of <see cref="IServiceBusinessLogic"/>.
/// </summary>
public class ServiceBusinessLogic : IServiceBusinessLogic
{
    private readonly IPortalRepositories _portalRepositories;
    private readonly ServiceSettings _settings;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="portalRepositories">Factory to access the repositories</param>
    /// <param name="settings">Access to the settings</param>
    public ServiceBusinessLogic(IPortalRepositories portalRepositories, IOptions<ServiceSettings> settings)
    {
        _portalRepositories = portalRepositories;
        _settings = settings.Value;
    }

    /// <inheritdoc />
    public Task<Pagination.Response<ServiceDetailData>> GetAllActiveServicesAsync(int page, int size)
    {
        var services = _portalRepositories.GetInstance<IOfferRepository>().GetActiveServices();
        return Pagination.CreateResponseAsync(
            page,
            size,
            _settings.ApplicationsMaxPageSize,
            (skip, take) => new Pagination.AsyncSource<ServiceDetailData>(
                services.CountAsync(),
                services
                    .Skip(skip)
                    .Take(take)
                    .Select(s =>
                        new ServiceDetailData(
                            s.id,
                            s.name ?? Constants.ErrorString,
                            s.provider,
                            s.thumbnailUrl ?? Constants.ErrorString,
                            s.contactEmail,
                            null,
                            s.price ?? Constants.ErrorString))
                    .AsAsyncEnumerable()));
    }

    /// <inheritdoc />
    public async Task<Guid> CreateServiceOffering(ServiceOfferingData data, string iamUserId)
    {
        var results = await _portalRepositories.GetInstance<IUserRepository>()
            .GetCompanyUserWithIamUserCheckAndCompanyShortName(iamUserId, data.SalesManager)
            .ToListAsync();

        if (!results.Any(x => x.IsIamUser))
            throw new ControllerArgumentException($"IamUser is not assignable to company user {iamUserId}", nameof(iamUserId));

        if (string.IsNullOrWhiteSpace(results.Single(x => x.IsIamUser).CompanyShortName))
            throw new ControllerArgumentException($"No matching company found for user {iamUserId}", nameof(iamUserId));

        if (results.All(x => x.CompanyUserId != data.SalesManager))
            throw new ControllerArgumentException("SalesManager does not exist", nameof(data.SalesManager));

        await CheckLanguageCodesExist(data.Descriptions.Select(x => x.LanguageCode).ToList()).ConfigureAwait(false);

        var appRepository = _portalRepositories.GetInstance<IOfferRepository>();
        var app = appRepository.CreateApp(string.Empty, OfferTypeId.SERVICE, app =>
        {
            app.ContactEmail = data.ContactEmail;
            app.Name = data.Title;
            app.SalesManagerId = data.SalesManager;
            app.ThumbnailUrl = data.ThumbnailUrl;
            app.Provider = results.Single(x => x.IsIamUser).CompanyShortName;
            app.OfferStatusId = OfferStatusId.CREATED;
            app.ProviderCompanyId = results.Single(x => x.IsIamUser).CompanyId;
        });
        var licenseId = appRepository.CreateAppLicenses(data.Price).Id;
        appRepository.CreateAppAssignedLicense(app.Id, licenseId);
        appRepository.AddAppDescriptions(data.Descriptions.Select(d =>
            new ValueTuple<Guid, string, string, string>(app.Id, d.LanguageCode, string.Empty, d.Description)));

        await _portalRepositories.SaveAsync();
        return app.Id;
    }

    /// <inheritdoc />
    public async Task AddServiceSubscription(Guid serviceId, string iamUserId)
    {
        if (!await _portalRepositories.GetInstance<IOfferRepository>().CheckAppExistsById(serviceId).ConfigureAwait(false))
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
        }

        var (companyId, companyUserId) = await _portalRepositories.GetInstance<IUserRepository>().GetOwnCompanAndCompanyUseryId(iamUserId).ConfigureAwait(false);
        if (companyId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company assigned", nameof(iamUserId));
        }
        
        if (companyUserId == Guid.Empty)
        {
            throw new ControllerArgumentException($"User {iamUserId} has no company user assigned", nameof(iamUserId));
        }

        _portalRepositories.GetInstance<IOfferSubscriptionsRepository>().CreateCompanyAssignedApp(serviceId, companyId, OfferSubscriptionStatusId.PENDING, companyUserId, companyUserId);
        await _portalRepositories.SaveAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<ServiceDetailData> GetServiceDetailsAsync(Guid serviceId, string lang)
    {        
        var serviceDetailData = await _portalRepositories.GetInstance<IOfferRepository>().GetServiceDetailByIdUntrackedAsync(serviceId, lang).ConfigureAwait(false);
        if (serviceDetailData is null)
        {
            throw new NotFoundException($"Service {serviceId} does not exist");
        }

        return serviceDetailData;
    }

    private async Task CheckLanguageCodesExist(IEnumerable<string> languageCodes)
    {
        if (languageCodes.Any())
        {
            var foundLanguageCodes = await _portalRepositories.GetInstance<ILanguageRepository>()
                .GetLanguageCodesUntrackedAsync(languageCodes)
                .ToListAsync()
                .ConfigureAwait(false);
            var notFoundLanguageCodes = languageCodes.Except(foundLanguageCodes);
            if (notFoundLanguageCodes.Any())
            {
                throw new ControllerArgumentException(
                    $"Language code(s) {string.Join(",", notFoundLanguageCodes)} do(es) not exist",
                    nameof(languageCodes));
            }
        }
    }
}

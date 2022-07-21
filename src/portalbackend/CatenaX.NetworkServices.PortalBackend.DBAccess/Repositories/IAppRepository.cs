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

using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing apps on persistence layer.
/// </summary>
public interface IAppRepository
{
    /// <summary>
    /// Checks if an app with the given id exists in the persistence layer. 
    /// </summary>
    /// <param name="appId">Id of the app.</param>
    /// <returns><c>true</c> if an app exists on the persistence layer with the given id, <c>false</c> if not.</returns>
    public Task<bool> CheckAppExistsById(Guid appId);

    /// <summary>
    /// Retrieves app provider company details by app id.
    /// </summary>
    /// <param name="appId">ID of the app.</param>
    /// <returns>Tuple of provider company details.</returns>
    public Task<AppProviderDetailsData> GetAppProviderDetailsAsync(Guid appId);

    /// <summary>
    /// Get Client Name by App Id
    /// </summary>
    /// <param name="appId"></param>
    /// <returns>Client Name</returns>
    Task<string?> GetAppAssignedClientIdUntrackedAsync(Guid appId);

    /// <summary>
    /// Adds an app to the database
    /// </summary>
    /// <param name="id">Id of the app</param>
    /// <param name="provider">Provider of the app</param>
    App CreateApp(Guid id, string provider);

    /// <summary>
    /// Gets all active apps with an optional filtered with the languageShortName
    /// </summary>
    /// <param name="languageShortName">The optional language shortName</param>
    /// <returns>Returns a async enumerable of <see cref="AppData"/></returns>
    IAsyncEnumerable<AppData> GetAllActiveAppsAsync(string? languageShortName);

    /// <summary>
    /// Gets the details of an app by its id
    /// </summary>
    /// <param name="appId">Id of the application to get details for</param>
    /// <param name="iamUserId">OPTIONAL: iamUserId of the company the calling user belongs to</param>
    /// <param name="languageShortName">OPTIONAL: language shortName</param>
    /// <returns>Returns the details of the application</returns>
    Task<AppDetailsData> GetAppDetailsByIdAsync(Guid appId, string iamUserId, string? languageShortName);

    /// <summary>
    /// Adds an <see cref="AppLicense"/> to the database
    /// </summary>
    /// <param name="licenseText">Text of the license</param>
    AppLicense CreateAppLicenses(string licenseText);


    /// <summary>
    /// Adds an <see cref="AppAssignedLicense"/> to the database
    /// </summary>
    /// <param name="appId">Id of the application</param>
    /// <param name="appLicenseId">Id of the app license</param>
    AppAssignedLicense CreateAppAssignedLicense(Guid appId, Guid appLicenseId);

    /// <summary>
    /// Adds the given app favourite to the database
    /// </summary>
    /// <param name="appId">Id of the app</param>
    /// <param name="companyUserId">Id of the company User</param>
    CompanyUserAssignedAppFavourite CreateAppFavourite(Guid appId, Guid companyUserId);

    /// <summary>
    /// Adds <see cref="AppAssignedUseCase"/>s to the database
    /// </summary>
    /// <param name="useCases">The use cases that should be added to the database</param>
    void AddAppAssignedUseCases(IEnumerable<(Guid appId, Guid useCaseId)> appUseCases);

    /// <summary>
    /// Adds <see cref="AppDescription"/>s to the database
    /// </summary>
    /// <param name="appDescriptions">The app descriptions that should be added to the database</param>
    void AddAppDescriptions(IEnumerable<(Guid appId, string languageShortName, string descriptionLong, string descriptionShort)> appDescriptions);

    /// <summary>
    /// Adds <see cref="AppLanguage"/>s to the database
    /// </summary>
    /// <param name="appLanguages">The app languages that should be added to the database</param>
    void AddAppLanguages(IEnumerable<(Guid appId, string languageShortName)> appLanguages);
}

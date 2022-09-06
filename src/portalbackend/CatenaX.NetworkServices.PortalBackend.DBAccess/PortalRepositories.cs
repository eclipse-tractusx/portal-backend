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

using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.DBAccess.Repositories;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess;

public class PortalRepositories : IPortalRepositories
{
    private readonly PortalDbContext _dbContext;

    private static readonly IReadOnlyDictionary<Type, Func<PortalDbContext, Object>> _types = new Dictionary<Type, Func<PortalDbContext, Object>> {
        { typeof(IApplicationRepository), context => new ApplicationRepository(context) },
        { typeof(IAppRepository), context => new AppRepository(context) },
        { typeof(IAppReleaseRepository), context => new AppReleaseRepository(context) },
        { typeof(ICompanyAssignedAppsRepository), context => new CompanyAssignedAppsRepository(context) },
        { typeof(ICompanyRepository), context => new CompanyRepository(context) },
        { typeof(ICompanyRolesRepository), context => new CompanyRolesRepository(context) },
        { typeof(IConnectorsRepository), context => new ConnectorsRepository(context) },
        { typeof(IConsentRepository), context => new ConsentRepository(context) },
        { typeof(ICountryRepository), context => new CountryRepository(context) },
        { typeof(IDocumentRepository), context => new DocumentRepository(context) },
        { typeof(IIdentityProviderRepository), context => new IdentityProviderRepository(context) },
        { typeof(IInvitationRepository), context => new InvitationRepository(context) },
        { typeof(ILanguageRepository), context => new LanguageRepository(context) },
        { typeof(INotificationRepository), context => new NotificationRepository(context) },
        { typeof(IServiceAccountRepository), context => new ServiceAccountRepository(context) },
        { typeof(IStaticDataRepository), context => new StaticDataRepository(context) },
        { typeof(IUserBusinessPartnerRepository), context => new UserBusinessPartnerRepository(context) },
        { typeof(IUserRepository), context => new UserRepository(context) },
        { typeof(IUserRolesRepository), context => new UserRolesRepository(context) },
    };

    public PortalRepositories(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public RepositoryType GetInstance<RepositoryType>()
    {
        Object? repository = default;

        if (_types.TryGetValue(typeof(RepositoryType), out Func<PortalDbContext, Object>? createFunc))
        {
            repository = createFunc(_dbContext);
        }
        return (RepositoryType)(repository ?? throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}",nameof(RepositoryType)));
    }

    /// <inheritdoc />
    public TEntity Attach<TEntity>(TEntity entity, Action<TEntity>? setOptionalParameters = null)
        where TEntity : class
    {
        var attachedEntity = _dbContext.Attach(entity).Entity;
        setOptionalParameters?.Invoke(attachedEntity);

        return attachedEntity;
    }

    /// <inheritdoc />
    public TEntity Remove<TEntity>(TEntity entity)
        where TEntity : class
        => _dbContext.Remove(entity).Entity;

    public Task<int> SaveAsync() => _dbContext.SaveChangesAsync();
}

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
using System.Reflection;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess;

public class PortalRepositories : IPortalRepositories
{
    private readonly PortalDbContext _dbContext;

    private static readonly IReadOnlyDictionary<Type, Type> _types = new Dictionary<Type, Type> {
        { typeof(IApplicationRepository), typeof(ApplicationRepository) },
        { typeof(IAppRepository), typeof(AppRepository) },
        { typeof(ICompanyAssignedAppsRepository), typeof(CompanyAssignedAppsRepository) },
        { typeof(ICompanyRepository), typeof(CompanyRepository) },
        { typeof(ICompanyRolesRepository), typeof(CompanyRolesRepository) },
        { typeof(IConnectorsRepository), typeof(ConnectorsRepository) },
        { typeof(IConsentRepository), typeof(ConsentRepository) },
        { typeof(ICountryRepository), typeof(CountryRepository) },
        { typeof(IDocumentRepository), typeof(DocumentRepository) },
        { typeof(IIdentityProviderRepository), typeof(IdentityProviderRepository) },
        { typeof(INotificationRepository), typeof(NotificationRepository) },
        { typeof(IServiceAccountRepository), typeof(ServiceAccountRepository) },
        { typeof(IStaticDataRepository), typeof(StaticDataRepository) },
        { typeof(IUserBusinessPartnerRepository), typeof(UserBusinessPartnerRepository) },
        { typeof(IUserRepository), typeof(UserRepository) },
        { typeof(IUserRolesRepository), typeof(UserRolesRepository) },
    };

    public PortalRepositories(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public RepositoryType GetInstance<RepositoryType>()
    {
        RepositoryType? repository = default;

        if (_types.TryGetValue(typeof(RepositoryType), out Type? type))
        {
            repository = (RepositoryType?)
                Activator.CreateInstance(
                    _types[typeof(RepositoryType)],
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new [] { _dbContext },
                    null
                );
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

    private static T To<T>(dynamic value) => (T) value;
}

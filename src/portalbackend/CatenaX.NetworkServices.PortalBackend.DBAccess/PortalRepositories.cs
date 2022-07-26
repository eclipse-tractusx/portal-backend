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
using Microsoft.EntityFrameworkCore.Storage;

namespace CatenaX.NetworkServices.PortalBackend.DBAccess;

public class PortalRepositories : IPortalRepositories
{
    private readonly PortalDbContext _dbContext;

    public PortalRepositories(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    /// <inheritdoc />
    public TEntity Attach<TEntity>(TEntity entity)
        where TEntity : class
        => _dbContext.Attach(entity).Entity;

    /// <inheritdoc />
    public TEntity Remove<TEntity>(TEntity entity)
        where TEntity : class
        => _dbContext.Remove(entity).Entity;

    public RepositoryType GetInstance<RepositoryType>()
    {
        var repositoryType = typeof(RepositoryType);

        if (repositoryType == typeof(IAppRepository))
        {
            return To<RepositoryType>(new AppRepository(_dbContext));
        }
        if (repositoryType == typeof(IApplicationRepository))
        {
            return To<RepositoryType>(new ApplicationRepository(_dbContext));
        }
        if (repositoryType == typeof(ICompanyAssignedAppsRepository))
        {
            return To<RepositoryType>(new CompanyAssignedAppsRepository(_dbContext));
        }
        if (repositoryType == typeof(ICompanyRepository))
        {
            return To<RepositoryType>(new CompanyRepository(_dbContext));
        }
        if (repositoryType == typeof(ICompanyRolesRepository))
        {
            return To<RepositoryType>(new CompanyRolesRepository(_dbContext));
        }
        if (repositoryType == typeof(IConnectorsRepository))
        {
            return To<RepositoryType>(new ConnectorsRepository(_dbContext));
        }
        if (repositoryType == typeof(IConsentRepository))
        {
            return To<RepositoryType>(new ConsentRepository(_dbContext));
        }
        if (repositoryType == typeof(ICountryRepository))
        {
            return To<RepositoryType>(new CountryRepository(_dbContext));
        }
        if (repositoryType == typeof(IDocumentRepository))
        {
            return To<RepositoryType>(new DocumentRepository(_dbContext));
        }
        if (repositoryType == typeof(IIdentityProviderRepository))
        {
            return To<RepositoryType>(new IdentityProviderRepository(_dbContext));
        }
        if (repositoryType == typeof(IServiceAccountsRepository))
        {
            return To<RepositoryType>(new ServiceAccountRepository(_dbContext));
        }
        if (repositoryType == typeof(IUserBusinessPartnerRepository))
        {
            return To<RepositoryType>(new UserBusinessPartnerRepository(_dbContext));
        }
        if (repositoryType == typeof(IUserRepository))
        {
            return To<RepositoryType>(new UserRepository(_dbContext));
        }
        if (repositoryType == typeof(IUserRolesRepository))
        {
            return To<RepositoryType>(new UserRolesRepository(_dbContext));
        }
        if (repositoryType == typeof(IStaticDataRepository))
        {
            return To<RepositoryType>(new StaticDataRepository(_dbContext));
        }
        throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}",nameof(RepositoryType));
    }

    public Task<int> SaveAsync() => _dbContext.SaveChangesAsync();

    public Task<IDbContextTransaction> BeginTransactionAsync() => _dbContext.Database.BeginTransactionAsync();

    private static T To<T>(dynamic value) => (T) value;
}

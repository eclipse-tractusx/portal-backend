/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;

public class PortalRepositories : IPortalRepositories
{
    private readonly PortalDbContext _dbContext;

    private static readonly IReadOnlyDictionary<Type, Func<PortalDbContext, Object>> _types = new Dictionary<Type, Func<PortalDbContext, Object>> {
        { typeof(IAgreementRepository), context => new AgreementRepository(context) },
        { typeof(IApplicationRepository), context => new ApplicationRepository(context) },
        { typeof(IApplicationChecklistRepository), context => new ApplicationChecklistRepository(context) },
        { typeof(IAppInstanceRepository), context => new AppInstanceRepository(context) },
        { typeof(IAppSubscriptionDetailRepository), context => new AppSubscriptionDetailRepository(context) },
        { typeof(IClientRepository), context => new ClientRepository(context) },
        { typeof(ICompanyRepository), context => new CompanyRepository(context) },
        { typeof(ICompanyRolesRepository), context => new CompanyRolesRepository(context) },
        { typeof(IConsentAssignedOfferSubscriptionRepository), context => new ConsentAssignedOfferSubscriptionRepository(context) },
        { typeof(IConnectorsRepository), context => new ConnectorsRepository(context) },
        { typeof(IConsentRepository), context => new ConsentRepository(context) },
        { typeof(ICountryRepository), context => new CountryRepository(context) },
        { typeof(IDocumentRepository), context => new DocumentRepository(context) },
        { typeof(IIdentityProviderRepository), context => new IdentityProviderRepository(context) },
        { typeof(IInvitationRepository), context => new InvitationRepository(context) },
        { typeof(ILanguageRepository), context => new LanguageRepository(context) },
        { typeof(INotificationRepository), context => new NotificationRepository(context) },
        { typeof(IOfferRepository), context => new OfferRepository(context) },
        { typeof(IOfferSubscriptionsRepository), context => new OfferSubscriptionsRepository(context) },
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
    public TEntity Attach<TEntity>(TEntity entity, Action<TEntity>? setOptionalParameters = null) where TEntity : class
    {
        var attachedEntity = _dbContext.Attach(entity).Entity;
        setOptionalParameters?.Invoke(attachedEntity);

        return attachedEntity;
    }

    public void AttachRange<TEntity>(IEnumerable<TEntity> entities, Action<TEntity> setOptionalParameters) where TEntity : class
    {
        foreach (var entity in entities)
        {
            var attachedEntity = _dbContext.Attach(entity).Entity;
            setOptionalParameters.Invoke(attachedEntity);
        }
    }

    public IEnumerable<TEntity> AttachRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        foreach (var entity in entities)
        {
            yield return _dbContext.Attach(entity).Entity;
        }
    }

    /// <inheritdoc />
    public TEntity Remove<TEntity>(TEntity entity) where TEntity : class
        => _dbContext.Remove(entity).Entity;

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        => _dbContext.RemoveRange(entities);
    

    public Task<int> SaveAsync() => _dbContext.SaveChangesAsync();
}

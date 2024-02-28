/********************************************************************************
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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;

public class PortalRepositories : IPortalRepositories
{
    private readonly PortalDbContext _dbContext;

    private static KeyValuePair<Type, Func<PortalDbContext, object>> CreateTypeEntry<T>(Func<PortalDbContext, object> createFunc) => KeyValuePair.Create(typeof(T), createFunc);

    private static readonly IReadOnlyDictionary<Type, Func<PortalDbContext, object>> RepositoryTypes = ImmutableDictionary.CreateRange(new[]
    {
        CreateTypeEntry<IAgreementRepository>(context => new AgreementRepository(context)),
        CreateTypeEntry<IApplicationRepository>(context => new ApplicationRepository(context)),
        CreateTypeEntry<IApplicationChecklistRepository>(context => new ApplicationChecklistRepository(context)),
        CreateTypeEntry<IAppInstanceRepository>(context => new AppInstanceRepository(context)),
        CreateTypeEntry<IAppSubscriptionDetailRepository>(context => new AppSubscriptionDetailRepository(context)),
        CreateTypeEntry<IClientRepository>(context => new ClientRepository(context)),
        CreateTypeEntry<ICompanyRepository>(context => new CompanyRepository(context)),
        CreateTypeEntry<ICompanySsiDetailsRepository>(context => new CompanySsiDetailsRepository(context)),
        CreateTypeEntry<ICompanyRolesRepository>(context => new CompanyRolesRepository(context)),
        CreateTypeEntry<IConsentAssignedOfferSubscriptionRepository>(context => new ConsentAssignedOfferSubscriptionRepository(context)),
        CreateTypeEntry<IConnectorsRepository>(context => new ConnectorsRepository(context)),
        CreateTypeEntry<IConsentRepository>(context => new ConsentRepository(context)),
        CreateTypeEntry<ICountryRepository>(context => new CountryRepository(context)),
        CreateTypeEntry<IDocumentRepository>(context => new DocumentRepository(context)),
        CreateTypeEntry<IIdentityProviderRepository>(context => new IdentityProviderRepository(context)),
        CreateTypeEntry<IIdentityRepository>(context => new IdentityRepository(context)),
        CreateTypeEntry<IInvitationRepository>(context => new InvitationRepository(context)),
        CreateTypeEntry<ILanguageRepository>(context => new LanguageRepository(context)),
        CreateTypeEntry<INotificationRepository>(context => new NotificationRepository(context)),
        CreateTypeEntry<INetworkRepository>(context => new NetworkRepository(context)),
        CreateTypeEntry<IOfferRepository>(context => new OfferRepository(context)),
        CreateTypeEntry<IOfferSubscriptionsRepository>(context => new OfferSubscriptionsRepository(context)),
        CreateTypeEntry<IProcessStepRepository>(context => new ProcessStepRepository(context)),
        CreateTypeEntry<IServiceAccountRepository>(context => new ServiceAccountRepository(context)),
        CreateTypeEntry<IStaticDataRepository>(context => new StaticDataRepository(context)),
        CreateTypeEntry<ITechnicalUserProfileRepository>(context => new TechnicalUserProfileRepository(context)),
        CreateTypeEntry<IUserBusinessPartnerRepository>(context => new UserBusinessPartnerRepository(context)),
        CreateTypeEntry<IUserRepository>(context => new UserRepository(context)),
        CreateTypeEntry<IUserRolesRepository>(context => new UserRolesRepository(context)),
        CreateTypeEntry<ICompanyCertificateRepository>(context => new CompanyCertificateRepository(context)),
    });

    public PortalRepositories(PortalDbContext portalDbContext)
    {
        _dbContext = portalDbContext;
    }

    public RepositoryType GetInstance<RepositoryType>()
    {
        object? repository = default;

        if (RepositoryTypes.TryGetValue(typeof(RepositoryType), out var createFunc))
        {
            repository = createFunc(_dbContext);
        }
        return (RepositoryType)(repository ?? throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}", nameof(RepositoryType)));
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
        foreach (var attachedEntity in entities.Select(entity => _dbContext.Attach(entity).Entity))
        {
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

    public Task<int> SaveAsync()
    {
        try
        {
            return _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConflictException("while processing a concurrent update was saved to the database (reason could also be data to be deleted is no longer existing)", e);
        }
    }
    public void Clear() => _dbContext.ChangeTracker.Clear();
}

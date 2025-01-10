/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Context;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Context;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess;

public class PortalRepositories(PortalDbContext portalDbContext)
    : ProcessRepositories<ProcessTypeId, ProcessStepTypeId>(portalDbContext), IPortalRepositories
{
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
        CreateTypeEntry<ICompanyInvitationRepository>(context => new CompanyInvitationRepository(context)),
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
        CreateTypeEntry<IMailingInformationRepository>(context => new MailingInformationRepository(context)),
        CreateTypeEntry<INotificationRepository>(context => new NotificationRepository(context)),
        CreateTypeEntry<INetworkRepository>(context => new NetworkRepository(context)),
        CreateTypeEntry<IOfferRepository>(context => new OfferRepository(context)),
        CreateTypeEntry<IOfferSubscriptionsRepository>(context => new OfferSubscriptionsRepository(context)),
        CreateTypeEntry<IPortalProcessStepRepository>(context => new ProcessStepRepository<ProcessTypeId, ProcessStepTypeId>(context)),
        CreateTypeEntry<IProcessStepRepository<Process<ProcessTypeId, ProcessStepTypeId>, ProcessStep<ProcessTypeId, ProcessStepTypeId>, ProcessTypeId, ProcessStepTypeId>>(context => new ProcessStepRepository<ProcessTypeId, ProcessStepTypeId>(context)),
        CreateTypeEntry<ITechnicalUserRepository>(context => new TechnicalUserRepository(context)),
        CreateTypeEntry<IStaticDataRepository>(context => new StaticDataRepository(context)),
        CreateTypeEntry<ITechnicalUserProfileRepository>(context => new TechnicalUserProfileRepository(context)),
        CreateTypeEntry<IUserBusinessPartnerRepository>(context => new UserBusinessPartnerRepository(context)),
        CreateTypeEntry<IUserRepository>(context => new UserRepository(context)),
        CreateTypeEntry<IUserRolesRepository>(context => new UserRolesRepository(context)),
        CreateTypeEntry<ICompanyCertificateRepository>(context => new CompanyCertificateRepository(context))
    });

    public override RepositoryType GetInstance<RepositoryType>()
    {
        object? repository = default;

        if (RepositoryTypes.TryGetValue(typeof(RepositoryType), out var createFunc))
        {
            repository = createFunc(portalDbContext);
        }

        return (RepositoryType)(repository ?? throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}", nameof(RepositoryType)));
    }
}

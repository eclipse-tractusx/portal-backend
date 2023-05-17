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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Repositories;

/// <summary>
/// Repository for accessing consent assigned offer subscription on the persistence layer.
/// </summary>
public class ConsentAssignedOfferSubscriptionRepository : IConsentAssignedOfferSubscriptionRepository
{
	private readonly PortalDbContext _portalDbContext;

	public ConsentAssignedOfferSubscriptionRepository(PortalDbContext portalDbContext)
	{
		_portalDbContext = portalDbContext;
	}

	/// <summary>
	/// Creates a consent with the given data in the database.
	/// </summary>
	/// <param name="consentId">Id of the consent</param>
	/// <param name="offerSubscriptionId">Id of the offer subscription</param>
	/// <returns>Returns the newly created consent</returns>
	public ConsentAssignedOfferSubscription CreateConsentAssignedOfferSubscription(Guid consentId, Guid offerSubscriptionId) =>
		_portalDbContext.ConsentAssignedOfferSubscriptions.Add(new ConsentAssignedOfferSubscription(offerSubscriptionId, consentId)).Entity;

	/// <inheritdoc />
	public IAsyncEnumerable<(Guid ConsentId, Guid AgreementId, ConsentStatusId ConsentStatusId)> GetConsentAssignedOfferSubscriptionsForSubscriptionAsync(Guid offerSubscriptionId,
			IEnumerable<Guid> agreementIds) =>
		_portalDbContext.ConsentAssignedOfferSubscriptions
			.Where(x =>
				x.OfferSubscriptionId == offerSubscriptionId &&
				agreementIds.Any(a => a == x.Consent!.AgreementId))
			.Select(x => new ValueTuple<Guid, Guid, ConsentStatusId>(x.ConsentId, x.Consent!.AgreementId, x.Consent.ConsentStatusId))
			.ToAsyncEnumerable();
}

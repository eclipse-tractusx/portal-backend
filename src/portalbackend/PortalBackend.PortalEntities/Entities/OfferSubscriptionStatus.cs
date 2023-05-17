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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

/// <summary>
/// Value table for app subscription statuses.
/// </summary>
public class OfferSubscriptionStatus
{
	/// <summary>
	/// Constructor.
	/// </summary>
	private OfferSubscriptionStatus()
	{
		Label = null!;
		OfferSubscriptions = new HashSet<OfferSubscription>();
	}

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="offerSubscriptionStatusId">Id of the subscription to wrap into entity.</param>
	public OfferSubscriptionStatus(OfferSubscriptionStatusId offerSubscriptionStatusId) : this()
	{
		Id = offerSubscriptionStatusId;
		Label = offerSubscriptionStatusId.ToString();
	}

	/// <summary>
	/// Id of the subscription status.
	/// </summary>
	public OfferSubscriptionStatusId Id { get; private set; }

	/// <summary>
	/// Label of the subscription status.
	/// </summary>
	[MaxLength(255)]
	public string Label { get; private set; }

	// Navigation properties

	/// <summary>
	/// All AppSubscriptions currently with this status.
	/// </summary>
	public virtual ICollection<OfferSubscription> OfferSubscriptions { get; private set; }
}

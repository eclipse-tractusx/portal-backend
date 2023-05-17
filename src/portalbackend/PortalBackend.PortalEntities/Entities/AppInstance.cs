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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Base;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class AppInstance : IBaseEntity
{
	private AppInstance()
	{
		AppSubscriptionDetails = new HashSet<AppSubscriptionDetail>();
		ServiceAccounts = new HashSet<AppInstanceAssignedCompanyServiceAccount>();
	}

	public AppInstance(Guid id, Guid appId, Guid iamClientId) : this()
	{
		Id = id;
		AppId = appId;
		IamClientId = iamClientId;
	}

	public Guid Id { get; private set; }
	public Guid AppId { get; private set; }
	public Guid IamClientId { get; set; }

	// Navigation properties
	public virtual Offer? App { get; private set; }
	public virtual IamClient? IamClient { get; private set; }
	public virtual ICollection<AppSubscriptionDetail> AppSubscriptionDetails { get; private set; }
	public virtual ICollection<AppInstanceAssignedCompanyServiceAccount> ServiceAccounts { get; private set; }
}

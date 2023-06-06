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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// View model containing an offer id and connected company subscription statuses.
/// </summary>
public class OfferCompanySubscriptionStatusData
{
    /// <summary>
    /// Constructor.
    /// </summary>
    public OfferCompanySubscriptionStatusData()
    {
        CompanySubscriptionStatuses = new HashSet<CompanySubscriptionStatusData>();
    }

    /// <summary>
    /// Id of the offer.
    /// </summary>
    public Guid OfferId { get; set; }

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Subscription statuses of subscribing companies.
    /// </summary>
    public IEnumerable<CompanySubscriptionStatusData> CompanySubscriptionStatuses { get; set; }

    /// <summary>
    /// Id of the lead Image
    /// </summary>
    /// <value></value>
    public Guid Image { get; set; }
}

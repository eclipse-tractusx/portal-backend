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

using Org.CatenaX.Ng.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.CatenaX.Ng.Portal.Backend.PortalBackend.DBAccess.Models;

/// <summary>
/// View model of an application's detailed data.
/// </summary>
public record AppDetailsData(string Title, string LeadPictureUri, string ProviderUri, string Provider, string LongDescription, string Price)
{
    /// <summary>
    /// ID of the app.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title or name of the app.
    /// </summary>
    public string Title { get; set; } = Title;

    /// <summary>
    /// Uri to app's lead picture.
    /// </summary>
    public string LeadPictureUri { get; set; } = LeadPictureUri;

    /// <summary>
    /// List of URIs to app's secondary pictures.
    /// </summary>
    public IEnumerable<string> DetailPictureUris { get; set; } = new List<string>();

    /// <summary>
    /// Uri to provider's marketing presence.
    /// </summary>
    public string ProviderUri { get; set; } = ProviderUri;

    /// <summary>
    /// Provider of the app.
    /// </summary>
    public string Provider { get; set; } = Provider;

    /// <summary>
    /// Email address of the app's primary contact.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Phone number of the app's primary contact.
    /// </summary>
    public string? ContactNumber { get; set; }

    /// <summary>
    /// Names of the app's use cases.
    /// </summary>
    public IEnumerable<string> UseCases { get; set; } = new List<string>();

    /// <summary>
    /// Long description of the app.
    /// </summary>
    public string LongDescription { get; set; } = LongDescription;

    /// <summary>
    /// Pricing information of the app.
    /// </summary>
    public string Price { get; set; } = Price;

    /// <summary>
    /// Tags assigned to application.
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Whether app has been purchased by the user's company.
    /// </summary>
    public OfferSubscriptionStatusId? IsSubscribed { get; set; }

    /// <summary>
    /// Languages that the app is available in.
    /// </summary>
    public IEnumerable<string> Languages { get; set; } = new List<string>();
}

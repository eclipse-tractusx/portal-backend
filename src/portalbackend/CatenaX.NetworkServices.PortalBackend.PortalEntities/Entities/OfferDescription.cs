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

using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class OfferDescription
{
    public OfferDescription(Guid offerId, string languageShortName, string descriptionLong, string descriptionShort)
    {
        OfferId = offerId;
        LanguageShortName = languageShortName;
        DescriptionLong = descriptionLong;
        DescriptionShort = descriptionShort;
    }

    /// <summary>
    /// construtor used for the Attach case
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="languageShortName"></param>
    public OfferDescription(Guid appId, string languageShortName)
    {
        OfferId = appId;
        LanguageShortName = languageShortName;
        DescriptionLong = null!;
        DescriptionShort = null!;
    }
    
    [MaxLength(4096)]
    public string DescriptionLong { get; set; }

    [MaxLength(255)]
    public string DescriptionShort { get; set; }

    public Guid OfferId { get; private set; }

    [StringLength(2, MinimumLength = 2)]
    public string LanguageShortName { get; private set; }

    // Navigation properties
    public virtual Offer? Offer { get; private set; }
    public virtual Language? Language { get; private set; }
}

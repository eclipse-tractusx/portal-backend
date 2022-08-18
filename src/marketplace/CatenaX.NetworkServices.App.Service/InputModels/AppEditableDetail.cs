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
using CatenaX.NetworkServices.PortalBackend.DBAccess.Models;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.App.Service.InputModels;

/// <summary>
/// Model for updating an app.
/// </summary>
public class AppEditableDetail
{
    public AppEditableDetail(IEnumerable<Localization> descriptions,IEnumerable<string>? images,string? providerUri,string? contactEmail,string? contactNumber)
    {
     Descriptions = descriptions;
     Images = images;
     ProviderUri = providerUri;
     ContactEmail = contactEmail;
     ContactNumber = contactNumber;
    }
    /// <summary>
    /// Description of Language.
    /// </summary>
    public IEnumerable<Localization> Descriptions { get; set; }

    /// <summary>
    /// Image Detail of App
    /// </summary>
    public IEnumerable<string>? Images { get; set; }

    /// <summary>
    /// Provider Url
    /// </summary>
    public string? ProviderUri { get; set; }

    /// <summary>
    /// Contact Email
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Contact Number
    /// </summary>
    public string? ContactNumber { get; set; }
}

/// <summary>
/// Model for updating Language Description
/// </summary>
public class Localization
{
    public Localization(string languageCode, string? longDescription)
    {
        LanguageCode = languageCode;
        LongDescription = longDescription;
    }
    /// <summary>
    /// Language Code
    /// </summary>
    public string LanguageCode {get;set;}

    /// <summary>
    /// Long Description
    /// </summary>
    public string? LongDescription {get;set;}
}




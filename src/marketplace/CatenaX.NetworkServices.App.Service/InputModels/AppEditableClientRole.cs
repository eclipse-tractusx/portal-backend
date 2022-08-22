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

namespace CatenaX.NetworkServices.App.Service.InputModels;

public class AppEditableClientRoles
{
    public AppEditableClientRoles(string role, IEnumerable<AppRoleDescription> descriptions)
    {
        Role = role;
        Descriptions = descriptions;
    }
    public string Role { get; set; }
    public IEnumerable<AppRoleDescription> Descriptions { get; set; }
}
public class AppRoleDescription
{
    public AppRoleDescription(string languageCode, string description)
    {
        LanguageCode = languageCode;
        Description = description;
    }
    /// <summary>
    /// Language Code
    /// </summary>
    public string LanguageCode {get;set;}

    /// <summary>
    /// Long Description
    /// </summary>
    public string Description {get;set;}
}

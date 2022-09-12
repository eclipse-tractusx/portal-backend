/********************************************************************************
 * Copyright (c) 2021,2022 Contributors to https://github.com/lvermeulen/Keycloak.Net.git and BMW Group AG
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

﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Keycloak.Net.Models.Users
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("createdTimestamp")]
        public long CreatedTimestamp { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }
        [JsonProperty("totp")]
        public bool? Totp { get; set; }
        [JsonProperty("emailVerified")]
        public bool? EmailVerified { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("disableableCredentialTypes")]
        public IEnumerable<string> DisableableCredentialTypes { get; set; }
        [JsonProperty("requiredActions")]
        public IEnumerable<string> RequiredActions { get; set; }
        [JsonProperty("notBefore")]
        public int? NotBefore { get; set; }
        [JsonProperty("access")]
        public UserAccess Access { get; set; }
        [JsonProperty("attributes")]
        public IDictionary<string, IEnumerable<string>> Attributes { get; set; }
        [JsonProperty("clientConsents")]
        public IEnumerable<UserConsent> ClientConsents { get; set; }
        [JsonProperty("clientRoles")]
        public IDictionary<string, object> ClientRoles { get; set; }
        [JsonProperty("credentials")]
        public IEnumerable<Credentials> Credentials { get; set; }
        [JsonProperty("federatedIdentities")]
        public IEnumerable<FederatedIdentity> FederatedIdentities { get; set; }
        [JsonProperty("federationLink")]
        public string FederationLink { get; set; }
        [JsonProperty("groups")]
        public IEnumerable<string> Groups { get; set; }
        [JsonProperty("origin")]
        public string Origin { get; set; }
        [JsonProperty("realmRoles")]
        public IEnumerable<string> RealmRoles { get; set; }
        [JsonProperty("self")]
        public string Self { get; set; }
        [JsonProperty("serviceAccountClientId")]
        public string ServiceAccountClientId { get; set; }
    }
}

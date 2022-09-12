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

﻿using Newtonsoft.Json;

namespace Keycloak.Net.Models.Clients
{
    public class ClientConfig
    {
        [JsonProperty("userinfotokenclaim")]
        public string UserInfoTokenClaim { get; set; }
        [JsonProperty("user.attribute")]
        public string UserAttribute { get; set; }
        [JsonProperty("idtokenclaim")]
        public string IdTokenClaim { get; set; }
        [JsonProperty("accesstokenclaim")]
        public string AccessTokenClaim { get; set; }
        [JsonProperty("claimname")]
        public string ClaimName { get; set; }
        [JsonProperty("jsonTypelabel")]
        public string JsonTypelabel { get; set; }
        [JsonProperty("friendly.name")]
        public string FriendlyName { get; set; }
        [JsonProperty("attribute.name")]
        public string AttributeName { get; set; }
    }
}
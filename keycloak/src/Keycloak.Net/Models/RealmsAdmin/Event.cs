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

namespace Keycloak.Net.Models.RealmsAdmin
{
    public class Event
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }
        [JsonProperty("details")]
        public IDictionary<string, object> Details { get; set; }
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        [JsonProperty("realmId")]
        public string RealmId { get; set; }
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
        [JsonProperty("time")]
        public long Time { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}

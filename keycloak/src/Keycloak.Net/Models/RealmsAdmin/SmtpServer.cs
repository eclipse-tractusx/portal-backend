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

namespace Keycloak.Net.Models.RealmsAdmin
{
    public class SmtpServer
    {
        [JsonProperty("host")]
        public string Host { get; set; }
        [JsonProperty("ssl")]
        public string Ssl { get; set; }
        [JsonProperty("starttls")]
        public string StartTls { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("auth")]
        public string Auth { get; set; }
        [JsonProperty("from")]
        public string From { get; set; }
        [JsonProperty("fromDisplayName")]
        public string FromDisplayName { get; set; }
        [JsonProperty("replyTo")]
        public string ReplyTo { get; set; }
        [JsonProperty("replyToDisplayName")]
        public string ReplyToDisplayName { get; set; }
        [JsonProperty("envelopeFrom")]
        public string EnvelopeFrom { get; set; }
        [JsonProperty("port")]
        public string Port { get; set; }
    }
}
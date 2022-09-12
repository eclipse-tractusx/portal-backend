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

using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Keycloak.Library.Models.Root;

public class Themes
{
    [JsonProperty("common")]
    public List<Common> Common { get; set; }

    [JsonProperty("admin")]
    public List<Account> Admin { get; set; }

    [JsonProperty("login")]
    public List<Account> Login { get; set; }

    [JsonProperty("welcome")]
    public List<Common> Welcome { get; set; }

    [JsonProperty("account")]
    public List<Account> Account { get; set; }

    [JsonProperty("email")]
    public List<Account> Email { get; set; }
}

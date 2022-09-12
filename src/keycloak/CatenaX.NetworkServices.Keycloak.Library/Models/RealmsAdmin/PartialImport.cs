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

using CatenaX.NetworkServices.Keycloak.Library.Common.Converters;
using CatenaX.NetworkServices.Keycloak.Library.Models.Clients;
using CatenaX.NetworkServices.Keycloak.Library.Models.Groups;
using CatenaX.NetworkServices.Keycloak.Library.Models.Users;
using Newtonsoft.Json;

namespace CatenaX.NetworkServices.Keycloak.Library.Models.RealmsAdmin;

public class PartialImport
{
    public IEnumerable<Client> Clients { get; set; }
    public IEnumerable<Group> Groups { get; set; }
    public IEnumerable<IdentityProvider> IdentityProviders { get; set; }
    public string IfResourceExists { get; set; }
    [JsonConverter(typeof(PoliciesConverter))]
    public Policies Policy { get; set; }
    public Roles Roles { get; set; }
    public IEnumerable<User> Users { get; set; }
}

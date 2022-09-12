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

namespace Keycloak.Net.Models.Root
{
    public class LdapMapperProviders
    {
        [JsonProperty("msad-lds-user-account-control-mapper")]
        public HasOrder MsadLdsUserAccountControlMapper { get; set; }

        [JsonProperty("msad-user-account-control-mapper")]
        public HasOrder MsadUserAccountControlMapper { get; set; }

        [JsonProperty("group-ldap-mapper")]
        public HasOrder GroupLdapMapper { get; set; }

        [JsonProperty("user-attribute-ldap-mapper")]
        public HasOrder UserAttributeLdapMapper { get; set; }

        [JsonProperty("role-ldap-mapper")]
        public HasOrder RoleLdapMapper { get; set; }

        [JsonProperty("hardcoded-ldap-role-mapper")]
        public HasOrder HardcodedLdapRoleMapper { get; set; }

        [JsonProperty("full-name-ldap-mapper")]
        public HasOrder FullNameLdapMapper { get; set; }

        [JsonProperty("hardcoded-ldap-attribute-mapper")]
        public HasOrder HardcodedLdapAttributeMapper { get; set; }

        [JsonProperty("hardcoded-ldap-group-mapper")]
        public HasOrder HardcodedLdapGroupMapper { get; set; }
    }
}
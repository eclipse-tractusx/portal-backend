/********************************************************************************
 * Copyright (c) 2021, 2023 BMW Group AG
 * Copyright (c) 2021, 2023 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.Provisioning.Library.Enums;

public enum IdentityProviderMapperType
{
	HARDCODED_SESSION_ATTRIBUTE = 1,
	HARDCODED_ATTRIBUTE = 2,
	OIDC_ADVANCED_GROUP = 3,
	OIDC_USER_ATTRIBUTE = 4,
	OIDC_ADVANCED_ROLE = 5,
	OIDC_HARDCODED_ROLE = 6,
	OIDC_ROLE = 7,
	OIDC_USERNAME = 8,
	KEYCLOAK_OIDC_ROLE = 9,
}

/********************************************************************************
 * Copyright (c) 2022 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

public enum CompanyApplicationStatusId
{
    CREATED = 1,
    ADD_COMPANY_DATA = 2,
    INVITE_USER = 3,
    SELECT_COMPANY_ROLE = 4,
    UPLOAD_DOCUMENTS = 5,
    VERIFY = 6,
    SUBMITTED = 7,
    CONFIRMED = 8,
    DECLINED = 9,
    CANCELLED_BY_CUSTOMER = 10
}

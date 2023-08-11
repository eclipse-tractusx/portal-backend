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

using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;
using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;

namespace Org.Eclipse.TractusX.Portal.Backend.Administration.Service.Tests.IntegrationTests.Seeding;

public class BaseSeeding : IBaseSeeding
{
    public Action<PortalDbContext> SeedData() => dbContext =>
    {
        BaseSeed.SeedBaseData().Invoke(dbContext);

        dbContext.Connectors.AddRange(new List<Connector>
        {
            new(new Guid("5aea3711-cc54-47b4-b7eb-ba9f3bf1cb15"), "Tes One", "DE", "https://api.tes-one.com")
            {
                ProviderId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
                HostId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
                TypeId = ConnectorTypeId.COMPANY_CONNECTOR,
                StatusId =ConnectorStatusId.ACTIVE,
            },
            new(new Guid("f7310cff-a51d-4af8-9bc3-1525e9d1601b"), "Con on Air", "PT", "https://api.con-on-air.com")
            {
                ProviderId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
                HostId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
                TypeId = ConnectorTypeId.CONNECTOR_AS_A_SERVICE,
                StatusId = ConnectorStatusId.PENDING,
            },
        });
    };
}

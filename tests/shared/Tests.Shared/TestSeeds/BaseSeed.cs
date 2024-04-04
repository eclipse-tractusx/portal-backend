/********************************************************************************
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

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;

public class BaseSeed
{
    public static Action<PortalDbContext> SeedBaseData() => dbContext =>
    {
        dbContext.Languages.AddRange(new List<Language> { new("de"), new("en") });

        dbContext.Countries.AddRange(new List<Country>
        {
            new("DE") {Alpha3Code = "DEU"},
            new("PT") {Alpha3Code = "PRT"}
        });

        dbContext.Addresses.AddRange(new List<Address>
        {
            new(new Guid("b4db3945-19a7-4a50-97d6-e66e8dfd04fb"), "Munich", "Street", "DE", DateTimeOffset.UtcNow)
            {
                Zipcode = "00001", Streetnumber = "1", Region = "BY", Streetadditional = "foo"
            }
        });

        dbContext.Companies.AddRange(new List<Company>
        {
            new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), "Catena-X", CompanyStatusId.ACTIVE,
                DateTimeOffset.UtcNow)
            {
                AddressId = new Guid("b4db3945-19a7-4a50-97d6-e66e8dfd04fb"),
                Shortname = "Cat-X",
                BusinessPartnerNumber = "CAXSDUMMYCATENAZZ",
            }
        });

        dbContext.Identities.AddRange(new List<Identity>
        {
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow,
                new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER)
        });

        dbContext.CompanyUsers.AddRange(new List<CompanyUser>
        {
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"))
            {
                Email = "tester.user6@test.de", Firstname = "Test User 6", Lastname = "CX Admin",
            }
        });
    };
}

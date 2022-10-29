/********************************************************************************
 * Copyright (c) 2021,2022 BMW Group AG
 * Copyright (c) 2021,2022 Contributors to the Eclipse Foundation
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

public static class BaseSeed
{
    public static Action<PortalDbContext> SeedBasedata() => dbContext =>
    {
        dbContext.Addresses.AddRange(new List<Address>
        {
            new(new Guid("b4db3945-19a7-4a50-97d6-e66e8dfd04fb"), "Munich", "Street", "DE", DateTimeOffset.UtcNow)
            {
                Zipcode = "00001",
                Streetnumber = "1"
            },
            new(new Guid("12302f9b-418c-4b8c-aea8-3eedf67e6a02"), "Munich", "Street", "DE", DateTimeOffset.UtcNow)
            {
                Zipcode = "00001",
                Streetnumber = "2"
            },
        });

        dbContext.Companies.AddRange(new List<Company>
        {
            new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), "Catena-X", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                AddressId = new Guid("b4db3945-19a7-4a50-97d6-e66e8dfd04fb"),
                Shortname = "Catena-X",
                BusinessPartnerNumber = "CAXSDUMMYCATENAZZ",
            },
            new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f99"), "Test Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                AddressId = new Guid("12302f9b-418c-4b8c-aea8-3eedf67e6a02"),
                Shortname = "Test",
            },
        });
        
        dbContext.ServiceProviderCompanyDetails.AddRange(new List<ServiceProviderCompanyDetail>
        {
            new ServiceProviderCompanyDetail(new Guid("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), "https://www.test-service.com", DateTimeOffset.UtcNow)
        });
        
        dbContext.CompanyRoles.AddRange(new List<CompanyRole>
        {
            new(CompanyRoleId.SERVICE_PROVIDER)
        }); 
        
        dbContext.CompanyAssignedRoles.AddRange(new List<CompanyAssignedRole>
        {
            new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyRoleId.SERVICE_PROVIDER),
            new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyRoleId.APP_PROVIDER),
            new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyRoleId.ACTIVE_PARTICIPANT),
            new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f99"), CompanyRoleId.ACTIVE_PARTICIPANT)
        });

        dbContext.CompanyUsers.AddRange(new List<CompanyUser>
        {
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "cx-user-2",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "cx-admin-2",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "company-admin-2",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "it-admin-2",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "CX User",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "CX Admin",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("adf37b09-53f3-48ea-b8fb-8cbb7fd79324"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f99"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "Test Company",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
        });

        dbContext.IamUsers.AddRange(new List<IamUser>
        {
            new ("623770c5-cf38-4b9f-9a35-f8b9ae972e2d", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000")),
            new ("3d8142f1-860b-48aa-8c2b-1ccb18699f65", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001")),
            new ("623770c5-cf38-4b9f-9a35-f8b9ae972e2e", new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990")),
            new ("3d8142f1-860b-48aa-8c2b-1ccb18699f66", new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991")),
            new ("47ea7f1f-f10d-4cb2-acaf-b77323ef25b4", new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992")),
            new ("4b8f156e-5dfc-4a58-9384-1efb195c1c34", new Guid("adf37b09-53f3-48ea-b8fb-8cbb7fd79324"))
        });

        dbContext.IamClients.AddRange(new List<IamClient>
        {
            new (new Guid("0c9051d0-d032-11ec-9d64-0242ac120002"), "Cl2-CX-Portal"),
            new (new Guid("f032a034-d035-11ec-9d64-0242ac120002"), "Cl1-CX-Registration"),
            new (new Guid("cf207afb-d213-4c33-becc-0cabeef174a7"), "https://catenax-int-dismantler-s66pftcc.authentication.eu10.hana.ondemand.com"),
        });

        dbContext.Offers.AddRange(OfferData.Offers);
        dbContext.AppInstances.AddRange(AppInstanceData.AppInstances);
        dbContext.OfferSubscriptions.AddRange(new List<OfferSubscription>
        {
            new (new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferSubscriptionStatusId.ACTIVE, new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001")),
            new (new Guid("28149c6d-833f-49c5-aea2-ab6a5a37f462"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferSubscriptionStatusId.ACTIVE, new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"))
        });

        dbContext.UserRoles.AddRange(new List<UserRole>
        {
            new (new Guid("58f897ec-0aad-4588-8ffa-5f45d6638633"), "CX User", new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4")),
            new (new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632"), "CX Admin", new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4")),
            new (new Guid("7410693c-c893-409e-852f-9ee886ce94a6"), "Company Admin", new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4")),
            new (new Guid("607818be-4978-41f4-bf63-fa8d2de51154"), "IT Admin", new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4")),
        });

        dbContext.CompanyUserAssignedRoles.AddRange(new List<CompanyUserAssignedRole>
        {
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), new Guid("58f897ec-0aad-4588-8ffa-5f45d6638633")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"), new Guid("58f897ec-0aad-4588-8ffa-5f45d6638633")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991"), new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992"), new Guid("7410693c-c893-409e-852f-9ee886ce94a6")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993"), new Guid("607818be-4978-41f4-bf63-fa8d2de51154"))
        });
        
        dbContext.Agreements.AddRange(new List<Agreement>
        {
            new(new Guid("f6d3148b-2e2b-4688-a382-326d4232ee6e"), AgreementCategoryId.CX_FRAME_CONTRACT, "CatenaX Base Frame Agreement", DateTimeOffset.UtcNow)
            {
                IssuerCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019951"), AgreementCategoryId.DATA_CONTRACT, "Test Agreement", DateTimeOffset.UtcNow)
            {
                IssuerCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019952"), AgreementCategoryId.APP_CONTRACT, "App Agreement", DateTimeOffset.UtcNow)
            {
                IssuerCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
            },
            new(new Guid("979a29b1-40c2-4169-979c-43c3156dbf64"), AgreementCategoryId.SERVICE_CONTRACT, "Service Agreement", DateTimeOffset.UtcNow)
            {
                IssuerCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
            }
        });
        dbContext.Consents.AddRange(new List<Consent>
        {
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019910"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019952"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), ConsentStatusId.INACTIVE, DateTimeOffset.UtcNow)
            {
                Comment = "Just a test"
            }
        });
        dbContext.AgreementAssignedCompanyRoles.AddRange(new List<AgreementAssignedCompanyRole>
        {
            new (new Guid("f6d3148b-2e2b-4688-a382-326d4232ee6e"), CompanyRoleId.ACTIVE_PARTICIPANT)
        });
        dbContext.AgreementAssignedOffers.AddRange(new List<AgreementAssignedOffer>
        {
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019952"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4")),
            new (new Guid("979a29b1-40c2-4169-979c-43c3156dbf64"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"))
        });
    };
}

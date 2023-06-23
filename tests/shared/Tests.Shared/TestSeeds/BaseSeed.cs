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

namespace Org.Eclipse.TractusX.Portal.Backend.Tests.Shared.TestSeeds;

public static class BaseSeed
{
    public static Action<PortalDbContext> SeedBasedata() => dbContext =>
    {
        dbContext.Languages.AddRange(new List<Language>
        {
            new ("de"),
            new ("en")
        });

        dbContext.Countries.AddRange(new List<Country>
        {
            new("DE", "Deutschland", "Germany")
            {
                Alpha3Code = "DEU"
            },
            new("PT", "Portugal", "Portugal")
            {
                Alpha3Code = "PRT"
            }
        });

        dbContext.UseCases.AddRange(new List<UseCase>
        {
            new(new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b90"), "Modular Production", "MP")
        });

        dbContext.Addresses.AddRange(new List<Address>
        {
            new(new Guid("b4db3945-19a7-4a50-97d6-e66e8dfd04fb"), "Munich", "Street", "DE", DateTimeOffset.UtcNow)
            {
                Zipcode = "00001",
                Streetnumber = "1",
                Region = "BY",
                Streetadditional = "foo"
            },
            new(new Guid("12302f9b-418c-4b8c-aea8-3eedf67e6a02"), "Munich", "Street", "DE", DateTimeOffset.UtcNow)
            {
                Zipcode = "00001",
                Streetnumber = "2"
            },
            new(new Guid("1fdf48eb-53f1-4d44-9685-c8f78189b156"), "Munich", "Street", "DE", DateTimeOffset.UtcNow)
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
                Shortname = "Cat-X",
                BusinessPartnerNumber = "CAXSDUMMYCATENAZZ",
            },
            new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f99"), "Test Company", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                AddressId = new Guid("12302f9b-418c-4b8c-aea8-3eedf67e6a02"),
                Shortname = "Test",
            },
            new(new Guid("27538eac-27a3-4f74-9306-e5149b93ade5"), "Submitted Company With Bpn", CompanyStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                AddressId = new Guid("1fdf48eb-53f1-4d44-9685-c8f78189b156"),
                Shortname = "Test",
                BusinessPartnerNumber = "CAXSTESTYCATENAZZ",
            }
        });

        dbContext.Identities.AddRange(new List<Identity>
        {
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER)
            {
                UserEntityId = "623770c5-cf38-4b9f-9a35-f8b9ae972e2e"
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER)
            {
                UserEntityId = "3d8142f1-860b-48aa-8c2b-1ccb18699f66"
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER)
            {
                UserEntityId = "47ea7f1f-f10d-4cb2-acaf-b77323ef25b4"
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER),
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER)
            {
                UserEntityId = "623770c5-cf38-4b9f-9a35-f8b9ae972e2d"
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER)
            {
                UserEntityId = "3d8142f1-860b-48aa-8c2b-1ccb18699f65"
            },
            new(new Guid("40ed8c0d-b506-4c15-b2a9-85fee4b0c280"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.INACTIVE, IdentityTypeId.COMPANY_USER),
            new(new Guid("22b7bfef-19f5-4d8a-9fb4-af1a5e978f21"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.DELETED, IdentityTypeId.COMPANY_USER),
            new(new Guid("adf37b09-53f3-48ea-b8fb-8cbb7fd79324"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f99"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_USER)
            {
                UserEntityId = "4b8f156e-5dfc-4a58-9384-1efb195c1c34"
            },

            new (new Guid("7259744a-2ab0-49bf-9fe3-fcb88f6ad332"), DateTimeOffset.UtcNow, new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UserStatusId.ACTIVE, IdentityTypeId.COMPANY_SERVICE_ACCOUNT)
        });

        dbContext.CompanyUsers.AddRange(new List<CompanyUser>
        {
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"), null)
            {
                Email = "tester.user1@test.de",
                Firstname = "Test User 1",
                Lastname = "cx-user-2",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991"), null)
            {
                Email = "tester.user2@test.de",
                Firstname = "Test User 2",
                Lastname = "cx-admin-2",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992"), null)
            {
                Email = "tester.user3@test.de",
                Firstname = "Test User 3",
                Lastname = "company-admin-2",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user4@test.de",
                Firstname = "Test User 4",
                Lastname = "it-admin-2",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user5@test.de",
                Firstname = "Test User 5",
                Lastname = "CX User",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user6@test.de",
                Firstname = "Test User 6",
                Lastname = "CX Admin",
            },
            new(new Guid("40ed8c0d-b506-4c15-b2a9-85fee4b0c280"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user7@test.de",
                Firstname = "Test User 7",
                Lastname = "Inactive",
            },
            new(new Guid("22b7bfef-19f5-4d8a-9fb4-af1a5e978f21"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user8@test.de",
                Firstname = "Test User 8",
                Lastname = "Deleted",
            },
            new(new Guid("adf37b09-53f3-48ea-b8fb-8cbb7fd79324"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "Test Company",
            },
        });

        dbContext.IamClients.AddRange(new List<IamClient>
        {
            new (new Guid("0c9051d0-d032-11ec-9d64-0242ac120002"), "Cl2-CX-Portal"),
            new (new Guid("f032a034-d035-11ec-9d64-0242ac120002"), "Cl1-CX-Registration"),
            new (new Guid("cf207afb-d213-4c33-becc-0cabeef174a7"), "https://catenax-int-dismantler-s66pftcc.authentication.eu10.hana.ondemand.com"),
        });

        dbContext.NotificationTypeAssignedTopics.AddRange(new List<NotificationTypeAssignedTopic>()
        {
            new(NotificationTypeId.INFO, NotificationTopicId.INFO),
            new(NotificationTypeId.TECHNICAL_USER_CREATION, NotificationTopicId.INFO),
            new(NotificationTypeId.CONNECTOR_REGISTERED, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME_SERVICE_PROVIDER, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME_CONNECTOR_REGISTRATION, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME_USE_CASES, NotificationTopicId.INFO),
            new(NotificationTypeId.WELCOME_APP_MARKETPLACE, NotificationTopicId.INFO),
            new(NotificationTypeId.ACTION, NotificationTopicId.ACTION),
            new(NotificationTypeId.APP_SUBSCRIPTION_REQUEST, NotificationTopicId.ACTION),
            new(NotificationTypeId.SERVICE_REQUEST, NotificationTopicId.ACTION),
            new(NotificationTypeId.APP_SUBSCRIPTION_ACTIVATION, NotificationTopicId.OFFER),
            new(NotificationTypeId.APP_RELEASE_REQUEST, NotificationTopicId.OFFER),
            new(NotificationTypeId.SERVICE_ACTIVATION, NotificationTopicId.OFFER),
            new(NotificationTypeId.APP_ROLE_ADDED, NotificationTopicId.OFFER),
            new(NotificationTypeId.APP_RELEASE_APPROVAL, NotificationTopicId.OFFER),
            new(NotificationTypeId.SERVICE_RELEASE_REQUEST, NotificationTopicId.OFFER),
            new(NotificationTypeId.SERVICE_RELEASE_APPROVAL, NotificationTopicId.OFFER),
            new(NotificationTypeId.APP_RELEASE_REJECTION, NotificationTopicId.OFFER),
            new(NotificationTypeId.SERVICE_RELEASE_REJECTION, NotificationTopicId.OFFER)
        });

        dbContext.Notifications.AddRange(new List<Notification>
        {
            new (new Guid("94F22922-04F6-4A4E-B976-1BF2FF3DE973"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.ACTION, false),
            new (new Guid("5FCBA636-E0F6-4C86-B5CC-7711A55669B6"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.ACTION, true),
            new (new Guid("8bdaada7-4885-4aa7-87ce-1a325492a485"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.ACTION, true),
            new (new Guid("9D03FE54-3581-4399-84DD-D606E9A2B3D5"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.ACTION, false),
            new (new Guid("34782A2E-7B54-4E78-85BA-419AF534837F"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.INFO, true),
            new (new Guid("19AFFED7-13F0-4868-9A23-E77C23D8C889"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), DateTimeOffset.UtcNow, NotificationTypeId.INFO, false),
        });

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

        dbContext.CountryAssignedIdentifiers.AddRange(new("DE", UniqueIdentifierId.COMMERCIAL_REG_NUMBER), new("DE", UniqueIdentifierId.VAT_ID) { BpdmIdentifierId = BpdmIdentifierId.EU_VAT_ID_DE });
        dbContext.CompanyIdentifiers.AddRange(new(new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UniqueIdentifierId.COMMERCIAL_REG_NUMBER, "REG08154711"), new(new Guid("27538eac-27a3-4f74-9306-e5149b93ade5"), UniqueIdentifierId.VAT_ID, "DE123456789"));
    };
}

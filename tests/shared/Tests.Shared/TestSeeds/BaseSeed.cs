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
            new ("de", "deutsch", "german"),
            new ("en", "englisch", "english")
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
            },
        });
        dbContext.CompanyApplications.AddRange(new List<CompanyApplication>
        {
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyApplicationStatusId.CONFIRMED, DateTimeOffset.UtcNow),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f99"), CompanyApplicationStatusId.SUBMITTED, DateTimeOffset.UtcNow),
            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), new Guid("27538eac-27a3-4f74-9306-e5149b93ade5"), CompanyApplicationStatusId.SUBMITTED, DateTimeOffset.UtcNow),
        });

        dbContext.ApplicationChecklist.AddRange(new List<ApplicationChecklistEntry>
        {
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.IN_PROGRESS, DateTimeOffset.UtcNow),
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow),
            
            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), ApplicationChecklistEntryTypeId.REGISTRATION_VERIFICATION, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow),
            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), ApplicationChecklistEntryTypeId.BUSINESS_PARTNER_NUMBER, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow),
            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), ApplicationChecklistEntryTypeId.SELF_DESCRIPTION_LP, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow),
            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), ApplicationChecklistEntryTypeId.IDENTITY_WALLET, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow),
            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), ApplicationChecklistEntryTypeId.CLEARING_HOUSE, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow),
            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), ApplicationChecklistEntryTypeId.APPLICATION_ACTIVATION, ApplicationChecklistEntryStatusId.TO_DO, DateTimeOffset.UtcNow),
        });
        
        dbContext.ProcessSteps.AddRange(new List<ProcessStep>
        {
            new (new Guid("b76c21e2-5480-4e74-b939-62e3817100fc"), ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("9a804509-1c6e-4d75-9657-a1fd52817b95"), ProcessStepTypeId.VERIFY_REGISTRATION, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("ee5a6a76-fe21-4242-afb2-5b0ae070a7e7"), ProcessStepTypeId.CREATE_IDENTITY_WALLET, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("bb772c1c-8b2e-4ea5-95db-6a61e653a37d"), ProcessStepTypeId.START_CLEARING_HOUSE, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("b5e2b207-2fa3-4936-b3ca-406b87ca4234"), ProcessStepTypeId.END_CLEARING_HOUSE, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("4e17a856-5851-43ef-b8b6-53f6c19e8e61"), ProcessStepTypeId.START_SELF_DESCRIPTION_LP, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("8a113543-70b0-439a-9324-46a7903bb682"), ProcessStepTypeId.ACTIVATE_APPLICATION, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            
            new (new Guid("24b9745b-7b2f-429b-83ca-937ae3cff7ae"), ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("44299c18-315c-4748-9e35-a0aafcda9795"), ProcessStepTypeId.VERIFY_REGISTRATION, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("53612888-a32e-4fe1-a2df-28d6c24843d0"), ProcessStepTypeId.CREATE_IDENTITY_WALLET, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            new (new Guid("f9046b45-7c00-4113-a779-15a657515c86"), ProcessStepTypeId.START_CLEARING_HOUSE, ProcessStepStatusId.DONE, DateTimeOffset.UtcNow),
            
            new (new Guid("48f35f84-8d98-4fbd-ba80-8cbce5eeadb5"), ProcessStepTypeId.CREATE_BUSINESS_PARTNER_NUMBER_MANUAL, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow),
            new (new Guid("7c077dd9-6e64-4d02-ac23-fe848ee94527"), ProcessStepTypeId.VERIFY_REGISTRATION, ProcessStepStatusId.TODO, DateTimeOffset.UtcNow),
        });
        
        dbContext.ApplicationAssignedProcessSteps.AddRange(new List<ApplicationAssignedProcessStep>
        {
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("b76c21e2-5480-4e74-b939-62e3817100fc")),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("9a804509-1c6e-4d75-9657-a1fd52817b95")),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("ee5a6a76-fe21-4242-afb2-5b0ae070a7e7")),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("bb772c1c-8b2e-4ea5-95db-6a61e653a37d")),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("b5e2b207-2fa3-4936-b3ca-406b87ca4234")),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("4e17a856-5851-43ef-b8b6-53f6c19e8e61")),
            new (new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("8a113543-70b0-439a-9324-46a7903bb682")),
            
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), new Guid("24b9745b-7b2f-429b-83ca-937ae3cff7ae")),
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), new Guid("44299c18-315c-4748-9e35-a0aafcda9795")),
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), new Guid("53612888-a32e-4fe1-a2df-28d6c24843d0")),
            new (new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), new Guid("f9046b45-7c00-4113-a779-15a657515c86")),

            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), new Guid("48f35f84-8d98-4fbd-ba80-8cbce5eeadb5")),
            new (new Guid("2bb2005f-6e8d-41eb-967b-cde67546cafc"), new Guid("7c077dd9-6e64-4d02-ac23-fe848ee94527")),
        });

        dbContext.Invitations.AddRange(new List<Invitation>
        {
            new (new Guid("aa6cdb72-22d8-4f4f-8a0b-5f8c4b59a407"), new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"), InvitationStatusId.ACCEPTED, DateTimeOffset.UtcNow),
            new (new Guid("c175e0a8-14cf-49fb-918b-e878060a55da"), new Guid("4829b64c-de6a-426c-81fc-c0bcf95bcb76"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991"), InvitationStatusId.ACCEPTED, DateTimeOffset.UtcNow),
            new (new Guid("2c7e3faa-1df1-41a1-abb0-6f902ef38987"), new Guid("1b86d973-3aac-4dcd-a9e9-0c222766202b"), new Guid("adf37b09-53f3-48ea-b8fb-8cbb7fd79324"), InvitationStatusId.ACCEPTED, DateTimeOffset.UtcNow),
        });
        dbContext.Documents.AddRange(new List<Document>
        {
            new (new Guid("fda6c9cb-62be-4a98-99c1-d9c5a2df4aad"), new byte[1024], new byte[1024], "test1.pdf", DateTimeOffset.UtcNow, DocumentStatusId.INACTIVE, DocumentTypeId.CX_FRAME_CONTRACT)
            {
                CompanyUserId = new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990")
            },
            new (new Guid("90a24c6d-1092-4590-ae89-a9d2bff1ea40"), new byte[1024], new byte[1024], "test2.pdf", DateTimeOffset.UtcNow, DocumentStatusId.INACTIVE, DocumentTypeId.CX_FRAME_CONTRACT)
            {
                CompanyUserId = new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991")
            },
            new (new Guid("5936c0f4-d07a-4f90-b397-b00b8b30c6f4"), new byte[1024], new byte[1024], "test3.pdf", DateTimeOffset.UtcNow, DocumentStatusId.INACTIVE, DocumentTypeId.CX_FRAME_CONTRACT)
            {
                CompanyUserId = new Guid("adf37b09-53f3-48ea-b8fb-8cbb7fd79324")
            },
            new (new Guid("7fc2fb78-8dc2-4f5f-b1d1-91c9c2f4506f"), new byte[1024], new byte[1024], "fake.pdf", DateTimeOffset.UtcNow.AddYears(-1), DocumentStatusId.INACTIVE, DocumentTypeId.CX_FRAME_CONTRACT)
            {
                CompanyUserId = new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992")
            },
            new (new Guid("90a24c6d-1092-4590-ae89-a9d2bff1ea41"), new byte[1024], new byte[1024], "test5.pdf", DateTimeOffset.UtcNow, DocumentStatusId.PENDING, DocumentTypeId.APP_LEADIMAGE)
            {
                CompanyUserId = new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001")
            },
            new (new Guid("2b9e45a6-ec22-489a-b2cb-9cbdd0b6bfbc"), new byte[1024], new byte[1024], "test5.pdf", DateTimeOffset.UtcNow, DocumentStatusId.PENDING, DocumentTypeId.APP_IMAGE)
            {
                CompanyUserId = new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001")
            },
            new (new Guid("b42de0f5-fd35-428a-86eb-a048ed7e57fa"), new byte[1024], new byte[1024], "test5.pdf", DateTimeOffset.UtcNow, DocumentStatusId.PENDING, DocumentTypeId.ADDITIONAL_DETAILS)
            {
                CompanyUserId = new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001")
            }
        });

        dbContext.ProviderCompanyDetails.AddRange(new List<ProviderCompanyDetail>
        {
            new(new Guid("ee8b4b4a-056e-4f0b-bc2a-cc1adbedf122"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), "https://www.test-service.com", DateTimeOffset.UtcNow)
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
                Email = "tester.user1@test.de",
                Firstname = "Test User 1",
                Lastname = "cx-user-2",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user2@test.de",
                Firstname = "Test User 2",
                Lastname = "cx-admin-2",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user3@test.de",
                Firstname = "Test User 3",
                Lastname = "company-admin-2",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user4@test.de",
                Firstname = "Test User 4",
                Lastname = "it-admin-2",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user5@test.de",
                Firstname = "Test User 5",
                Lastname = "CX User",
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user6@test.de",
                Firstname = "Test User 6",
                Lastname = "CX Admin",
            },
            new(new Guid("40ed8c0d-b506-4c15-b2a9-85fee4b0c280"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.INACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user7@test.de",
                Firstname = "Test User 7",
                Lastname = "Inactive",
            },
            new(new Guid("22b7bfef-19f5-4d8a-9fb4-af1a5e978f21"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.DELETED, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user8@test.de",
                Firstname = "Test User 8",
                Lastname = "Deleted",
            },
            new(new Guid("adf37b09-53f3-48ea-b8fb-8cbb7fd79324"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f99"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow, new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"))
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "Test Company",
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

        dbContext.CompanyServiceAccounts.AddRange(new List<CompanyServiceAccount>
        {
            new (new Guid("7259744a-2ab0-49bf-9fe3-fcb88f6ad332"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyServiceAccountStatusId.ACTIVE, "Test SA", "Only a test sa", DateTimeOffset.UtcNow, CompanyServiceAccountTypeId.MANAGED)
            {
                OfferSubscriptionId = new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd")
            }
        });
        
        dbContext.IamClients.AddRange(new List<IamClient>
        {
            new (new Guid("0c9051d0-d032-11ec-9d64-0242ac120002"), "Cl2-CX-Portal"),
            new (new Guid("f032a034-d035-11ec-9d64-0242ac120002"), "Cl1-CX-Registration"),
            new (new Guid("cf207afb-d213-4c33-becc-0cabeef174a7"), "https://catenax-int-dismantler-s66pftcc.authentication.eu10.hana.ondemand.com"),
        });

        dbContext.Offers.AddRange(OfferData.Offers);
        dbContext.OfferLicenses.AddRange(new List<OfferLicense>
        {
            new (new Guid("6ca00fc6-4c82-47d8-8616-059ebe65232b"), "43")
        });
        dbContext.OfferAssignedLicenses.AddRange(new List<OfferAssignedLicense>
        {
            new (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("6ca00fc6-4c82-47d8-8616-059ebe65232b"))
        });
        dbContext.OfferDescriptions.AddRange(new OfferDescription[] {
            new (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), "en", "some long Description", "some short Description"),
            new (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), "de", "eine lange Beschreibung", "eine kurze Beschreibung"),
        });
        dbContext.AppInstances.AddRange(AppInstanceData.AppInstances);
        dbContext.OfferSubscriptions.AddRange(new List<OfferSubscription>
        {
            new (new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferSubscriptionStatusId.ACTIVE, new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001")),
            new (new Guid("28149c6d-833f-49c5-aea2-ab6a5a37f462"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), OfferSubscriptionStatusId.ACTIVE, new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"))
        });
        dbContext.AppSubscriptionDetails.AddRange(new List<AppSubscriptionDetail>
        {
            new(new Guid("c4e979f7-d62d-4705-9218-dea4a28d3369"), new Guid("eb98bdf5-14e1-4feb-a954-453eac0b93cd"))
            {
                AppInstanceId = new Guid("89FF0C72-052F-4B1D-B5D5-89F3D61BA0B1"),
                AppSubscriptionUrl = "https://url.test-app.com"
            }
        });
        dbContext.AppLanguages.AddRange(new List<AppLanguage>
        {
            new (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "de"),
            new (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "en")
        });
        dbContext.AppAssignedUseCases.AddRange(new List<AppAssignedUseCase>
        {
            new(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("06b243a4-ba51-4bf3-bc40-5d79a2231b90"))
        });

        dbContext.ServiceAssignedServiceTypes.AddRange(new List<ServiceAssignedServiceType>
        {
            new (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), ServiceTypeId.CONSULTANCE_SERVICE)
        });

        dbContext.OfferAssignedDocuments.AddRange(new List<OfferAssignedDocument>
        {
            new (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("7fc2fb78-8dc2-4f5f-b1d1-91c9c2f4506f")),
            new (new Guid("99c5fd12-8085-4de2-abfd-215e1ee4baa4"), new Guid("90a24c6d-1092-4590-ae89-a9d2bff1ea41")),
            new (new Guid("99c5fd12-8085-4de2-abfd-215e1ee4baa4"), new Guid("2b9e45a6-ec22-489a-b2cb-9cbdd0b6bfbc")),
            new (new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), new Guid("b42de0f5-fd35-428a-86eb-a048ed7e57fa"))
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
                DocumentId = new Guid("fda6c9cb-62be-4a98-99c1-d9c5a2df4aad")
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019951"), AgreementCategoryId.DATA_CONTRACT, "Test Agreement", DateTimeOffset.UtcNow)
            {
                IssuerCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
                DocumentId = new Guid("fda6c9cb-62be-4a98-99c1-d9c5a2df4aad")
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019952"), AgreementCategoryId.APP_CONTRACT, "App Agreement", DateTimeOffset.UtcNow)
            {
                IssuerCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
                DocumentId = new Guid("90a24c6d-1092-4590-ae89-a9d2bff1ea40")
            },
            new(new Guid("979a29b1-40c2-4169-979c-43c3156dbf64"), AgreementCategoryId.SERVICE_CONTRACT, "Service Agreement", DateTimeOffset.UtcNow)
            {
                IssuerCompanyId = new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"),
                DocumentId = new Guid("90a24c6d-1092-4590-ae89-a9d2bff1ea40")
            }
        });
        dbContext.Consents.AddRange(new List<Consent>
        {
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019910"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019951"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), ConsentStatusId.INACTIVE, DateTimeOffset.UtcNow)
            {
                Comment = "Just a test"
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019911"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058019952"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), ConsentStatusId.INACTIVE, DateTimeOffset.UtcNow)
            {
                Comment = "Test"
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019912"), new Guid("f6d3148b-2e2b-4688-a382-326d4232ee6e"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), ConsentStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                Comment = "Test"
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

        dbContext.CountryAssignedIdentifiers.AddRange(new CountryAssignedIdentifier[]
        {
            new ("DE", UniqueIdentifierId.COMMERCIAL_REG_NUMBER),
            new ("DE", UniqueIdentifierId.VAT_ID) { BpdmIdentifierId = BpdmIdentifierId.EU_VAT_ID_DE },
        });

        dbContext.CompanyIdentifiers.AddRange(new CompanyIdentifier[]
        {
            new (new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), UniqueIdentifierId.COMMERCIAL_REG_NUMBER, "REG08154711"),
            new (new Guid("27538eac-27a3-4f74-9306-e5149b93ade5"), UniqueIdentifierId.VAT_ID, "DE123456789")
        });
    };
}

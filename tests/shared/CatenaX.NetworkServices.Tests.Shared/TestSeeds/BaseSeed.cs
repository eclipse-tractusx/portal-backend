using CatenaX.NetworkServices.PortalBackend.PortalEntities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Tests.Shared.TestSeeds;

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
            }
        });

        dbContext.CompanyUsers.AddRange(new List<CompanyUser>
        {
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "cx-user-2",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "cx-admin-2",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "company-admin-2",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "it-admin-2",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "CX User",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
            new(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("2dc4249f-b5ca-4d42-bef1-7a7a950a4f87"), CompanyUserStatusId.ACTIVE, DateTimeOffset.UtcNow)
            {
                Email = "tester.user@test.de",
                Firstname = "Test User",
                Lastname = "CX Admin",
                CompanyUserStatusId = CompanyUserStatusId.ACTIVE
            },
        });

        dbContext.IamUsers.AddRange(new List<IamUser>
        {
            new ("623770c5-cf38-4b9f-9a35-f8b9ae972e2d", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000")),
            new ("3d8142f1-860b-48aa-8c2b-1ccb18699f65", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001")),
            new ("623770c5-cf38-4b9f-9a35-f8b9ae972e2e", new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990")),
            new ("3d8142f1-860b-48aa-8c2b-1ccb18699f66", new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991")),
            new ("47ea7f1f-f10d-4cb2-acaf-b77323ef25b4", new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992"))
        });

        dbContext.IamClients.AddRange(new List<IamClient>
        {
            new (new Guid("0c9051d0-d032-11ec-9d64-0242ac120002"), "Cl2-CX-Portal"),
            new (new Guid("f032a034-d035-11ec-9d64-0242ac120002"), "Cl1-CX-Registration"),
            new (new Guid("cf207afb-d213-4c33-becc-0cabeef174a7"), "https://catenax-int-dismantler-s66pftcc.authentication.eu10.hana.ondemand.com"),
        });

        dbContext.UserRoles.AddRange(new List<UserRole>
        {
            new (new Guid("efc20368-9e82-46ff-b88f-6495b9810253"), "EarthCommerce.AdministratorRC_QAS2", new Guid("cf207afb-d213-4c33-becc-0cabeef174a7")),
            new (new Guid("aabcdfeb-6669-4c74-89f0-19cda090873f"), "EarthCommerce.Advanced.BuyerRC_QAS2", new Guid("cf207afb-d213-4c33-becc-0cabeef174a7")),
            new (new Guid("58f897ec-0aad-4588-8ffa-5f45d6638633"), "CX User", new Guid("0c9051d0-d032-11ec-9d64-0242ac120002")),
            new (new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632"), "CX Admin", new Guid("0c9051d0-d032-11ec-9d64-0242ac120002")),
            new (new Guid("7410693c-c893-409e-852f-9ee886ce94a6"), "Company Admin", new Guid("f032a034-d035-11ec-9d64-0242ac120002")),
            new (new Guid("607818be-4978-41f4-bf63-fa8d2de51154"), "IT Admin", new Guid("0c9051d0-d032-11ec-9d64-0242ac120002")),
        });

        dbContext.CompanyUserAssignedRoles.AddRange(new List<CompanyUserAssignedRole>
        {
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), new Guid("efc20368-9e82-46ff-b88f-6495b9810253")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), new Guid("aabcdfeb-6669-4c74-89f0-19cda090873f")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058020000"), new Guid("58f897ec-0aad-4588-8ffa-5f45d6638633")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("efc20368-9e82-46ff-b88f-6495b9810253")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), new Guid("aabcdfeb-6669-4c74-89f0-19cda090873f")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019990"), new Guid("58f897ec-0aad-4588-8ffa-5f45d6638633")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019991"), new Guid("58f897ec-0aad-4588-8ffa-5f45d6638632")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019992"), new Guid("7410693c-c893-409e-852f-9ee886ce94a6")),
            new (new Guid("ac1cf001-7fbc-1f2f-817f-bce058019993"), new Guid("607818be-4978-41f4-bf63-fa8d2de51154"))
        });
    };

}
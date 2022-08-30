using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;
using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;

namespace CatenaX.NetworkServices.Tests.Shared.TestSeeds;

public static class AppData
{
    public static readonly App[] Apps = {
        new(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), "Catena X", DateTimeOffset.UtcNow, AppTypeId.APP)
        {
            AppStatusId = AppStatusId.ACTIVE,
        },
        new(new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), "Catena X", DateTimeOffset.UtcNow, AppTypeId.SERVICE)
        {
            Name = "Newest Service",
            ContactEmail = "service-test@mail.com",
            AppStatusId = AppStatusId.ACTIVE,
        }
    };
}
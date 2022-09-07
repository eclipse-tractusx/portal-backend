using CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

namespace CatenaX.NetworkServices.Tests.Shared.TestSeeds;

public static class AppInstanceData
{
    public static readonly List<AppInstance> AppInstances = new()
    {
        new (new Guid("89FF0C72-052F-4B1D-B5D5-89F3D61BA0B1"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("0c9051d0-d032-11ec-9d64-0242ac120002")),
        new (new Guid("B87F5778-928B-4375-B653-0D6F28E2A1C1"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("f032a034-d035-11ec-9d64-0242ac120002")),
        new (new Guid("C398F1E9-92A2-4C76-89DC-062FBD7CA6F1"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA4"), new Guid("cf207afb-d213-4c33-becc-0cabeef174a7")),
        new (new Guid("C398F1E9-92A2-4C76-89DC-062FBD7CA6F2"), new Guid("99C5FD12-8085-4DE2-ABFD-215E1EE4BAA5"), new Guid("cf207afb-d213-4c33-becc-0cabeef174a7")),
    };
}
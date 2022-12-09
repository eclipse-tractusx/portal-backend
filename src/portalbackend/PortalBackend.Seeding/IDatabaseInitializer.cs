namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

public interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
}
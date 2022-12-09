namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

public interface ICustomSeeder
{
    Task InitializeAsync(CancellationToken cancellationToken);
}
namespace Org.Eclipse.TractusX.Portal.Backend.Keycloak.Seeding.Extensions;

public static class ParallelOptionsExtensions
{
    public static ParallelOptions CreateParallelOptions(CancellationToken cancellationToken) =>
        new() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1, CancellationToken = cancellationToken };
}

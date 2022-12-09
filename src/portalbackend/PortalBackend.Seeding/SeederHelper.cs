using System.Reflection;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

public static class SeederHelper
{
    public static async Task<IList<T>?> GetSeedData<T>(CancellationToken cancellationToken) where T : class
    {
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = $"{location}/Seeder/Data/{nameof(T)}.json";
        if (!File.Exists(path))
        {
            return null;
        }

        var data = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<List<T>>(data);
    }
}
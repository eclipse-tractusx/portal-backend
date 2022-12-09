using System.Reflection;
using System.Text.Json;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.Seeding;

public static class SeederHelper
{
    public static async Task<IList<T>?> GetSeedData<T>(CancellationToken cancellationToken) where T : class
    {
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (location == null)
        {
            throw new ConflictException($"No location found for assembly {Assembly.GetExecutingAssembly()}");
        }

        var path = Path.Combine(location, @"Seeder\Data", $"{typeof(T).Name.ToLower()}.json");
        if (!File.Exists(path))
        {
            return null;
        }

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonDateTimeOffsetConverter());

        var data = await File.ReadAllTextAsync(path, cancellationToken);
        return JsonSerializer.Deserialize<List<T>>(data, options);
    }
}
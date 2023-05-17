using Newtonsoft.Json.Serialization;
using System.Text.Json;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding.JsonHelper;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    private readonly SnakeCaseNamingStrategy _newtonsoftSnakeCaseNamingStrategy = new();

    public static SnakeCaseNamingPolicy Instance { get; } = new();

    public override string ConvertName(string name)
    {
        /* A conversion to snake case implementation goes here. */
        return _newtonsoftSnakeCaseNamingStrategy.GetPropertyName(name, false);
    }
}

using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Extensions;

public static class AssemblyExtension
{
    public static string GetApplicationVersion() =>
        $"v{Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split("+")[0]}";
}

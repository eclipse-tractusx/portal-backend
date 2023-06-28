using System.Reflection;
using System.Runtime.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Models;

public static class EnumExtensions
{
    public static string GetEnumValue(this Enum value)
    {
        return value.GetType()
            .GetTypeInfo()
            .DeclaredMembers
            .SingleOrDefault(x => x.Name == value.ToString())
            ?.GetCustomAttribute<EnumMemberAttribute>(false)
            ?.Value ?? value.ToString();
    }
}

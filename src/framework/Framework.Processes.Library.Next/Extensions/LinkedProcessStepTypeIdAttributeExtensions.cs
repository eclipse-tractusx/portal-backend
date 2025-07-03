using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Attributes;
using System.Reflection;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Extensions;

public static class LinkedProcessStepTypeIdAttributeExtensions
{
    public static Type GetLinkedProcessStepTypeIdType<TProcessTypeId>(this TProcessTypeId processTypeId)
        where TProcessTypeId : Enum
    {
        var enumType = typeof(TProcessTypeId);

        if (!enumType.IsEnum)
        {
            throw new ArgumentException($"{enumType} is not an enum");
        }

        var field = enumType.GetField(processTypeId.ToString());
        if (field == null)
        {
            throw new ArgumentException($"The value '{processTypeId}' is not a valid member of the enum {enumType}");
        }

        var attribute = field
            .GetCustomAttributes()
            .FirstOrDefault(attr => attr.GetType().IsGenericType
                                    && attr.GetType().GetGenericTypeDefinition() == typeof(LinkedProcessStepTypeIdAttribute<>));
        var genericArguments = attribute?.GetType().GetGenericArguments();
        if (genericArguments?.Length != 1)
        {
            throw new ArgumentException($"Only one generic argument is expected for the attribute {attribute}, but found {genericArguments?.Length}");
        }

        var processStepTypeIdType = genericArguments.Single();
        if (!processStepTypeIdType.IsEnum)
        {
            throw new ArgumentException($"{processStepTypeIdType} is not an enum");
        }

        return processStepTypeIdType;
    }
}

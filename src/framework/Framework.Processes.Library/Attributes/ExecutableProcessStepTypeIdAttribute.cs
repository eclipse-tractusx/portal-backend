namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ExecutableProcessStepTypeIdAttribute<TProcessTypeId>(params TProcessTypeId[] processTypeIds) : Attribute
    where TProcessTypeId : struct, IConvertible
{
    public TProcessTypeId[] ProcessTypeIds { get; } = processTypeIds;
}

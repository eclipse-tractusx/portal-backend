namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class LinkedProcessStepTypeIdAttribute<TProcessStepTypeId> : Attribute
    where TProcessStepTypeId : struct, IConvertible
{
    public TProcessStepTypeId Value { get; }
}

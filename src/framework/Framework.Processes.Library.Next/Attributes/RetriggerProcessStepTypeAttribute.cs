namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Next.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class RetriggerProcessStepTypeAttribute<TProcessStepTypeId>(TProcessStepTypeId processStepTypeId) : Attribute
    where TProcessStepTypeId : Enum
{
    public TProcessStepTypeId ProcessStepTypeId { get; } = processStepTypeId;
}

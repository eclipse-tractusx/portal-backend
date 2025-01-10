using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;

public interface IProcessStep<out TProcessStepTypeId>
    where TProcessStepTypeId : struct, IConvertible
{
    Guid Id { get; }
    TProcessStepTypeId ProcessStepTypeId { get; }
    ProcessStepStatusId ProcessStepStatusId { get; set; }
    Guid ProcessId { get; }
    DateTimeOffset DateCreated { get; }
    DateTimeOffset? DateLastChanged { get; set; }
    string? Message { get; set; }
}

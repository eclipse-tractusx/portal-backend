using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;

public interface IProcessStepStatus
{
    ProcessStepStatusId Id { get; }
    string Label { get; }
}

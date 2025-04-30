using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library.Extensions;

public static class ProcessTypeExecutorExtensions
{
    public static bool IsExecutableStepTypeId<TProcessTypeId>(this IProcessTypeExecutor<TProcessTypeId> executor, int processStepTypeId)
        where TProcessTypeId : struct, Enum =>
        executor.GetProcessTypeId().GetExecutableProcessStepTypeIdsForProcessType()
            .Contains(processStepTypeId);
}

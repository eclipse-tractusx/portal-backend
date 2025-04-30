
using System.Reflection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Attributes;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Extensions;

public static class ExecutableProcessStepTypeIdAttributeExtensions
{
    public static IEnumerable<TProcessStepTypeId> GetExecutableProcessStepTypeIdsForProcessType<TProcessTypeId, TProcessStepTypeId>(this TProcessTypeId processTypeId)
        where TProcessTypeId : struct, Enum
        where TProcessStepTypeId : struct, Enum
    {
        var processStepTypeIdType = processTypeId.GetLinkedProcessStepTypeIdType();
        if (processStepTypeIdType != typeof(TProcessStepTypeId))
        {
            throw new ArgumentException($"The processStepTypeIdType {processStepTypeIdType} must be same as {typeof(TProcessStepTypeId)}");
        }

        return Enum.GetValues(typeof(TProcessStepTypeId))
            .Cast<TProcessStepTypeId>()
            .Where(stepTypeId =>
            {
                var attribute = typeof(TProcessStepTypeId).GetMember(stepTypeId.ToString()).FirstOrDefault()?.GetCustomAttributes(typeof(ExecutableProcessStepTypeIdAttribute<TProcessTypeId>), false)
                    .Cast<ExecutableProcessStepTypeIdAttribute<TProcessTypeId>>()
                    .FirstOrDefault();

                return attribute?.ProcessTypeIds.Contains(processTypeId) ?? false;
            });
    }

    public static IEnumerable<int> GetExecutableProcessStepTypeIdsForProcessType<TProcessTypeId>(this TProcessTypeId processTypeId)
        where TProcessTypeId : struct, Enum
    {
        var processStepTypeIdType = processTypeId.GetLinkedProcessStepTypeIdType();

        return Enum.GetValues(processStepTypeIdType)
            .Cast<Enum>()
            .Where(stepTypeId =>
            {
                var attribute = processStepTypeIdType.GetMember(stepTypeId.ToString()).FirstOrDefault()?.GetCustomAttributes(typeof(ExecutableProcessStepTypeIdAttribute<TProcessTypeId>), false)
                    .Cast<ExecutableProcessStepTypeIdAttribute<TProcessTypeId>>()
                    .FirstOrDefault();

                return attribute?.ProcessTypeIds.Contains(processTypeId) ?? false;
            })
            .Select(Convert.ToInt32);
    }

    public static TProcessStepTypeId? GetRetriggerStep<TProcessStepTypeId>(this TProcessStepTypeId processStepTypeId)
        where TProcessStepTypeId : struct, Enum
    {
        var attribute = processStepTypeId.GetType().GetMember(processStepTypeId.ToString()).FirstOrDefault()?.GetCustomAttribute<RetriggerProcessStepTypeAttribute<TProcessStepTypeId>>();
        return attribute?.ProcessStepTypeId;
    }

    public static (TProcessTypeId ProcessTypeId, TProcessStepTypeId ProcessStepTypeId) GetStepToRetrigger<TProcessTypeId, TProcessStepTypeId>(this TProcessStepTypeId processStepTypeId)
        where TProcessTypeId : struct, Enum
        where TProcessStepTypeId : struct, Enum
    {
        var retriggerStepsTypeIds = Enum.GetValues<TProcessStepTypeId>()
            .Where(x =>
            {
                var stepTypeId = x.GetType().GetMember(x.ToString()).FirstOrDefault()
                    ?.GetCustomAttribute<RetriggerProcessStepTypeAttribute<TProcessStepTypeId>>()
                    ?.ProcessStepTypeId;
                return stepTypeId != null && stepTypeId.Value.Equals(processStepTypeId);
            });

        if (retriggerStepsTypeIds.Count() != 1)
        {
            throw new UnexpectedConditionException($"{processStepTypeId} must only have one retrigger step");
        }

        var processTypeIds = Enum.GetValues<TProcessTypeId>()
            .Where(x =>
            {
                var stepTypeId = x.GetType().GetMember(x.ToString()).FirstOrDefault()
                    ?.GetCustomAttribute<LinkedProcessStepTypeIdAttribute<TProcessStepTypeId>>()
                    ?.Value;
                return stepTypeId != null && stepTypeId.GetType() == typeof(TProcessStepTypeId);
            });
        if (processTypeIds.Count() != 1)
        {
            throw new UnexpectedConditionException($"{typeof(TProcessStepTypeId)} must be linked to a {typeof(TProcessTypeId)}");
        }

        return (processTypeIds.Single(), retriggerStepsTypeIds.Single());
    }
}

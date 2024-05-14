using Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Extensions;

public static class ProcessTypeExtensions
{
    public static ProcessStepTypeId GetInitialProcessStepTypeIdForSaCreation(this ProcessTypeId processTypeId) =>
        processTypeId switch
        {
            ProcessTypeId.DIM_TECHNICAL_USER => ProcessStepTypeId.CREATE_DIM_TECHNICAL_USER,
            ProcessTypeId.OFFER_SUBSCRIPTION => ProcessStepTypeId.OFFERSUBSCRIPTION_CREATE_DIM_TECHNICAL_USER,
            _ => throw new ArgumentException($"ProcessType {processTypeId} is not supported")
        };
}

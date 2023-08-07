using System.Runtime.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

public enum VerifiedCredentialExternalTypeId
{
    [EnumMember(Value = "TraceabilityCredential")]
    TRACEABILITY_CREDENTIAL = 1,

    [EnumMember(Value = "PcfCredential")]
    PCF_CREDENTIAL = 2,

    [EnumMember(Value = "BehaviorTwinCredential")]
    BEHAVIOR_TWIN_CREDENTIAL = 3,

    [EnumMember(Value = "vehicleDismantle")]
    VEHICLE_DISMANTLE = 4,

    [EnumMember(Value = "Sustainability_Credential")]
    SUSTAINABILITY_CREDENTIAL = 5,
}

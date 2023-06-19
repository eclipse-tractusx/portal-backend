using System.Runtime.Serialization;

namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Enums;

public enum VerifiedCredentialExternalTypeId
{
    [EnumMember(Value = "TraceabilityCredential")]
    TRACEABILITY_CREDENTIAL = 1,

    [EnumMember(Value = "SustainabilityCredential")]
    SUSTAINABILITY_CREDENTIAL = 2,

    [EnumMember(Value = "vehicleDismantle")]
    VEHICLE_DISMANTLE = 3,

    [EnumMember(Value = "PcfCredential")]
    PCF_CREDENTIAL = 4,

    [EnumMember(Value = "QualityCredential")]
    QUALITY_CREDENTIAL = 5,

    [EnumMember(Value = "BehaviorTwinCredential")]
    BEHAVIOR_TWIN_CREDENTIAL = 6,
}

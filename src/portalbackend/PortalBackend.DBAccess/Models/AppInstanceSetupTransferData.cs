namespace PortalBackend.DBAccess.Models;

public record AppInstanceSetupTransferData(
	Guid Id,
	bool IsSingleInstance,
	string? InstanceUrl
);

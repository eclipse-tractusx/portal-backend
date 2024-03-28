namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.DBAccess.Models;

public record WalletInformation(
    string ClientId,
    byte[] ClientSecret,
    byte[]? InitializationVector,
    int EncryptionMode,
    string WalletUrl
);

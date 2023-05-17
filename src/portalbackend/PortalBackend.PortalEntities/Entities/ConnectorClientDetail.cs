namespace Org.Eclipse.TractusX.Portal.Backend.PortalBackend.PortalEntities.Entities;

public class ConnectorClientDetail
{
    private ConnectorClientDetail()
    {
        ClientId = null!;
    }

    public ConnectorClientDetail(Guid connectorId, string clientId)
        : this()
    {
        this.ConnectorId = connectorId;
        this.ClientId = clientId;
    }

    public Guid ConnectorId { get; set; }

    public string ClientId { get; set; }

    public virtual Connector? Connector { get; set; }
}

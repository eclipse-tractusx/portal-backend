using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class ConnectorStatus
{
    private ConnectorStatus()
    {
        Label = null!;
        Connectors = new HashSet<Connector>();
    }

    public ConnectorStatus(ConnectorStatusId connectorStatusId) : this()
    {
        Id = connectorStatusId;
        Label = connectorStatusId.ToString();
    }

    public ConnectorStatusId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Connector> Connectors { get; private set; }
}

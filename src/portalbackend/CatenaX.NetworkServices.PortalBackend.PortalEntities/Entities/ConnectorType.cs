using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class ConnectorType
{
    private ConnectorType()
    {
        Label = null!;
        Connectors = new HashSet<Connector>();
    }

    public ConnectorType(ConnectorTypeId connectorTypeId) : this()
    {
        Id = connectorTypeId;
        Label = connectorTypeId.ToString();
    }

    public ConnectorTypeId Id { get; private set; }

    [MaxLength(255)]
    public string Label { get; private set; }

    // Navigation properties
    public virtual ICollection<Connector> Connectors { get; private set; }
}

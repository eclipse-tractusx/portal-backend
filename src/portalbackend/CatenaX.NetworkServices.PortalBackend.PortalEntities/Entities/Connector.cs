using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.PortalBackend.PortalEntities.Entities;

public class Connector
{
    public Connector(string name, string locationId, string connectorUrl)
    {
        Name = name;
        LocationId = locationId;
        ConnectorUrl = connectorUrl;
    }

    public Guid Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string ConnectorUrl { get; set; }

    public ConnectorTypeId TypeId { get; set; }

    public ConnectorStatusId StatusId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid? HostId { get; set; }

    [StringLength(2, MinimumLength = 2)]
    public string LocationId { get; set; }

    // Navigation properties
    public ConnectorType? Type { get; set; }
    public ConnectorStatus? Status { get; set; }
    public Company? Provider { get; set; }
    public Company? Host { get; set; }
    public Country? Location { get; set; }
}

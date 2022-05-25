using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Administration.Service.Models;

/// <summary>
/// Input model defining all parameters for creating a connector in persistence layer.
/// </summary>
public class ConnectorInputModel
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">Connector name.</param>
    /// <param name="location">Connector location country code.</param>
    /// <param name="connectorUrl">Connector URL.</param>
    public ConnectorInputModel(string name, string location, string connectorUrl)
    {
        Name = name;
        Location = location;
        ConnectorUrl = connectorUrl;
    }

    /// <summary>
    /// Display name of the connector.
    /// </summary>
    [MaxLength(255)]
    public string Name { get; set; }

    /// <summary>
    /// URL of the connector.
    /// </summary>
    [MaxLength(255)]
    public string ConnectorUrl { get; set; }

    /// <summary>
    /// Connector type.
    /// </summary>
    public ConnectorTypeId Type { get; set; }

    /// <summary>
    /// Connector status.
    /// </summary>
    public ConnectorStatusId Status { get; set; }

    /// <summary>
    /// Connector's location country code.
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Location { get; set; }

    /// <summary>
    /// Providing company's ID.
    /// </summary>
    public Guid Provider { get; set; }

    /// <summary>
    /// Hosting company's ID.
    /// </summary>
    public Guid? Host { get; set; }
}

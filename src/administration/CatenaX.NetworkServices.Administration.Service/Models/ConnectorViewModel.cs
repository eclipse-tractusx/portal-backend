using CatenaX.NetworkServices.PortalBackend.PortalEntities.Enums;
using System.ComponentModel.DataAnnotations;

namespace CatenaX.NetworkServices.Administration.Service.Models;

/// <summary>
/// View model for connectors.
/// </summary>
public class ConnectorViewModel
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="location">Location.</param>
    public ConnectorViewModel(string name, string location)
    {
        Name = name;
        Location = location;
    }

    /// <summary>
    /// ID of the connector.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the connector.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Connector type.
    /// </summary>
    public ConnectorTypeId Type { get; set; }

    /// <summary>
    /// Country code of the connector's location.
    /// </summary>
    [StringLength(2, MinimumLength = 2)]
    public string Location { get; set; }

    /// <summary>
    /// Connector status.
    /// </summary>
    public ConnectorStatusId Status { get; set; }
}

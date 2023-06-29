using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

public class SeederSettings
{
    public SeederSettings()
    {
        TestDataEnvironments = null!;
        DataPaths = null!;
    }

    /// <summary>
    /// Configurable paths where to check for the seeding data
    /// </summary>
    [Required]
    public IEnumerable<string> DataPaths { get; set; }

    /// <summary>
    /// Configurable environments for the testdata
    /// </summary>
    [Required]
    public IEnumerable<string> TestDataEnvironments { get; set; }
}

namespace Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

public class SeederSettings
{
    /// <summary>
    /// Configurable paths where to check for the seeding data
    /// </summary>
    public List<string> DataPaths { get; set; } = new();

    /// <summary>
    /// Configurable environments for the testdata
    /// </summary>
    public List<string> TestDataEnvironments { get; set; } = new();
}

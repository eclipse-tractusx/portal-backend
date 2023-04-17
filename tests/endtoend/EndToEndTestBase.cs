using Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;
using Xunit.Abstractions;

namespace EndToEnd.Tests;

//prepare xUnit logging for every e2e test according to https://github.com/basdijkstra/rest-assured-net/wiki/Usage-Guide#logging-when-using-xunit
public abstract class EndToEndTestBase
{
    protected EndToEndTestBase(ITestOutputHelper output)
    {
        Console.SetOut(new ConsoleWriter(output));
    }
}

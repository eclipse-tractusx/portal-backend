using Xunit.Abstractions;

namespace Org.Eclipse.TractusX.Portal.Backend.EndToEnd.Tests;

public class ConsoleWriter : StringWriter
{
    private readonly ITestOutputHelper _output;

    public ConsoleWriter(ITestOutputHelper output)
    {
        this._output = output;
    }

    public override void WriteLine(string? m)
    {
        _output.WriteLine(m);
    }
}

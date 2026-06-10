using AcpNet.Process;
using Xunit;

namespace Acp.Net.UnitTests;

public sealed class AcpPathMapperTests
{
    [Fact]
    public void ToWslPath_MapsWslLocalhostUncPath()
    {
        var path = @"\\wsl.localhost\Ubuntu\home\mertb\agent.py";

        var result = AcpPathMapper.ToWslPath(path);

        Assert.Equal("/home/mertb/agent.py", result);
    }

    [Fact]
    public void ToWslPath_MapsWindowsDrivePath()
    {
        var path = @"C:\Users\mertb\agent.py";

        var result = AcpPathMapper.ToWslPath(path);

        Assert.Equal("/mnt/c/Users/mertb/agent.py", result);
    }

    [Fact]
    public void ToWslPath_KeepsLinuxPath()
    {
        var path = "/home/mertb/agent.py";

        var result = AcpPathMapper.ToWslPath(path);

        Assert.Equal(path, result);
    }
}

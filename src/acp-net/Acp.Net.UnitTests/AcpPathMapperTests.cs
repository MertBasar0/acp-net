using AcpNet.Process;
using Xunit;

namespace Acp.Net.UnitTests;

public sealed class AcpPathMapperTests
{
    [Fact]
    public void ToWslPath_MapsWslLocalhostUncPath()
    {
        var path = @"\\wsl.localhost\Ubuntu\home\user\agent.py";

        var result = AcpPathMapper.ToWslPath(path);

        Assert.Equal("/home/user/agent.py", result);
    }

    [Fact]
    public void ToWslPath_MapsWindowsDrivePath()
    {
        var path = @"C:\Users\user\agent.py";

        var result = AcpPathMapper.ToWslPath(path);

        Assert.Equal("/mnt/c/Users/user/agent.py", result);
    }

    [Fact]
    public void ToWslPath_KeepsLinuxPath()
    {
        var path = "/home/user/agent.py";

        var result = AcpPathMapper.ToWslPath(path);

        Assert.Equal(path, result);
    }
}

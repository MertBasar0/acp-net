using AcpNet.Process;
using Xunit;

namespace Acp.Net.UnitTests;

public sealed class AcpRuntimeResolverTests
{
    [Fact]
    public void Resolve_ForcedNative_UsesOriginalCommand()
    {
        var resolved = AcpRuntimeResolver.Resolve(new AcpProcessOptions
        {
            Command = "python3",
            Arguments = ["/tmp/agent.py"],
            Runtime = AcpRuntime.Native
        });

        Assert.False(resolved.UsesWsl);
        Assert.Equal("python3", resolved.StartInfo.FileName);
        Assert.Equal("/tmp/agent.py", resolved.StartInfo.Arguments);
    }

    [Fact]
    public void Resolve_ForcedWsl_BuildsWslExeCommand()
    {
        var resolved = AcpRuntimeResolver.Resolve(new AcpProcessOptions
        {
            Command = "python3",
            Arguments = [@"\\wsl.localhost\Ubuntu\home\mertb\agent.py"],
            Runtime = AcpRuntime.Wsl,
            WslDistribution = "Ubuntu"
        });

        Assert.True(resolved.UsesWsl);
        Assert.Equal("wsl.exe", resolved.StartInfo.FileName);
        Assert.Contains("-d Ubuntu", resolved.StartInfo.Arguments);
        Assert.Contains("/home/mertb/agent.py", resolved.StartInfo.Arguments);
    }

    [Fact]
    public void Resolve_AdditionalPathEntries_PrependsMappedEntries()
    {
        var resolved = AcpRuntimeResolver.Resolve(new AcpProcessOptions
        {
            Command = "gemini",
            Arguments = ["--acp"],
            Runtime = AcpRuntime.Wsl,
            AdditionalPathEntries = [@"\\wsl.localhost\Ubuntu\home\mertb\bin", "/opt/tools"]
        });

        Assert.True(resolved.StartInfo.Environment.TryGetValue("PATH", out var path));
        Assert.StartsWith("/home/mertb/bin:/opt/tools:", path);
    }

    [Fact]
    public void Resolve_Environment_CanSetAndRemoveVariables()
    {
        var resolved = AcpRuntimeResolver.Resolve(new AcpProcessOptions
        {
            Command = "python3",
            Runtime = AcpRuntime.Native,
            Environment = new Dictionary<string, string?>
            {
                ["ACP_TEST_ENV"] = "1",
                ["ACP_TEST_REMOVE"] = null
            }
        });

        Assert.Equal("1", resolved.StartInfo.Environment["ACP_TEST_ENV"]);
        Assert.False(resolved.StartInfo.Environment.ContainsKey("ACP_TEST_REMOVE"));
    }
}

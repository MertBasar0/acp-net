using AcpNet.Process;
using Xunit;

namespace Acp.Net.UnitTests;

public sealed class AcpWindowsExecutableProbeTests
{
    [Theory]
    [InlineData(@"C:\Users\dev\AppData\Local\Microsoft\WindowsApps\python3.exe")]
    [InlineData(@"C:\Program Files\WindowsApps\python3.exe")]
    [InlineData("C:/Users/dev/AppData/Local/Microsoft/WindowsApps/python3.exe")]
    public void IsExecutionAliasStubPath_DetectsZeroLengthWindowsAppsStub(string path)
    {
        Assert.True(AcpWindowsExecutableProbe.IsExecutionAliasStubPath(path, fileLength: 0));
    }

    [Theory]
    [InlineData(@"C:\Python312\python3.exe")]
    [InlineData(@"C:\Program Files\Git\bin\git.exe")]
    [InlineData("/usr/bin/python3")]
    public void IsExecutionAliasStubPath_IgnoresRealExecutablePaths(string path)
    {
        Assert.False(AcpWindowsExecutableProbe.IsExecutionAliasStubPath(path, fileLength: 0));
    }

    [Fact]
    public void IsExecutionAliasStubPath_RequiresZeroLength()
    {
        // A non-zero length under WindowsApps is a real packaged executable, not the
        // execution-alias stub.
        Assert.False(AcpWindowsExecutableProbe.IsExecutionAliasStubPath(
            @"C:\Users\dev\AppData\Local\Microsoft\WindowsApps\python3.exe",
            fileLength: 4096));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsExecutionAliasStubPath_HandlesEmptyInput(string? path)
    {
        Assert.False(AcpWindowsExecutableProbe.IsExecutionAliasStubPath(path, fileLength: 0));
    }
}

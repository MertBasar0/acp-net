using AcpNet.Process;
using Xunit;

namespace Acp.Net.UnitTests;

public sealed class AcpPreflightCheckerTests
{
    [Fact]
    public async Task CheckRequiredExecutablesAsync_FindsNativeExecutable()
    {
        var options = new AcpProcessOptions
        {
            Command = "python3",
            Runtime = AcpRuntime.Native,
            RequiredExecutables = [OperatingSystem.IsWindows() ? "cmd" : "sh"]
        };
        var resolved = AcpRuntimeResolver.Resolve(options);

        var results = await AcpPreflightChecker.CheckRequiredExecutablesAsync(options, resolved, CancellationToken.None);

        Assert.Single(results);
        Assert.True(results[0].Found);
        Assert.NotNull(results[0].Path);
    }

    [Fact]
    public async Task CheckRequiredExecutablesAsync_ReportsMissingExecutable()
    {
        var options = new AcpProcessOptions
        {
            Command = "python3",
            Runtime = AcpRuntime.Native,
            RequiredExecutables = ["definitely-not-a-real-acp-tool"]
        };
        var resolved = AcpRuntimeResolver.Resolve(options);

        var results = await AcpPreflightChecker.CheckRequiredExecutablesAsync(options, resolved, CancellationToken.None);

        Assert.Single(results);
        Assert.False(results[0].Found);
        Assert.False(results[0].IsFailure);
        Assert.Equal(AcpMissingExecutablePolicy.Warn, results[0].MissingPolicy);
    }

    [Fact]
    public async Task CheckRequiredExecutablesAsync_MarksThrowPolicyAsFailure()
    {
        var options = new AcpProcessOptions
        {
            Command = "python3",
            Runtime = AcpRuntime.Native,
            RequiredTools = [AcpRequiredExecutable.Throw("definitely-not-a-real-acp-required-tool")]
        };
        var resolved = AcpRuntimeResolver.Resolve(options);

        var results = await AcpPreflightChecker.CheckRequiredExecutablesAsync(options, resolved, CancellationToken.None);

        Assert.Single(results);
        Assert.False(results[0].Found);
        Assert.True(results[0].IsFailure);
        Assert.Equal(AcpMissingExecutablePolicy.Throw, results[0].MissingPolicy);
    }
}

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Acp.Net.IntegrationTests;

/// <summary>
/// Verifies the diagnostic command contract of the openclaw-acpnet-probe sample:
/// stdout carries exactly one JSON result and the exit codes follow the documented mapping
/// (0 ok, 2 environment/preflight failure, 64 invalid CLI configuration).
/// </summary>
public sealed class OpenClawProbeContractTests : IClassFixture<OpenClawProbeContractTests.ProbeBuildFixture>
{
    readonly ProbeBuildFixture fixture;

    public OpenClawProbeContractTests(ProbeBuildFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task InvalidConfiguration_Exits64_WithSingleJsonResult()
    {
        var run = await RunProbeAsync(["--arg", "orphan"], TimeSpan.FromSeconds(60));

        Assert.Equal(64, run.ExitCode);
        using var json = ParseSingleJson(run.Stdout);
        Assert.Equal("openclaw.acpnet.probe.result", json.RootElement.GetProperty("kind").GetString());
        Assert.False(json.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("ConfigurationFailure", json.RootElement.GetProperty("failureKind").GetString());
        Assert.Equal("--arg requires --command.", json.RootElement.GetProperty("failureMessage").GetString());
    }

    [Fact]
    public async Task MissingCriticalTool_Exits2_WithEnvironmentFailure()
    {
        var artifactDir = PrepareArtifactDir("probe-env-failure");
        var run = await RunProbeAsync(
            ["--required-tool", "definitely-not-a-real-acpnet-critical-tool", "--artifact-dir", artifactDir],
            TimeSpan.FromSeconds(120));

        Assert.Equal(2, run.ExitCode);
        using var json = ParseSingleJson(run.Stdout);
        Assert.Equal("openclaw.acpnet.probe.result", json.RootElement.GetProperty("kind").GetString());
        Assert.False(json.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("EnvironmentFailure", json.RootElement.GetProperty("failureKind").GetString());

        var criticalMissing = json.RootElement.GetProperty("preflight").GetProperty("criticalMissing");
        Assert.True(criticalMissing.GetArrayLength() > 0);
        Assert.Equal(
            "definitely-not-a-real-acpnet-critical-tool",
            criticalMissing[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task DefaultFakeAgentRun_Exits0_WithSingleJsonResult()
    {
        var artifactDir = PrepareArtifactDir("probe-success");
        var run = await RunProbeAsync(["--artifact-dir", artifactDir], TimeSpan.FromSeconds(180));

        Assert.True(run.ExitCode == 0, $"expected exit 0, got {run.ExitCode}; stderr: {run.Stderr}");
        using var json = ParseSingleJson(run.Stdout);
        Assert.Equal("openclaw.acpnet.probe.result", json.RootElement.GetProperty("kind").GetString());
        Assert.True(json.RootElement.GetProperty("ok").GetBoolean());
        Assert.Equal("completed", json.RootElement.GetProperty("result").GetString());
        Assert.Equal("EndTurn", json.RootElement.GetProperty("stopReason").GetString());
        Assert.True(File.Exists(json.RootElement.GetProperty("runArtifactPath").GetString()));
        Assert.True(File.Exists(json.RootElement.GetProperty("transcriptPath").GetString()));
    }

    static JsonDocument ParseSingleJson(string stdout)
    {
        // JsonDocument.Parse rejects trailing content, so this also proves
        // stdout contained exactly one JSON document.
        return JsonDocument.Parse(stdout.Trim());
    }

    static string PrepareArtifactDir(string name)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", name);
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, recursive: true);
        }

        Directory.CreateDirectory(dir);
        return dir;
    }

    async Task<ProbeRunResult> RunProbeAsync(IReadOnlyList<string> args, TimeSpan timeout)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add(fixture.ProbeDllPath);
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new System.Diagnostics.Process { StartInfo = startInfo };
        Assert.True(process.Start(), "probe process did not start");

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"probe did not exit within {timeout.TotalSeconds:0} seconds");
        }

        return new ProbeRunResult(process.ExitCode, await stdoutTask, await stderrTask);
    }

    sealed record ProbeRunResult(int ExitCode, string Stdout, string Stderr);

    public sealed class ProbeBuildFixture
    {
        public ProbeBuildFixture()
        {
            var repoRoot = FindRepoRoot();
            var projectPath = Path.Combine(repoRoot, "src", "samples", "openclaw-acpnet-probe", "openclaw-acpnet-probe.csproj");
            ProbeDllPath = Path.Combine(
                repoRoot, "src", "samples", "openclaw-acpnet-probe", "bin", "Debug", "net8.0", "openclaw-acpnet-probe.dll");

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            startInfo.ArgumentList.Add("build");
            startInfo.ArgumentList.Add(projectPath);
            startInfo.ArgumentList.Add("--nologo");
            startInfo.ArgumentList.Add("--verbosity");
            startInfo.ArgumentList.Add("quiet");

            using var process = System.Diagnostics.Process.Start(startInfo)
                ?? throw new InvalidOperationException("dotnet build did not start");
            var output = new StringBuilder();
            output.Append(process.StandardOutput.ReadToEnd());
            output.Append(process.StandardError.ReadToEnd());
            process.WaitForExit();

            if (process.ExitCode != 0 || !File.Exists(ProbeDllPath))
            {
                throw new InvalidOperationException($"probe build failed (exit {process.ExitCode}): {output}");
            }
        }

        public string ProbeDllPath { get; }

        static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "src", "samples", "openclaw-acpnet-probe", "openclaw-acpnet-probe.csproj")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            throw new InvalidOperationException("Could not locate the repository root from the test base directory.");
        }
    }
}

namespace AcpNet.Process;

public sealed record AcpProcessOptions
{
    public required string Command { get; init; }

    public string? AgentName { get; init; }

    public IReadOnlyList<string> Arguments { get; init; } = [];

    public string? WorkingDirectory { get; init; }

    public AcpRuntime Runtime { get; init; } = AcpRuntime.Auto;

    public string? WslDistribution { get; init; }

    public string? TranscriptPath { get; init; }

    public string? RunArtifactPath { get; init; }

    public AcpShutdownPolicy Shutdown { get; init; } = AcpShutdownPolicy.GracefulThenKill(TimeSpan.FromSeconds(2));

    public IReadOnlyDictionary<string, string?> Environment { get; init; } = new Dictionary<string, string?>();

    public IReadOnlyList<string> AdditionalPathEntries { get; init; } = [];

    public IReadOnlyList<AcpRequiredExecutable> RequiredExecutables { get; init; } = [];
}

namespace AcpNet.Process;

public sealed record AcpRunArtifact(
    string RunId,
    string AgentName,
    string? WorkingDirectory,
    bool UsesWsl,
    string ResolvedCommandLine,
    string Result,
    AcpRunFailureKind FailureKind,
    string? FailureMessage,
    string? TranscriptPath,
    IReadOnlyList<AcpExecutablePreflightResult> Preflight,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt);

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AcpNet.Process;

internal static class AcpRunArtifactWriter
{
    static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    static AcpRunArtifactWriter()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static void Save(
        AcpProcessOptions options,
        AcpProcessStartInfo resolved,
        string runId,
        DateTimeOffset startedAt,
        string result,
        AcpRunFailureKind failureKind,
        string? failureMessage,
        IReadOnlyList<AcpExecutablePreflightResult> preflight)
    {
        if (string.IsNullOrWhiteSpace(options.RunArtifactPath))
        {
            return;
        }

        var artifact = new AcpRunArtifact(
            RunId: runId,
            AgentName: ResolveAgentName(options),
            WorkingDirectory: options.WorkingDirectory,
            UsesWsl: resolved.UsesWsl,
            ResolvedCommandLine: resolved.ResolvedCommandLine,
            Result: result,
            FailureKind: failureKind,
            FailureMessage: failureMessage,
            TranscriptPath: options.TranscriptPath,
            Preflight: preflight,
            StartedAt: startedAt,
            EndedAt: DateTimeOffset.UtcNow);

        var path = Path.GetFullPath(options.RunArtifactPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(artifact, SerializerOptions));
    }

    static string ResolveAgentName(AcpProcessOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.AgentName))
        {
            return options.AgentName;
        }

        var fileName = Path.GetFileNameWithoutExtension(options.Command);
        return string.IsNullOrWhiteSpace(fileName) ? options.Command : fileName;
    }
}

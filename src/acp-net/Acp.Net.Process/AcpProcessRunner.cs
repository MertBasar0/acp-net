using System.Diagnostics;

namespace AcpNet.Process;

public sealed class AcpProcessRunner(AcpProcessOptions options)
{
    public async Task<AcpProcessSession> StartAsync(CancellationToken cancellationToken = default)
    {
        var runId = Guid.NewGuid().ToString("n");
        var startedAt = DateTimeOffset.UtcNow;
        var resolved = AcpRuntimeResolver.Resolve(options);
        var process = new System.Diagnostics.Process { StartInfo = resolved.StartInfo };
        var transcript = new AcpTranscriptRecorder();
        transcript.Event("process.starting", new { runId, resolved.UsesWsl, resolved.ResolvedCommandLine });

        var preflightResults = await AcpPreflightChecker.CheckRequiredExecutablesAsync(options, resolved, cancellationToken);
        foreach (var result in preflightResults)
        {
            transcript.Event(result.Found ? "preflight.tool.found" : "preflight.tool.missing", result);
        }

        if (preflightResults.Any(result => result.IsFailure))
        {
            transcript.Event("preflight.failed", new { failureKind = AcpRunFailureKind.EnvironmentFailure, results = preflightResults.Where(result => result.IsFailure).ToArray() });
            SaveTranscript(transcript);
            AcpRunArtifactWriter.Save(
                options,
                resolved,
                runId,
                startedAt,
                result: "failed",
                failureKind: AcpRunFailureKind.EnvironmentFailure,
                failureMessage: "Required executable preflight failed.",
                preflight: preflightResults);
            throw new AcpPreflightException(preflightResults);
        }

        if (!process.Start())
        {
            SaveTranscript(transcript);
            AcpRunArtifactWriter.Save(
                options,
                resolved,
                runId,
                startedAt,
                result: "failed",
                failureKind: AcpRunFailureKind.ProcessFailure,
                failureMessage: "ACP process could not be started.",
                preflight: preflightResults);
            throw new InvalidOperationException("ACP process could not be started.");
        }

        transcript.Event("process.started", new { process.Id });
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        return new AcpProcessSession(process, options, resolved, transcript, runId, startedAt, preflightResults);
    }

    void SaveTranscript(AcpTranscriptRecorder transcript)
    {
        if (!string.IsNullOrWhiteSpace(options.TranscriptPath))
        {
            transcript.Save(options.TranscriptPath);
        }
    }
}

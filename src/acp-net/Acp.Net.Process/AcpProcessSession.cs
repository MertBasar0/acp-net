namespace AcpNet.Process;

public sealed class AcpProcessSession : IAsyncDisposable
{
    readonly System.Diagnostics.Process process;
    readonly AcpProcessOptions options;
    readonly string runId;
    readonly DateTimeOffset startedAt;
    readonly IReadOnlyList<AcpExecutablePreflightResult> preflightResults;
    readonly Task stderrTask;
    bool stopped;

    internal AcpProcessSession(
        System.Diagnostics.Process process,
        AcpProcessOptions options,
        AcpProcessStartInfo resolved,
        AcpTranscriptRecorder transcript,
        string runId,
        DateTimeOffset startedAt,
        IReadOnlyList<AcpExecutablePreflightResult> preflightResults)
    {
        this.process = process;
        this.options = options;
        this.runId = runId;
        this.startedAt = startedAt;
        this.preflightResults = preflightResults;
        Resolved = resolved;
        Transcript = transcript;
        Stdin = new RecordingTextWriter(process.StandardInput, transcript);
        Stdout = new RecordingTextReader(process.StandardOutput, transcript);
        stderrTask = DrainStderrAsync();
    }

    public TextWriter Stdin { get; }

    public TextReader Stdout { get; }

    public AcpTranscriptRecorder Transcript { get; }

    internal AcpProcessStartInfo Resolved { get; }

    public bool UsesWsl => Resolved.UsesWsl;

    public string ResolvedCommandLine => Resolved.ResolvedCommandLine;

    public string RunId => runId;

    public string ToAgentPath(string path)
    {
        return UsesWsl ? AcpPathMapper.ToWslPath(path) : path;
    }

    public int? ExitCode => process.HasExited ? process.ExitCode : null;

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (stopped)
        {
            return;
        }

        stopped = true;
        if (process.HasExited)
        {
            Transcript.Event("process.exited", new { process.ExitCode });
            SaveTranscript();
            SaveRunArtifact(process.ExitCode == 0 ? "completed" : "failed", process.ExitCode == 0 ? AcpRunFailureKind.None : AcpRunFailureKind.ProcessFailure, process.ExitCode == 0 ? null : $"Process exited with code {process.ExitCode}.");
            return;
        }

        var result = "completed";
        var failureKind = AcpRunFailureKind.None;
        string? failureMessage = null;
        try
        {
            process.StandardInput.Close();
            using var timeout = new CancellationTokenSource(options.Shutdown.GracePeriod);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
            await process.WaitForExitAsync(linked.Token);
            Transcript.Event("process.graceful_exit", new { process.ExitCode });
            if (process.ExitCode != 0)
            {
                result = "failed";
                failureKind = AcpRunFailureKind.ProcessFailure;
                failureMessage = $"Process exited with code {process.ExitCode}.";
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && options.Shutdown.KillProcessTree)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
            Transcript.Event("process.hard_kill", new { process.ExitCode });
            result = "failed";
            failureKind = AcpRunFailureKind.ProcessFailure;
            failureMessage = "Process did not exit before shutdown grace period and was killed.";
        }

        try
        {
            await stderrTask.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            Transcript.Event("stderr.drain_timeout");
        }
        SaveTranscript();
        SaveRunArtifact(result, failureKind, failureMessage);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        Stdin.Dispose();
        process.Dispose();
    }

    async Task DrainStderrAsync()
    {
        while (await process.StandardError.ReadLineAsync() is { } line)
        {
            Transcript.Error(line);
        }
    }

    void SaveTranscript()
    {
        if (!string.IsNullOrWhiteSpace(options.TranscriptPath))
        {
            Transcript.Save(options.TranscriptPath);
        }
    }

    void SaveRunArtifact(string result, AcpRunFailureKind failureKind, string? failureMessage)
    {
        AcpRunArtifactWriter.Save(options, Resolved, runId, startedAt, result, failureKind, failureMessage, preflightResults);
    }
}

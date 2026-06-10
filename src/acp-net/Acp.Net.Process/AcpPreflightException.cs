namespace AcpNet.Process;

public sealed class AcpPreflightException : InvalidOperationException
{
    public AcpPreflightException(IReadOnlyList<AcpExecutablePreflightResult> results)
        : base(BuildMessage(results))
    {
        Results = results;
    }

    public IReadOnlyList<AcpExecutablePreflightResult> Results { get; }

    public AcpRunFailureKind FailureKind => AcpRunFailureKind.EnvironmentFailure;

    static string BuildMessage(IReadOnlyList<AcpExecutablePreflightResult> results)
    {
        var missing = results
            .Where(result => result.IsFailure)
            .Select(result => result.Name);

        return $"ACP process preflight failed. Missing required executable(s): {string.Join(", ", missing)}.";
    }
}

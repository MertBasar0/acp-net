namespace AcpNet.Process;

public sealed record AcpExecutablePreflightResult(
    string Name,
    bool Found,
    string? Path,
    string? Error,
    AcpMissingExecutablePolicy MissingPolicy = AcpMissingExecutablePolicy.Warn)
{
    public bool IsFailure => !Found && MissingPolicy == AcpMissingExecutablePolicy.Throw;
}

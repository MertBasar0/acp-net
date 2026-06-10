namespace AcpNet.Process;

public sealed record AcpShutdownPolicy(TimeSpan GracePeriod, bool KillProcessTree)
{
    public static AcpShutdownPolicy GracefulOnly(TimeSpan gracePeriod) => new(gracePeriod, KillProcessTree: false);

    public static AcpShutdownPolicy GracefulThenKill(TimeSpan gracePeriod) => new(gracePeriod, KillProcessTree: true);
}

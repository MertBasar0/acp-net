namespace AcpNet.Process;

public enum AcpRunFailureKind
{
    None,
    EnvironmentFailure,
    ProcessFailure,
    ProtocolFailure,
    AgentFailure,
    Unknown
}

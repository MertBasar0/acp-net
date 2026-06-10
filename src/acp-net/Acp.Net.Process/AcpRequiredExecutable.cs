namespace AcpNet.Process;

public sealed record AcpRequiredExecutable(string Name, AcpMissingExecutablePolicy MissingPolicy)
{
    public static AcpRequiredExecutable Warn(string name) => new(name, AcpMissingExecutablePolicy.Warn);

    public static AcpRequiredExecutable Throw(string name) => new(name, AcpMissingExecutablePolicy.Throw);

    public static implicit operator AcpRequiredExecutable(string name) => Warn(name);
}

using System.Diagnostics;

namespace AcpNet.Process;

internal sealed record AcpProcessStartInfo(ProcessStartInfo StartInfo, bool UsesWsl, string ResolvedCommandLine);

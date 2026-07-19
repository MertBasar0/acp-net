using System.Diagnostics;
using System.Text;

namespace AcpNet.Process;

internal static class AcpRuntimeResolver
{
    public static AcpProcessStartInfo Resolve(AcpProcessOptions options)
    {
        var runtime = ResolveRuntime(options);
        return runtime == AcpRuntime.Wsl
            ? BuildWslStartInfo(options)
            : BuildNativeStartInfo(options);
    }

    static AcpRuntime ResolveRuntime(AcpProcessOptions options)
    {
        if (options.Runtime != AcpRuntime.Auto)
        {
            return options.Runtime;
        }

        if (!OperatingSystem.IsWindows())
        {
            return AcpRuntime.Native;
        }

        if (!string.IsNullOrWhiteSpace(options.WslDistribution))
        {
            return AcpRuntime.Wsl;
        }

        if (options.Arguments.Any(IsLikelyWslPath) || IsLikelyWslPath(options.WorkingDirectory))
        {
            return AcpRuntime.Wsl;
        }

        // A bare command like `python3` resolves on Windows to the Microsoft Store
        // execution-alias stub under WindowsApps, which hangs when launched from a
        // non-interactive process. When that is the only native match, the command
        // is meant for a POSIX runtime, so route it through WSL instead of launching
        // a stub that would silently block.
        if (AcpWindowsExecutableProbe.ResolvesOnlyToExecutionAliasStub(options.Command))
        {
            return AcpRuntime.Wsl;
        }

        return AcpRuntime.Native;
    }

    static AcpProcessStartInfo BuildNativeStartInfo(AcpProcessOptions options)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = options.Command,
            Arguments = JoinArguments(options.Arguments),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        if (!string.IsNullOrWhiteSpace(options.WorkingDirectory))
        {
            startInfo.WorkingDirectory = options.WorkingDirectory;
        }

        ApplyEnvironment(startInfo, options, mapPath: false);
        return new AcpProcessStartInfo(startInfo, UsesWsl: false, $"{options.Command} {startInfo.Arguments}".Trim());
    }

    static AcpProcessStartInfo BuildWslStartInfo(AcpProcessOptions options)
    {
        var wslFlags = new List<string>();
        if (!string.IsNullOrWhiteSpace(options.WslDistribution))
        {
            wslFlags.Add("-d");
            wslFlags.Add(options.WslDistribution);
        }

        if (!string.IsNullOrWhiteSpace(options.WorkingDirectory))
        {
            wslFlags.Add("--cd");
            wslFlags.Add(AcpPathMapper.ToWslPath(options.WorkingDirectory));
        }

        // wsl.exe parses its own flags from Windows argv, but everything after `--`
        // is handed verbatim to the default POSIX shell (`bash -c`). The two halves
        // therefore need different quoting: Windows rules before `--`, POSIX rules
        // after. Windows-style double quotes after `--` reach the shell as literal
        // characters and corrupt the command instead of protecting it.
        var shellWords = new List<string> { options.Command };
        shellWords.AddRange(options.Arguments.Select(AcpPathMapper.ToWslPath));

        var flagLine = JoinArguments(wslFlags);
        var shellLine = string.Join(" ", shellWords.Select(PosixQuote));
        var argumentLine = flagLine.Length == 0 ? $"-- {shellLine}" : $"{flagLine} -- {shellLine}";

        var startInfo = new ProcessStartInfo
        {
            FileName = "wsl.exe",
            Arguments = argumentLine,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        ApplyEnvironment(startInfo, options, mapPath: true);
        return new AcpProcessStartInfo(startInfo, UsesWsl: true, $"wsl.exe {startInfo.Arguments}");
    }

    static bool IsLikelyWslPath(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.StartsWith("/", StringComparison.Ordinal)
                || value.StartsWith(@"\\wsl.localhost\", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith(@"\\wsl$\", StringComparison.OrdinalIgnoreCase));
    }

    internal static string JoinArguments(IEnumerable<string> arguments)
    {
        return string.Join(" ", arguments.Select(Quote));
    }

    internal static string PosixQuote(string value)
    {
        if (value.Length == 0)
        {
            return "''";
        }

        if (value.All(IsPosixSafe))
        {
            return value;
        }

        return "'" + value.Replace("'", @"'\''") + "'";
    }

    static bool IsPosixSafe(char ch)
    {
        return ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9')
            or '-' or '_' or '.' or '/' or ':' or '=' or '+' or ',' or '@' or '%';
    }

    static string Quote(string value)
    {
        if (value.Length == 0)
        {
            return "\"\"";
        }

        if (!value.Any(char.IsWhiteSpace) && !value.Contains('"'))
        {
            return value;
        }

        var builder = new StringBuilder("\"");
        foreach (var ch in value)
        {
            builder.Append(ch == '"' ? "\\\"" : ch);
        }

        builder.Append('"');
        return builder.ToString();
    }

    static void ApplyEnvironment(ProcessStartInfo startInfo, AcpProcessOptions options, bool mapPath)
    {
        foreach (var (key, value) in options.Environment)
        {
            if (value is null)
            {
                startInfo.Environment.Remove(key);
            }
            else
            {
                startInfo.Environment[key] = value;
            }
        }

        if (options.AdditionalPathEntries.Count == 0)
        {
            return;
        }

        var currentPath = startInfo.Environment.TryGetValue("PATH", out var existing)
            ? existing
            : Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var pathEntries = options.AdditionalPathEntries
            .Select(entry => mapPath ? AcpPathMapper.ToWslPath(entry) : entry)
            .Where(entry => !string.IsNullOrWhiteSpace(entry));
        startInfo.Environment["PATH"] = string.Join(":", pathEntries.Concat([currentPath]));
    }
}

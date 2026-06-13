using System.Diagnostics;

namespace AcpNet.Process;

/// <summary>
/// Detects Windows "app execution alias" stubs. These zero-length reparse points
/// live under <c>...\Microsoft\WindowsApps\</c> and stand in for Store apps such as
/// <c>python3.exe</c>. Launching one from a non-interactive process blocks (it tries
/// to open the Microsoft Store), so a stub must never be treated as a real, runnable
/// executable: <c>where.exe</c> reports it as a match, which would otherwise turn a
/// missing tool into a silent hang.
/// </summary>
internal static class AcpWindowsExecutableProbe
{
    /// <summary>
    /// Pure predicate: true when <paramref name="path"/> points at a Windows Store
    /// execution-alias stub, identified by a zero-length file under a
    /// <c>WindowsApps</c> directory. Kept side-effect free so it can be unit tested
    /// without touching the filesystem.
    /// </summary>
    public static bool IsExecutionAliasStubPath(string? path, long fileLength)
    {
        if (string.IsNullOrWhiteSpace(path) || fileLength != 0)
        {
            return false;
        }

        var normalized = path.Replace('/', '\\');
        return normalized.Contains(@"\WindowsApps\", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// True when <paramref name="path"/> exists on disk and is a Windows Store
    /// execution-alias stub. Always false off Windows.
    /// </summary>
    public static bool IsExecutionAliasStubFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            var info = new FileInfo(path);
            return info.Exists && IsExecutionAliasStubPath(path, info.Length);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves <paramref name="command"/> through <c>where.exe</c> and returns the
    /// first match that is a real executable (not a Store execution-alias stub), or
    /// <c>null</c> when the command is missing or resolves only to stubs. Always
    /// <c>null</c> off Windows.
    /// </summary>
    public static string? ResolveRealNativeExecutable(string command)
    {
        if (string.IsNullOrWhiteSpace(command) || !OperatingSystem.IsWindows())
        {
            return null;
        }

        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };

            if (!process.Start())
            {
                return null;
            }

            var stdout = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            if (!process.WaitForExit(5000))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Best effort.
                }

                return null;
            }

            foreach (var line in stdout.Split('\n'))
            {
                var candidate = line.Trim();
                if (candidate.Length > 0 && !IsExecutionAliasStubFile(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// True when <paramref name="command"/> resolves natively only to a Windows Store
    /// execution-alias stub (i.e. there is no real native executable behind it). Used
    /// to steer runtime resolution toward WSL instead of launching a stub that hangs.
    /// </summary>
    public static bool ResolvesOnlyToExecutionAliasStub(string command)
    {
        if (string.IsNullOrWhiteSpace(command) || !OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };

            if (!process.Start())
            {
                return false;
            }

            var stdout = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            if (!process.WaitForExit(5000))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Best effort.
                }

                return false;
            }

            var candidates = stdout
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToArray();

            return candidates.Length > 0 && candidates.All(IsExecutionAliasStubFile);
        }
        catch
        {
            return false;
        }
    }
}

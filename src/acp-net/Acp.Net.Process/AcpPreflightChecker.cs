using System.Diagnostics;

namespace AcpNet.Process;

internal static class AcpPreflightChecker
{
    public static async Task<IReadOnlyList<AcpExecutablePreflightResult>> CheckRequiredExecutablesAsync(
        AcpProcessOptions options,
        AcpProcessStartInfo agentStartInfo,
        CancellationToken cancellationToken)
    {
        var requirements = GetRequirements(options);
        if (requirements.Count == 0)
        {
            return [];
        }

        var results = new List<AcpExecutablePreflightResult>();
        foreach (var requirement in requirements)
        {
            results.Add(await CheckOneAsync(requirement, agentStartInfo, cancellationToken));
        }

        return results;
    }

    static IReadOnlyList<AcpRequiredExecutable> GetRequirements(AcpProcessOptions options)
    {
        return options.RequiredExecutables
            .Select(AcpRequiredExecutable.Warn)
            .Concat(options.RequiredTools)
            .Where(requirement => !string.IsNullOrWhiteSpace(requirement.Name))
            .ToArray();
    }

    static async Task<AcpExecutablePreflightResult> CheckOneAsync(
        AcpRequiredExecutable requirement,
        AcpProcessStartInfo agentStartInfo,
        CancellationToken cancellationToken)
    {
        var executable = requirement.Name;
        var startInfo = agentStartInfo.UsesWsl
            ? BuildWslWhichStartInfo(executable, agentStartInfo)
            : BuildNativeWhichStartInfo(executable, agentStartInfo);

        try
        {
            using var process = new System.Diagnostics.Process { StartInfo = startInfo };
            if (!process.Start())
            {
                return new AcpExecutablePreflightResult(executable, Found: false, Path: null, Error: "preflight process did not start", requirement.MissingPolicy);
            }

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var stdout = (await stdoutTask).Trim();
            var stderr = (await stderrTask).Trim();

            return process.ExitCode == 0 && stdout.Length > 0
                ? new AcpExecutablePreflightResult(executable, Found: true, Path: stdout.Split('\n')[0].Trim(), Error: null, requirement.MissingPolicy)
                : new AcpExecutablePreflightResult(executable, Found: false, Path: null, Error: stderr.Length > 0 ? stderr : $"which exited {process.ExitCode}", requirement.MissingPolicy);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new AcpExecutablePreflightResult(executable, Found: false, Path: null, Error: ex.Message, requirement.MissingPolicy);
        }
    }

    static ProcessStartInfo BuildNativeWhichStartInfo(string executable, AcpProcessStartInfo agentStartInfo)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "where.exe" : "which",
            Arguments = executable,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        CopyEnvironment(agentStartInfo.StartInfo, startInfo);
        return startInfo;
    }

    static ProcessStartInfo BuildWslWhichStartInfo(string executable, AcpProcessStartInfo agentStartInfo)
    {
        var args = new List<string>();
        var originalArgs = SplitArguments(agentStartInfo.StartInfo.Arguments);

        for (var i = 0; i < originalArgs.Count; i++)
        {
            if (originalArgs[i] == "--")
            {
                break;
            }

            if (originalArgs[i] == "--cd" && i + 1 < originalArgs.Count)
            {
                args.Add("--cd");
                args.Add(originalArgs[++i]);
                continue;
            }

            if (originalArgs[i] == "-d" && i + 1 < originalArgs.Count)
            {
                args.Add("-d");
                args.Add(originalArgs[++i]);
            }
        }

        args.Add("--");
        args.Add("which");
        args.Add(executable);

        var startInfo = new ProcessStartInfo
        {
            FileName = "wsl.exe",
            Arguments = AcpRuntimeResolver.JoinArguments(args),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        CopyEnvironment(agentStartInfo.StartInfo, startInfo);
        return startInfo;
    }

    static void CopyEnvironment(ProcessStartInfo source, ProcessStartInfo target)
    {
        foreach (var item in source.Environment)
        {
            target.Environment[item.Key] = item.Value;
        }
    }

    static List<string> SplitArguments(string arguments)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < arguments.Length; i++)
        {
            var ch = arguments[i];
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }

            current.Append(ch);
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }
}

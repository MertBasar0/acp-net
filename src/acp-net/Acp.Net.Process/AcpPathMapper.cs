namespace AcpNet.Process;

internal static class AcpPathMapper
{
    public static string ToWslPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        if (path.StartsWith(@"\\wsl.localhost\", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            var relative = string.Join("/", parts.Skip(2));
            return "/" + relative;
        }

        if (path.StartsWith(@"\\wsl$\", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            var relative = string.Join("/", parts.Skip(2));
            return "/" + relative;
        }

        if (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && (path[2] == '\\' || path[2] == '/'))
        {
            var drive = char.ToLowerInvariant(path[0]);
            var rest = path[3..].Replace('\\', '/');
            return $"/mnt/{drive}/{rest}";
        }

        return path.Replace('\\', '/');
    }
}

namespace AcpNet.Testing;

public static class AcpTranscriptAssert
{
    public static void Contains(string transcriptPath, string expected)
    {
        var content = File.ReadAllText(transcriptPath);
        if (!content.Contains(expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Transcript does not contain expected text: {expected}");
        }
    }

    public static void ExistsAndNotEmpty(string transcriptPath)
    {
        var info = new FileInfo(transcriptPath);
        if (!info.Exists || info.Length == 0)
        {
            throw new InvalidOperationException($"Transcript was not created or is empty: {transcriptPath}");
        }
    }
}

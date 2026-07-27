namespace InDows.Core;

/// <summary>Small display formatters shared across the app.</summary>
public static class Format
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB", "TB", "PB"];

    /// <summary>A byte count as a rounded gigabyte string, for example "465 GB".</summary>
    public static string Gigabytes(long bytes) => $"{Math.Round(bytes / 1_000_000_000d)} GB";

    /// <summary>A byte count in the largest fitting unit, for example "12.3 MB" or "512 B".</summary>
    public static string Bytes(long bytes)
    {
        double value = bytes;
        var unit = 0;
        while (value >= 1024 && unit < Units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} B" : $"{value:0.#} {Units[unit]}";
    }
}

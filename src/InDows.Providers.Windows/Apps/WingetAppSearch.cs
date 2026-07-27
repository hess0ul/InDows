using System.Diagnostics;
using System.Text;
using InDows.Core.Build;

namespace InDows.Providers.Windows.Apps;

/// <summary>
/// Runs <c>winget search &lt;query&gt;</c> and parses its table output into results. The parse is deliberately
/// locale-independent: winget translates the header words ("Name/Id" in English, "Nom/ID" in French, …), so we
/// key off the table's <em>structure</em> instead — the dashed separator line locates the table, and the header
/// line above it gives the column start offsets. The id is always the second column. Parsing is a static method
/// so it can be unit-tested against captured output, in any language, without running winget.
/// </summary>
public sealed class WingetAppSearch : IAppSearch
{
    public IReadOnlyList<AppSearchResult> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var psi = new ProcessStartInfo
        {
            FileName = "winget",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
        };
        psi.ArgumentList.Add("search");
        psi.ArgumentList.Add(query);
        psi.ArgumentList.Add("--source");
        psi.ArgumentList.Add("winget");
        psi.ArgumentList.Add("--accept-source-agreements");
        psi.ArgumentList.Add("--disable-interactivity");

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Could not start winget (is it installed?).");
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit(20_000);
        return Parse(output);
    }

    /// <summary>
    /// Parse winget's search table, whatever the display language. Name = the first column, Id = the second
    /// (a single token). Column offsets come from the header line found just above the dashed separator.
    /// </summary>
    public static IReadOnlyList<AppSearchResult> Parse(string output)
    {
        var lines = output.Replace("\r", "", StringComparison.Ordinal).Split('\n');

        // The separator (a run of dashes/box chars) sits between the header and the data rows; it anchors the table.
        var separatorIndex = Array.FindIndex(lines, IsSeparator);
        if (separatorIndex <= 0)
        {
            return [];
        }

        // Column start offsets from the header: each column begins where a non-space follows a space (or line start).
        var columnStarts = ColumnStarts(lines[separatorIndex - 1]);
        if (columnStarts.Count < 2)
        {
            return [];   // need at least a Name and an Id column
        }

        var nameCol = columnStarts[0];
        var idCol = columnStarts[1];
        var idEnd = columnStarts.Count > 2 ? columnStarts[2] : int.MaxValue;

        var results = new List<AppSearchResult>();
        for (var i = separatorIndex + 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.Length <= idCol || IsSeparator(line) || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var name = line[nameCol..Math.Min(idCol, line.Length)].Trim();
            var idField = line[idCol..Math.Min(idEnd, line.Length)].Trim();
            var id = idField.Split(' ', '\t')[0];   // the id is a single token; drop any trailing column that ran in
            if (name.Length > 0 && id.Length > 0)
            {
                results.Add(new AppSearchResult(name, id));
            }
        }

        return results;
    }

    /// <summary>A table separator: a non-empty line made only of dashes or box-drawing rule characters.</summary>
    private static bool IsSeparator(string line)
    {
        var trimmed = line.Trim();
        return trimmed.Length >= 3 && trimmed.All(c => c is '-' or '─' or '—' or '=');
    }

    /// <summary>Offsets where each column begins on the header line (a non-space that follows a space or the start).</summary>
    private static List<int> ColumnStarts(string header)
    {
        var starts = new List<int>();
        for (var i = 0; i < header.Length; i++)
        {
            if (header[i] != ' ' && (i == 0 || header[i - 1] == ' '))
            {
                starts.Add(i);
            }
        }

        return starts;
    }
}

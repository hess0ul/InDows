using System.Text;

namespace InDows.Core.Build;

/// <summary>One snippet module to graft: the base anchor it plugs into, and the XML to insert there.</summary>
public sealed record SnippetGraft(string Anchor, string SnippetXml);

/// <summary>One resolved module to compose in: its name, kind, base anchor, and the content to graft
/// (snippet XML for a snippet module, or the assembled script for a script module).</summary>
public sealed record ModuleGraft(string Name, ModuleKind Kind, string Anchor, string Content);

/// <summary>
/// Composes an InDows autounattend.xml from an already-anchored base and a set of selected modules. Snippet
/// modules are inserted at their anchor comment; script modules become a <c>&lt;File&gt;</c> (the .ps1) plus a
/// run command at their anchor, with an <c>&lt;Order&gt;</c> in the free range of that anchor's pass. Pure string
/// surgery so the rest of the base is preserved byte-for-byte, and unit-tested.
/// </summary>
public static class AutounattendGenerator
{
    /// <summary>Inserts each snippet's XML right after its module anchor comment, keeping the anchor's indent.
    /// Grafts sharing an anchor are grouped and emitted in selection order (a single insert per anchor), so the
    /// document order matches the order they were passed in.</summary>
    public static string GraftSnippets(string anchoredBaseXml, IReadOnlyList<SnippetGraft> grafts)
    {
        var xml = anchoredBaseXml;
        var eol = xml.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

        foreach (var group in grafts.GroupBy(g => g.Anchor))
        {
            var anchor = $"<!-- [InDows:module] {group.Key} -->";
            var index = xml.IndexOf(anchor, StringComparison.Ordinal);
            if (index < 0)
            {
                throw new InvalidOperationException(
                    $"The base has no anchor for '{group.Key}'. Is it an InDows base with the module anchors?");
            }

            var lineStart = xml.LastIndexOf('\n', index) + 1;
            var indent = xml[lineStart..index];
            var block = new StringBuilder();
            foreach (var graft in group)
            {
                foreach (var line in graft.SnippetXml.Trim().Split('\n'))
                {
                    block.Append(eol).Append(indent).Append(line.TrimEnd('\r'));
                }
            }

            var after = index + anchor.Length;
            xml = xml[..after] + block + xml[after..];
        }

        return xml;
    }

    /// <summary>
    /// Rebuilds a decomposed module's script so each setting's line matches its checkbox: a <b>selected</b>
    /// fragment's line is forced active (uncommented — this lets a module ship a "danger zone" commented out and
    /// still be togglable), and a <b>deselected</b> fragment's line is forced commented. Lines are matched on the
    /// code part (ignoring a leading <c>#</c> and any trailing comment), so the module's scaffold (helpers,
    /// variables, prose) is preserved untouched and no dependency can go missing.
    /// </summary>
    public static string AssembleScript(string fullScript, IReadOnlyList<string> selectedFragments, IReadOnlyList<string> deselectedFragments)
    {
        var selected = FragmentCodes(selectedFragments);
        var deselected = FragmentCodes(deselectedFragments);
        if (selected.Count == 0 && deselected.Count == 0)
        {
            return fullScript;
        }

        var eol = fullScript.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = fullScript.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmedStart = line.TrimStart();
            var indent = line[..(line.Length - trimmedStart.Length)];
            var commented = trimmedStart.StartsWith('#');
            var bare = commented ? trimmedStart.TrimStart('#').TrimStart() : trimmedStart;
            var code = Code(bare);

            if (code.Length > 0 && selected.Contains(code))
            {
                sb.Append(indent).Append(bare);              // force active (uncomment if it was commented)
            }
            else if (code.Length > 0 && deselected.Contains(code))
            {
                sb.Append(indent).Append("# ").Append(bare); // force commented
            }
            else
            {
                sb.Append(line);                             // scaffold / prose: leave untouched
            }

            if (i < lines.Length - 1)
            {
                sb.Append(eol);
            }
        }

        return sb.ToString();
    }

    private static HashSet<string> FragmentCodes(IEnumerable<string> fragments) =>
        new(fragments.SelectMany(f => f.Replace("\r\n", "\n").Split('\n')).Select(Code).Where(c => c.Length > 0),
            StringComparer.Ordinal);

    /// <summary>Appends a ReDows profile's winget apps (M2/M3) to the base's <c>configuration.dsc.yaml</c>
    /// block, so the first-logon bootstrap installs them alongside the base essentials.</summary>
    public static string AppendProfileApps(string xml, IReadOnlyList<string> wingetIds)
    {
        if (wingetIds.Count == 0)
        {
            return xml;
        }

        const string marker = "configuration.dsc.yaml\"><![CDATA[";
        var start = xml.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException("The base has no configuration.dsc.yaml block to add profile apps to.");
        }

        var end = xml.IndexOf("]]></File>", start, StringComparison.Ordinal);
        if (end < 0)
        {
            throw new InvalidOperationException("The configuration.dsc.yaml block is not closed.");
        }

        var eol = xml.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sb = new StringBuilder();
        foreach (var id in wingetIds)
        {
            sb.Append("    - resource: Microsoft.WinGet.DSC/WinGetPackage").Append(eol);
            sb.Append("      id: ").Append(Slug(id, used)).Append(eol);
            sb.Append("      settings: { id: ").Append(id).Append(", source: winget }").Append(eol);
        }

        // Insert whole lines before the closing "]]></File>" line, so the entries keep their own indent
        // regardless of how that line is indented.
        var lineStart = xml.LastIndexOf('\n', end) + 1;
        return xml[..lineStart] + sb + xml[lineStart..];
    }

    private static string Slug(string wingetId, HashSet<string> used)
    {
        var s = new string(wingetId.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        if (s.Length == 0)
        {
            s = "app";
        }

        var slug = s;
        var i = 2;
        while (used.Contains(slug))
        {
            slug = s + i;
            i++;
        }

        used.Add(slug);
        return slug;
    }

    /// <summary>Replaces each <c>__KEY__</c> placeholder in the content with the user's value for that key.</summary>
    public static string FillParams(string content, IReadOnlyDictionary<string, string> values)
    {
        foreach (var (key, value) in values)
        {
            content = content.Replace($"__{key}__", value, StringComparison.Ordinal);
        }

        return content;
    }

    /// <summary>
    /// Replaces the right-hand side of an assignment line marked <c>$var = &lt;default&gt;   # [InDows:param KEY]</c>
    /// with the user's value for KEY, keeping the marker. Lets a script module carry a valid default (so it still
    /// runs standalone) while the GUI overrides it — used for presets like the DNS resolver or services preset.
    /// </summary>
    public static string FillAssignments(string content, IReadOnlyDictionary<string, string> values)
    {
        if (values.Count == 0)
        {
            return content;
        }

        const string marker = "# [InDows:param ";
        var eol = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = content.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var markerAt = line.IndexOf(marker, StringComparison.Ordinal);
            if (markerAt < 0)
            {
                continue;
            }

            var keyEnd = line.IndexOf(']', markerAt + marker.Length);
            var equals = line.IndexOf('=', StringComparison.Ordinal);
            if (keyEnd < 0 || equals < 0 || equals > markerAt)
            {
                continue;
            }

            var key = line[(markerAt + marker.Length)..keyEnd].Trim();
            if (values.TryGetValue(key, out var value))
            {
                lines[i] = $"{line[..(equals + 1)]} {value}   {line[markerAt..]}";
            }
        }

        return string.Join(eol, lines);
    }

    /// <summary>Composes the final autounattend.xml: grafts snippets at their anchors, and for each script
    /// module inserts a run command at its anchor plus a <c>&lt;File&gt;</c> block before <c>&lt;/Extensions&gt;</c>.</summary>
    public static string Compose(string baseXml, IReadOnlyList<ModuleGraft> grafts)
    {
        var snippetGrafts = new List<SnippetGraft>();
        var files = new List<(string Name, string Content)>();
        var nextOrder = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var graft in grafts)
        {
            if (graft.Kind == ModuleKind.Snippet)
            {
                snippetGrafts.Add(new SnippetGraft(graft.Anchor, graft.Content));
                continue;
            }

            var (firstLogon, baseOrder) = Plumbing(graft.Anchor);
            var order = nextOrder.TryGetValue(graft.Anchor, out var next) ? next : baseOrder;
            nextOrder[graft.Anchor] = order + 1;

            var path = $@"C:\Windows\Setup\Scripts\{graft.Name}.ps1";
            snippetGrafts.Add(new SnippetGraft(graft.Anchor, CommandXml(firstLogon, order, path)));
            files.Add((graft.Name, graft.Content));
        }

        var xml = GraftSnippets(baseXml, snippetGrafts);
        return GraftFiles(xml, files);
    }

    /// <summary>The code part of a line: without a trailing "# comment", with interior whitespace collapsed to a
    /// single space; empty for a comment line. Collapsing makes tweak matching survive spacing differences
    /// between a tweak fragment and its script line.</summary>
    private static string Code(string line)
    {
        var t = line.Trim();
        if (t.StartsWith('#'))
        {
            return "";
        }

        var hash = t.IndexOf(" #", StringComparison.Ordinal);
        if (hash >= 0)
        {
            t = t[..hash];
        }

        return string.Join(' ', t.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static (bool FirstLogon, int BaseOrder) Plumbing(string anchor) => anchor switch
    {
        // Inside the default-user hive window (loaded Order 3, unloaded Order 50): use 6..49.
        "default-user-scripts" => (false, 6),
        // After the hive unload, still in specialize's RunSynchronous: use 51+.
        "specialize-scripts" => (false, 51),
        // FirstLogonCommands (the base uses Orders 1 and 2): use 3+.
        "first-logon-scripts" => (true, 3),
        _ => throw new InvalidOperationException($"Anchor '{anchor}' does not take a script module."),
    };

    private static string CommandXml(bool firstLogon, int order, string path)
    {
        const string run = "powershell.exe -WindowStyle \"Hidden\" -ExecutionPolicy \"Unrestricted\" -NoProfile -File";
        return firstLogon
            ? "<SynchronousCommand wcm:action=\"add\">\n" +
              $"    <Order>{order}</Order>\n" +
              $"    <CommandLine>{run} \"{path}\"</CommandLine>\n" +
              "</SynchronousCommand>"
            : "<RunSynchronousCommand wcm:action=\"add\">\n" +
              $"    <Order>{order}</Order>\n" +
              $"    <Path>{run} \"{path}\"</Path>\n" +
              "</RunSynchronousCommand>";
    }

    private static string GraftFiles(string xml, IReadOnlyList<(string Name, string Content)> files)
    {
        if (files.Count == 0)
        {
            return xml;
        }

        const string close = "</Extensions>";
        var index = xml.IndexOf(close, StringComparison.Ordinal);
        if (index < 0)
        {
            throw new InvalidOperationException("The base has no <Extensions> section to hold the script <File> blocks.");
        }

        var eol = xml.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lineStart = xml.LastIndexOf('\n', index) + 1;
        var indent = xml[lineStart..index];

        var sb = new StringBuilder();
        foreach (var (name, content) in files)
        {
            var body = string.Join(eol, content.Replace("\r\n", "\n").Split('\n'));
            sb.Append(indent).Append($"<File path=\"C:\\Windows\\Setup\\Scripts\\{name}.ps1\"><![CDATA[").Append(eol);
            sb.Append(body).Append(eol);
            sb.Append("]]></File>").Append(eol);
        }

        return xml[..lineStart] + sb + xml[lineStart..];
    }
}

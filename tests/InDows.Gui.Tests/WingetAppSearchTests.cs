using InDows.Providers.Windows.Apps;
using Xunit;

namespace InDows.Gui.Tests;

public class WingetAppSearchTests
{
    // winget aligns data rows to the header's column offsets; reproduce that with fixed-width padding.
    private static string Row(string name, string id, string version, string source) =>
        name.PadRight(21) + id.PadRight(22) + version.PadRight(10) + source;

    [Fact]
    public void Parse_reads_the_name_and_id_from_each_row()
    {
        var output = string.Join('\n',
            Row("Name", "Id", "Version", "Source"),
            new string('-', 60),
            Row("Mozilla Firefox", "Mozilla.Firefox", "128.0", "winget"),
            Row("Git", "Git.Git", "2.45.0", "winget"));

        var results = WingetAppSearch.Parse(output);

        Assert.Equal(2, results.Count);
        Assert.Equal("Mozilla Firefox", results[0].Name);
        Assert.Equal("Mozilla.Firefox", results[0].Id);
        Assert.Equal("Git", results[1].Name);
        Assert.Equal("Git.Git", results[1].Id);
    }

    // A French winget: translated header ("Nom/ID/Version/Correspondance") and no Source column. The parse must
    // still work — it keys off the table structure, never the header words.
    private static string FrRow(string nom, string id, string version, string correspondance) =>
        nom.PadRight(21) + id.PadRight(26) + version.PadRight(13) + correspondance;

    [Fact]
    public void Parse_is_locale_independent_and_reads_a_french_table()
    {
        var output = string.Join('\n',
            FrRow("Nom", "ID", "Version", "Correspondance"),
            new string('-', 74),
            FrRow("Brave", "Brave.Brave", "150.1.92.141", ""),
            FrRow("Browser Tamer", "aloneguid.bt", "5.6.12", "Tag: brave"),
            FrRow("Brave Beta", "Brave.Brave.Beta", "151.1.93.124", ""));

        var results = WingetAppSearch.Parse(output);

        Assert.Equal(3, results.Count);
        Assert.Equal("Brave", results[0].Name);
        Assert.Equal("Brave.Brave", results[0].Id);
        Assert.Equal("Browser Tamer", results[1].Name);
        Assert.Equal("aloneguid.bt", results[1].Id);   // the trailing "Tag: brave" column is not mistaken for the id
    }

    [Fact]
    public void Parse_returns_nothing_when_there_is_no_table()
    {
        Assert.Empty(WingetAppSearch.Parse("No package found matching input criteria."));
    }

    [Fact]
    public void Parse_tolerates_crlf_line_endings()
    {
        var output = string.Join("\r\n",
            Row("Name", "Id", "Version", "Source"),
            new string('-', 60),
            Row("7-Zip", "7zip.7zip", "24.08", "winget"));

        var result = Assert.Single(WingetAppSearch.Parse(output));
        Assert.Equal("7zip.7zip", result.Id);
    }
}

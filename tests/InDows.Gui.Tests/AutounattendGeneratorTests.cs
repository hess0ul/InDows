using System.Xml;
using InDows.Core.Build;
using Xunit;

namespace InDows.Gui.Tests;

public class AutounattendGeneratorTests
{
    private const string AnchoredBase =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<unattend xmlns=\"urn:schemas-microsoft-com:unattend\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\">\n" +
        "\t<settings pass=\"specialize\">\n" +
        "\t\t<!-- [InDows:module] specialize-shell-setup -->\n" +
        "\t\t<component name=\"Microsoft-Windows-Deployment\" processorArchitecture=\"amd64\" publicKeyToken=\"x\" language=\"neutral\" versionScope=\"nonSxS\" />\n" +
        "\t</settings>\n" +
        "\t<settings pass=\"oobeSystem\">\n" +
        "\t\t<component name=\"Microsoft-Windows-Shell-Setup\" processorArchitecture=\"amd64\" publicKeyToken=\"x\" language=\"neutral\" versionScope=\"nonSxS\">\n" +
        "\t\t\t<OOBE>\n" +
        "\t\t\t\t<HideEULAPage>true</HideEULAPage>\n" +
        "\t\t\t</OOBE>\n" +
        "\t\t\t<!-- [InDows:module] account -->\n" +
        "\t\t</component>\n" +
        "\t</settings>\n" +
        "</unattend>\n";

    private const string AccountSnippet =
        "<UserAccounts>\n\t<LocalAccounts>\n\t\t<LocalAccount wcm:action=\"add\"><Name>Tom</Name><Group>Administrators</Group></LocalAccount>\n\t</LocalAccounts>\n</UserAccounts>";

    private const string ComputerNameSnippet =
        "<component name=\"Microsoft-Windows-Shell-Setup\" processorArchitecture=\"amd64\" publicKeyToken=\"x\" language=\"neutral\" versionScope=\"nonSxS\">\n\t<ComputerName>PC</ComputerName>\n</component>";

    private const string ComposeBase =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<unattend xmlns=\"urn:schemas-microsoft-com:unattend\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\">\n" +
        "\t<settings pass=\"specialize\">\n" +
        "\t\t<RunSynchronous>\n" +
        "\t\t\t<!-- [InDows:module] specialize-scripts -->\n" +
        "\t\t</RunSynchronous>\n" +
        "\t</settings>\n" +
        "\t<Extensions xmlns=\"\">\n" +
        "\t</Extensions>\n" +
        "</unattend>\n";

    private static XmlDocument Load(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        return doc;
    }

    [Fact]
    public void Grafts_a_snippet_after_its_anchor_and_stays_valid_xml()
    {
        var result = AutounattendGenerator.GraftSnippets(AnchoredBase, new[] { new SnippetGraft("account", AccountSnippet) });

        Load(result); // throws if not well-formed
        Assert.Contains("<UserAccounts>", result);
        Assert.True(result.IndexOf("<UserAccounts>", StringComparison.Ordinal) > result.IndexOf("[InDows:module] account", StringComparison.Ordinal));
    }

    [Fact]
    public void Grafts_several_snippets_each_at_its_own_anchor()
    {
        var result = AutounattendGenerator.GraftSnippets(AnchoredBase, new[]
        {
            new SnippetGraft("account", AccountSnippet),
            new SnippetGraft("specialize-shell-setup", ComputerNameSnippet),
        });

        Load(result);
        Assert.Contains("<UserAccounts>", result);
        Assert.Contains("<ComputerName>PC</ComputerName>", result);
    }

    [Fact]
    public void A_missing_anchor_is_rejected()
    {
        Assert.Throws<InvalidOperationException>(() =>
            AutounattendGenerator.GraftSnippets(AnchoredBase, new[] { new SnippetGraft("does-not-exist", AccountSnippet) }));
    }

    [Fact]
    public void No_grafts_returns_the_base_unchanged()
    {
        var result = AutounattendGenerator.GraftSnippets(AnchoredBase, []);

        Assert.Equal(AnchoredBase, result);
    }

    [Fact]
    public void AssembleScript_comments_out_only_the_deselected_settings()
    {
        var script =
            "Log start\n" +
            "RegDword $adv 'A' 0   # keep me\n" +
            "RegDword $adv 'B' 1   # drop me\n" +
            "Log done";

        var result = AutounattendGenerator.AssembleScript(script, ["RegDword $adv 'A' 0"], ["RegDword $adv 'B' 1"]);

        Assert.Contains("RegDword $adv 'A' 0   # keep me", result);       // selected line kept active
        Assert.Contains("# RegDword $adv 'B' 1   # drop me", result);     // deselected line commented out
        Assert.DoesNotContain("\nRegDword $adv 'B' 1", result);           // no active B line remains
    }

    [Fact]
    public void AssembleScript_uncomments_a_selected_line_that_was_commented_in_the_source()
    {
        // A module that ships an option commented out (a "danger zone"): selecting it must activate it.
        var script = "Log start\n# RegDword $adv 'Danger' 1   # opt-in\nLog done";

        var result = AutounattendGenerator.AssembleScript(script, ["RegDword $adv 'Danger' 1"], []);

        Assert.Contains("\nRegDword $adv 'Danger' 1   # opt-in", result);   // now active
        Assert.DoesNotContain("# RegDword $adv 'Danger' 1", result);        // no longer commented
    }

    [Fact]
    public void AssembleScript_with_nothing_selected_or_deselected_is_unchanged()
    {
        var script = "RegDword A\nRegDword B";

        Assert.Equal(script, AutounattendGenerator.AssembleScript(script, [], []));
    }

    [Fact]
    public void AssembleScript_matches_despite_interior_whitespace_differences()
    {
        // The script line has several spaces before the value/comment; the fragment uses single spaces.
        var script = "RegDword $adv 'A'   0   # keep me\nRegDword $adv 'B'   1   # drop me";

        var result = AutounattendGenerator.AssembleScript(script, [], ["RegDword $adv 'B' 1"]);

        Assert.Contains("# RegDword $adv 'B'   1   # drop me", result);   // commented despite the spacing mismatch
        Assert.Contains("RegDword $adv 'A'   0   # keep me", result);     // untouched
        Assert.DoesNotContain("\nRegDword $adv 'B'", result);            // no active B line remains
    }

    [Fact]
    public void FillAssignments_rewrites_the_marked_line_value_and_keeps_the_marker()
    {
        var content = "$dns = @('1.1.1.1', '1.0.0.1')   # [InDows:param DNS] Cloudflare\nLog $dns";

        var result = AutounattendGenerator.FillAssignments(content,
            new Dictionary<string, string> { ["DNS"] = "@('9.9.9.9', '149.112.112.112')" });

        Assert.Contains("$dns = @('9.9.9.9', '149.112.112.112')   # [InDows:param DNS] Cloudflare", result);
        Assert.DoesNotContain("1.1.1.1", result);
        Assert.Contains("Log $dns", result);   // unmarked lines untouched
    }

    [Fact]
    public void FillAssignments_without_a_marker_is_unchanged()
    {
        Assert.Equal("$x = 1", AutounattendGenerator.FillAssignments("$x = 1", new Dictionary<string, string> { ["DNS"] = "y" }));
    }

    [Fact]
    public void AppendProfileApps_adds_winget_entries_to_the_dsc_block()
    {
        var xml =
            "<x>\n<File path=\"C:\\Windows\\Setup\\Scripts\\configuration.dsc.yaml\"><![CDATA[\n" +
            "  resources:\n    - resource: Microsoft.WinGet.DSC/WinGetPackage\n      id: brave\n" +
            "      settings: { id: Brave.Brave, source: winget }\n]]></File>\n</x>";

        var result = AutounattendGenerator.AppendProfileApps(xml, new[] { "Microsoft.VisualStudioCode", "Git.Git" });

        Assert.Contains("settings: { id: Microsoft.VisualStudioCode, source: winget }", result);
        Assert.Contains("settings: { id: Git.Git, source: winget }", result);
        // The base entry survives, and the new entries land inside the CDATA (before the close).
        Assert.Contains("id: Brave.Brave", result);
        Assert.True(result.IndexOf("Git.Git", StringComparison.Ordinal) < result.IndexOf("]]></File>", StringComparison.Ordinal));
    }

    [Fact]
    public void AppendProfileApps_with_no_apps_is_unchanged()
    {
        Assert.Equal("<x/>", AutounattendGenerator.AppendProfileApps("<x/>", []));
    }

    [Fact]
    public void FillParams_replaces_every_placeholder_occurrence()
    {
        var content = "<Name>__USERNAME__</Name><Value>__PASSWORD__</Value><Also>__USERNAME__</Also>";

        var result = AutounattendGenerator.FillParams(content,
            new Dictionary<string, string> { ["USERNAME"] = "Tom", ["PASSWORD"] = "" });

        Assert.Equal("<Name>Tom</Name><Value></Value><Also>Tom</Also>", result);
    }

    [Fact]
    public void Compose_grafts_a_script_module_as_a_file_plus_a_run_command()
    {
        var grafts = new[] { new ModuleGraft("misc", ModuleKind.Script, "specialize-scripts", "RegDword X Y 1") };

        var result = AutounattendGenerator.Compose(ComposeBase, grafts);

        Load(result); // still well-formed XML
        Assert.Contains("<RunSynchronousCommand", result);
        Assert.Contains(@"C:\Windows\Setup\Scripts\misc.ps1", result);
        Assert.Contains("<![CDATA[", result);
        Assert.Contains("RegDword X Y 1", result);
        Assert.Contains("<Order>51</Order>", result);   // specialize-scripts starts after the hive unload
    }

    [Fact]
    public void Compose_orders_two_modules_at_the_same_anchor_distinctly()
    {
        var grafts = new[]
        {
            new ModuleGraft("a", ModuleKind.Script, "specialize-scripts", "# a"),
            new ModuleGraft("b", ModuleKind.Script, "specialize-scripts", "# b"),
        };

        var result = AutounattendGenerator.Compose(ComposeBase, grafts);

        Load(result);
        Assert.Contains("<Order>51</Order>", result);
        Assert.Contains("<Order>52</Order>", result);
    }
}

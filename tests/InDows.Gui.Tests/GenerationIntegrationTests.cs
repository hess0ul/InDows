using System.IO;
using System.Xml;
using InDows.Core.Build;
using Xunit;

namespace InDows.Gui.Tests;

/// <summary>
/// Grafts real modules onto the actual shipping base (both bundled next to this test). A guard against the
/// base or catalog drifting out of sync with the generator — a renamed anchor or a broken <File> block would
/// fail well-formedness here, not silently in a user's install.
/// </summary>
public class GenerationIntegrationTests
{
    private static string DataPath(string name) => Path.Combine(AppContext.BaseDirectory, "data", name);

    [Fact]
    public void The_real_base_plus_every_module_composes_into_well_formed_xml()
    {
        var baseXml = new FileBaseTemplate(DataPath("autounattend.base.xml")).Read();
        var modules = new JsonModuleCatalog(DataPath("modules.catalog.json")).Load();

        // Select every module whole: snippets grafted as-is, script modules as their full script.
        var grafts = modules.Select(m => new ModuleGraft(m.Name, m.Kind, m.Anchor, m.Content)).ToList();

        var result = AutounattendGenerator.Compose(baseXml, grafts);

        new XmlDocument().LoadXml(result);   // throws if a graft broke well-formedness

        Assert.Contains("<RunSynchronousCommand", result);   // [S]/[U] script modules
        Assert.Contains("<SynchronousCommand", result);      // [F] script modules
        Assert.Contains("<![CDATA[", result);                // script <File> blocks
        foreach (var module in modules.Where(m => m.Kind == ModuleKind.Script))
        {
            Assert.Contains($@"C:\Windows\Setup\Scripts\{module.Name}.ps1", result);
        }
    }

    /// <summary>The security module ships its "danger zone" commented out (safe standalone). Ticking a danger-zone
    /// setting must uncomment its line; leaving it unticked keeps it commented — the whole point of the module.</summary>
    [Fact]
    public void A_selected_security_danger_zone_setting_is_uncommented_only_when_ticked()
    {
        var module = new JsonModuleCatalog(DataPath("modules.catalog.json")).Load().Single(m => m.Name == "security");
        var bitlocker = module.Tweaks.Single(t => t.Id == "bitlocker-auto-off").Content;
        var uacDisable = module.Tweaks.Single(t => t.Id == "uac-disable").Content;

        // Danger zone unticked (only the safe BitLocker default selected): its line stays commented.
        var off = AutounattendGenerator.AssembleScript(module.Content, [bitlocker],
            module.Tweaks.Where(t => t.Id != "bitlocker-auto-off").Select(t => t.Content).ToList());
        Assert.Contains("# " + uacDisable, off, StringComparison.Ordinal);

        // Ticking the UAC-disable setting activates its line.
        var on = AutounattendGenerator.AssembleScript(module.Content, [bitlocker, uacDisable],
            module.Tweaks.Where(t => t.Id != "bitlocker-auto-off" && t.Id != "uac-disable").Select(t => t.Content).ToList());
        Assert.DoesNotContain("# " + uacDisable, on, StringComparison.Ordinal);
        Assert.Contains(uacDisable, on, StringComparison.Ordinal);
    }

    /// <summary>Every individual setting must carry a plain-language hover description, so no tweak ships showing
    /// only its raw registry key. Guards against a new tweak being added without a description entry.</summary>
    [Fact]
    public void Every_tweak_in_the_catalog_has_a_hover_description()
    {
        var modules = new JsonModuleCatalog(DataPath("modules.catalog.json")).Load();

        var undescribed = modules
            .SelectMany(m => m.Tweaks.Select(t => (Module: m.Name, t.Id, t.Description)))
            .Where(t => string.IsNullOrWhiteSpace(t.Description))
            .Select(t => $"{t.Module}/{t.Id}")
            .ToList();

        Assert.True(undescribed.Count == 0, "Tweaks with no description: " + string.Join(", ", undescribed));
    }

    /// <summary>Every decomposed module's tweak fragments must match a real line of its script, or a deselected
    /// tweak silently stays active. Assemble each module keeping ONE tweak: that tweak's line stays live and all
    /// the others get commented — proving every fragment lines up with the shipping script.</summary>
    [Theory]
    [InlineData("debloat-appx")]
    [InlineData("optional-features")]
    public void Every_tweak_fragment_of_a_decomposed_module_matches_a_script_line(string moduleName)
    {
        var module = new JsonModuleCatalog(DataPath("modules.catalog.json")).Load().Single(m => m.Name == moduleName);
        Assert.NotEmpty(module.Tweaks);

        var kept = module.Tweaks[0];
        var deselected = module.Tweaks.Skip(1).Select(t => t.Content).ToList();
        var script = AutounattendGenerator.AssembleScript(module.Content, [kept.Content], deselected);

        // The kept tweak's line is still active (not commented).
        Assert.Contains(kept.Content, script, StringComparison.Ordinal);
        Assert.DoesNotContain("# " + kept.Content, script, StringComparison.Ordinal);
        // Every other tweak's line got commented out (fragment matched a real line).
        foreach (var content in deselected)
        {
            Assert.Contains("# " + content, script, StringComparison.Ordinal);
        }
    }
}

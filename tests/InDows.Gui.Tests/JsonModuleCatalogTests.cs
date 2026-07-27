using System.IO;
using InDows.Core.Build;
using Xunit;

namespace InDows.Gui.Tests;

public sealed class JsonModuleCatalogTests : IDisposable
{
    private readonly string _path;

    public JsonModuleCatalogTests()
    {
        _path = Path.Combine(Path.GetTempPath(), "indows-catalog-test-" + Guid.NewGuid().ToString("N") + ".json");
    }

    public void Dispose()
    {
        try
        {
            File.Delete(_path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public void Reads_entries_and_parses_the_lowercase_risk_and_kind()
    {
        File.WriteAllText(_path, """
            {
              "modules": [
                { "name": "disable-ai", "category": "Privacy", "risk": "safe", "kind": "script",
                  "anchor": "specialize-scripts", "description": "Turn off Copilot.",
                  "why": "You don't want AI running.", "riskNote": "Policy keys only.", "content": "# ps",
                  "tweaks": [
                    { "id": "copilot", "label": "Copilot off", "risk": "safe", "default": true, "content": "reg copilot" },
                    { "id": "recall", "label": "Recall off", "risk": "advanced", "default": false, "content": "reg recall" }
                  ] },
                { "name": "autologon", "category": "Identity & setup", "risk": "advanced", "kind": "snippet",
                  "anchor": "account", "description": "Sign in automatically.",
                  "why": "Skip the password prompt.", "riskNote": "Clear-text password.", "content": "<x/>",
                  "params": [ { "key": "USERNAME", "label": "Account name", "kind": "text", "default": "User" } ] }
              ]
            }
            """);

        var modules = new JsonModuleCatalog(_path).Load();

        Assert.Equal(2, modules.Count);
        var autologon = Assert.Single(modules, m => m.Name == "autologon");
        Assert.Equal(ModuleRisk.Advanced, autologon.Risk);
        Assert.Equal(ModuleKind.Snippet, autologon.Kind);
        Assert.Equal("Identity & setup", autologon.Category);
        Assert.Equal("Skip the password prompt.", autologon.Why);
        Assert.Equal("Clear-text password.", autologon.RiskNote);
        Assert.Equal("<x/>", autologon.Content);
        Assert.Empty(autologon.Tweaks);                     // no "tweaks" in JSON -> empty, never null
        var param = Assert.Single(autologon.Params);
        Assert.Equal("USERNAME", param.Key);
        Assert.Equal(ParamKind.Text, param.Kind);

        var disableAi = Assert.Single(modules, m => m.Name == "disable-ai");
        Assert.Equal(2, disableAi.Tweaks.Count);
        Assert.Equal("Copilot off", disableAi.Tweaks[0].Label);
        Assert.Equal(ModuleRisk.Advanced, disableAi.Tweaks[1].Risk);
        Assert.False(disableAi.Tweaks[1].Default);
    }

    [Fact]
    public void A_missing_file_is_rejected()
    {
        Assert.Throws<InvalidOperationException>(() => new JsonModuleCatalog(_path).Load());
    }

    [Fact]
    public void A_malformed_file_is_rejected()
    {
        File.WriteAllText(_path, "{ not json");

        Assert.Throws<InvalidOperationException>(() => new JsonModuleCatalog(_path).Load());
    }
}

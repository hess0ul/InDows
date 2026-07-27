using InDows.Core.Build;
using InDows.Core.Profile;
using InDows.Gui.ViewModels;
using Xunit;

namespace InDows.Gui.Tests;

public class BuildViewModelTests
{
    private static ModuleTweak Tweak(string id) => new(id, id, ModuleRisk.Safe, false, $"# {id}");

    private const string Base =
        "<unattend xmlns:wcm=\"x\">\n" +
        "  <settings pass=\"specialize\">\n" +
        "    <RunSynchronous>\n" +
        "      <!-- [InDows:module] specialize-scripts -->\n" +
        "    </RunSynchronous>\n" +
        "  </settings>\n" +
        "  <settings pass=\"oobeSystem\">\n" +
        "    <!-- [InDows:module] account -->\n" +
        "  </settings>\n" +
        "  <Extensions xmlns=\"\">\n" +
        "  </Extensions>\n" +
        "</unattend>\n";

    private const string BaseWithDsc =
        "<unattend xmlns:wcm=\"x\">\n" +
        "  <settings pass=\"specialize\">\n" +
        "    <RunSynchronous>\n" +
        "      <!-- [InDows:module] specialize-scripts -->\n" +
        "    </RunSynchronous>\n" +
        "  </settings>\n" +
        "  <Extensions xmlns=\"\">\n" +
        "    <File path=\"C:\\Windows\\Setup\\Scripts\\configuration.dsc.yaml\"><![CDATA[\n" +
        "properties:\n" +
        "  resources:\n" +
        "    - resource: Microsoft.WinGet.DSC/WinGetPackage\n" +
        "      id: brave\n" +
        "      settings: { id: Brave.Brave, source: winget }\n" +
        "]]></File>\n" +
        "  </Extensions>\n" +
        "</unattend>\n";

    private static ModuleCatalogEntry Entry(string name, string category, ModuleRisk risk = ModuleRisk.Safe, params ModuleTweak[] tweaks) =>
        new(name, category, risk, ModuleKind.Script, "specialize-scripts",
            $"{name} does a thing.", $"why {name}", $"risk of {name}", "# content", tweaks, []);

    private static BuildViewModel WithModules(params ModuleCatalogEntry[] modules) =>
        new(new FakeModuleCatalog(modules), new FakeBaseTemplate(Base), new FakeFileSaver(),
            new FakeProfileReader(), new FakeFolderBrowser(null), new FakeAppSearch());

    [Fact]
    public void It_loads_and_groups_modules_by_category()
    {
        var vm = WithModules(
            Entry("privacy-telemetry", "Privacy"),
            Entry("disable-ai", "Privacy"),
            Entry("dark-mode", "UI & shell"));

        Assert.Equal(2, vm.Categories.Count);
        var privacy = Assert.Single(vm.Categories, c => c.Name == "Privacy");
        Assert.Equal(2, privacy.Modules.Count);
        // Alphabetised within the group.
        Assert.Equal("disable-ai", privacy.Modules[0].Name);
    }

    [Fact]
    public void The_advanced_category_is_gated_until_accepted()
    {
        var vm = WithModules(Entry("developer-tools", "Advanced"), Entry("misc", "System"));
        var advanced = vm.Categories.Single(c => c.Name == "Advanced");
        var system = vm.Categories.Single(c => c.Name == "System");

        Assert.True(advanced.IsGated);
        Assert.False(advanced.ShowModules);      // hidden behind the disclaimer
        Assert.True(advanced.ShowDisclaimer);
        Assert.False(system.IsGated);            // normal categories are always shown
        Assert.True(system.ShowModules);

        advanced.AcceptCommand.Execute(null);

        Assert.True(advanced.ShowModules);        // revealed after accepting
        Assert.False(advanced.ShowDisclaimer);
    }

    [Fact]
    public void Selecting_whole_modules_updates_the_summary()
    {
        var vm = WithModules(Entry("a", "Privacy"), Entry("b", "System"));

        vm.Categories[0].Modules[0].IsSelected = true;
        vm.Categories[1].Modules[0].IsSelected = true;

        Assert.Contains("2 setting(s) across 2 module(s)", vm.Summary);
    }

    [Fact]
    public void A_decomposed_module_is_tri_state_and_counts_individual_settings()
    {
        var vm = WithModules(Entry("explorer-ui", "UI & shell", ModuleRisk.Safe, Tweak("a"), Tweak("b"), Tweak("c")));
        var module = vm.Categories[0].Modules[0];

        Assert.True(module.HasTweaks);
        Assert.Equal(3, module.Tweaks.Count);
        Assert.Equal((bool?)false, module.IsSelected);     // nothing pre-selected

        module.Tweaks[0].IsSelected = true;
        Assert.Null(module.IsSelected);                    // a mix -> indeterminate
        Assert.Contains("1 setting(s) across 1 module(s)", vm.Summary);

        module.IsSelected = true;                          // parent ticks all
        Assert.Equal((bool?)true, module.IsSelected);
        Assert.All(module.Tweaks, t => Assert.True(t.IsSelected));
        Assert.Contains("3 setting(s) across 1 module(s)", vm.Summary);

        module.IsSelected = false;                         // parent unticks all
        Assert.Equal((bool?)false, module.IsSelected);
        Assert.All(module.Tweaks, t => Assert.False(t.IsSelected));
    }

    [Fact]
    public void Generating_a_decomposed_module_comments_only_the_deselected_settings()
    {
        var script = "RegDword $adv 'Keep' 1   # keep\nRegDword $adv 'Drop' 1   # drop";
        var entry = new ModuleCatalogEntry("explorer-ui", "UI & shell", ModuleRisk.Safe, ModuleKind.Script,
            "specialize-scripts", "desc", "why", "risk", script,
            [new ModuleTweak("keep", "Keep it", ModuleRisk.Safe, true, "RegDword $adv 'Keep' 1"),
             new ModuleTweak("drop", "Drop it", ModuleRisk.Safe, true, "RegDword $adv 'Drop' 1")],
            []);
        var vm = WithModules(entry);
        var module = vm.Categories[0].Modules[0];
        module.Tweaks.Single(t => t.Label == "Keep it").IsSelected = true;   // "Drop it" stays unselected

        vm.GenerateCommand.Execute(null);

        Assert.Contains("RegDword $adv 'Keep' 1   # keep", vm.PreviewXml);      // selected -> active
        Assert.Contains("# RegDword $adv 'Drop' 1   # drop", vm.PreviewXml);    // deselected -> commented
    }

    [Fact]
    public void Changing_the_mode_updates_the_summary()
    {
        var vm = WithModules();

        Assert.Contains("Clean install", vm.Summary);

        vm.Mode = BuildMode.FullRestore;

        Assert.Contains("Full restore", vm.Summary);
    }

    [Fact]
    public void A_load_failure_becomes_an_error()
    {
        var vm = new BuildViewModel(new FakeModuleCatalog(failure: new InvalidOperationException("no catalog")),
            new FakeBaseTemplate(Base), new FakeFileSaver(), new FakeProfileReader(), new FakeFolderBrowser(null), new FakeAppSearch());

        Assert.NotNull(vm.Error);
        Assert.Empty(vm.Categories);
    }

    [Fact]
    public void Generate_grafts_selected_modules_into_the_preview()
    {
        var vm = WithModules(Entry("misc", "System"));
        vm.Categories[0].Modules[0].IsSelected = true;

        vm.GenerateCommand.Execute(null);

        Assert.True(vm.HasPreview);
        Assert.Contains("misc.ps1", vm.PreviewXml);
        Assert.Contains("<RunSynchronousCommand", vm.PreviewXml);
        Assert.Contains("<![CDATA[", vm.PreviewXml);
        Assert.Null(vm.Error);
    }

    [Fact]
    public void Generate_flags_leftover_placeholders()
    {
        var snippet = new ModuleCatalogEntry("username", "Identity & setup", ModuleRisk.Safe, ModuleKind.Snippet,
            "account", "creates the account", "skip the OOBE screen", "clear-text password", "<Name>__USERNAME__</Name>", [], []);
        var vm = WithModules(snippet);
        vm.Categories[0].Modules[0].IsSelected = true;

        vm.GenerateCommand.Execute(null);

        Assert.Contains("__USERNAME__", vm.PlaceholderWarning);
    }

    [Fact]
    public void Filling_a_parameter_substitutes_it_and_clears_the_warning()
    {
        var withParam = new ModuleCatalogEntry("username", "Identity & setup", ModuleRisk.Safe, ModuleKind.Snippet,
            "account", "creates the account", "skip the OOBE screen", "clear-text password", "<Name>__USERNAME__</Name>", [],
            [new ModuleParam("USERNAME", "Account name", ParamKind.Text, "User", [])]);
        var vm = WithModules(withParam);
        var module = vm.Categories[0].Modules[0];
        module.IsSelected = true;
        module.Params[0].Value = "Thomas";

        vm.GenerateCommand.Execute(null);

        Assert.Contains("<Name>Thomas</Name>", vm.PreviewXml);
        Assert.DoesNotContain("__USERNAME__", vm.PreviewXml);
        Assert.Null(vm.PlaceholderWarning);
    }

    [Fact]
    public void A_choice_param_defaults_to_its_default_and_substitutes_the_chosen_value()
    {
        var options = new[] { new ParamOption("UTC", "UTC"), new ParamOption("Paris", "Romance Standard Time") };
        var tz = new ModuleCatalogEntry("timezone", "Identity & setup", ModuleRisk.Safe, ModuleKind.Snippet,
            "specialize-scripts", "sets the clock", "right from boot", "trivial", "<TimeZone>__TIMEZONE__</TimeZone>", [],
            [new ModuleParam("TIMEZONE", "Time zone", ParamKind.Choice, "UTC", options)]);
        var vm = WithModules(tz);
        var module = vm.Categories[0].Modules[0];
        module.IsSelected = true;

        Assert.Equal("UTC", module.Params[0].Value);   // seeded from the default
        module.Params[0].Value = "Romance Standard Time";
        vm.GenerateCommand.Execute(null);

        Assert.Contains("<TimeZone>Romance Standard Time</TimeZone>", vm.PreviewXml);
    }

    [Fact]
    public void A_preset_choice_rewrites_the_marked_line_in_the_generated_script()
    {
        var entry = new ModuleCatalogEntry("services-tune", "System", ModuleRisk.Risky, ModuleKind.Script,
            "specialize-scripts", "tunes services", "trim background load", "hardening can break things",
            "$preset = 'safe'   # [InDows:param PRESET]\nLog $preset", [],
            [new ModuleParam("PRESET", "Preset", ParamKind.Choice, "'safe'",
                [new ParamOption("Safe", "'safe'"), new ParamOption("Hardening", "'hardening'")])]);
        var vm = WithModules(entry);
        var module = vm.Categories[0].Modules[0];
        module.IsSelected = true;
        module.Params[0].Value = "'hardening'";

        vm.GenerateCommand.Execute(null);

        Assert.Contains("$preset = 'hardening'   # [InDows:param PRESET]", vm.PreviewXml);
    }

    [Fact]
    public void A_number_param_rewrites_the_marked_assignment_with_the_typed_value()
    {
        var entry = new ModuleCatalogEntry("storage-sense", "System", ModuleRisk.Safe, ModuleKind.Script,
            "specialize-scripts", "storage sense", "auto cleanup", "reversible",
            "$recycleDays = 60   # [InDows:param RECYCLE_DAYS]\nRegDword $sp '256' $recycleDays", [],
            [new ModuleParam("RECYCLE_DAYS", "Recycle Bin days", ParamKind.Number, "60", [])]);
        var vm = WithModules(entry);
        var module = vm.Categories[0].Modules[0];
        module.IsSelected = true;
        module.Params[0].Value = "30";

        vm.GenerateCommand.Execute(null);

        Assert.Contains("$recycleDays = 30   # [InDows:param RECYCLE_DAYS]", vm.PreviewXml);
    }

    [Fact]
    public void A_file_param_substitutes_the_chosen_path()
    {
        var entry = new ModuleCatalogEntry("wallpaper", "Identity & setup", ModuleRisk.Safe, ModuleKind.Snippet,
            "specialize-scripts", "wallpaper", "personalise", "cosmetic",
            "$desktop = '__WALLPAPER__'", [],
            [new ModuleParam("WALLPAPER", "Desktop image", ParamKind.File, "", [])]);
        var vm = WithModules(entry);
        var module = vm.Categories[0].Modules[0];
        module.IsSelected = true;
        module.Params[0].Value = @"C:\Pictures\bg.jpg";

        vm.GenerateCommand.Execute(null);

        Assert.Contains(@"$desktop = 'C:\Pictures\bg.jpg'", vm.PreviewXml);
    }

    [Fact]
    public void A_custom_dns_choice_formats_the_typed_ips_into_the_script()
    {
        var entry = new ModuleCatalogEntry("network-dns", "System", ModuleRisk.Safe, ModuleKind.Script,
            "specialize-scripts", "sets dns", "speed & privacy", "reversible",
            "$dns = @('1.1.1.1', '1.0.0.1')   # [InDows:param DNS]", [],
            [new ModuleParam("DNS", "DNS resolver", ParamKind.Choice, "@('1.1.1.1', '1.0.0.1')",
                [new ParamOption("Cloudflare", "@('1.1.1.1', '1.0.0.1')"), new ParamOption("Custom…", ParamRow.CustomOption)])]);
        var vm = WithModules(entry);
        var module = vm.Categories[0].Modules[0];
        module.IsSelected = true;
        module.Params[0].Value = ParamRow.CustomOption;
        Assert.True(module.Params[0].IsCustom);
        module.Params[0].CustomPrimary = "9.9.9.9";
        module.Params[0].CustomSecondary = "8.8.8.8";

        vm.GenerateCommand.Execute(null);

        Assert.Contains("$dns = @('9.9.9.9', '8.8.8.8')   # [InDows:param DNS]", vm.PreviewXml);
    }

    [Fact]
    public void A_custom_dns_with_only_a_primary_drops_the_blank_secondary()
    {
        var entry = new ModuleCatalogEntry("network-dns", "System", ModuleRisk.Safe, ModuleKind.Script,
            "specialize-scripts", "sets dns", "speed & privacy", "reversible",
            "$dns = @('1.1.1.1', '1.0.0.1')   # [InDows:param DNS]", [],
            [new ModuleParam("DNS", "DNS resolver", ParamKind.Choice, "@('1.1.1.1', '1.0.0.1')",
                [new ParamOption("Custom…", ParamRow.CustomOption)])]);
        var vm = WithModules(entry);
        var module = vm.Categories[0].Modules[0];
        module.IsSelected = true;
        module.Params[0].Value = ParamRow.CustomOption;
        module.Params[0].CustomPrimary = "1.1.1.1";

        vm.GenerateCommand.Execute(null);

        Assert.Contains("$dns = @('1.1.1.1')   # [InDows:param DNS]", vm.PreviewXml);
    }

    [Fact]
    public void Save_writes_the_preview_and_reports_the_path()
    {
        var saver = new FakeFileSaver();
        var vm = new BuildViewModel(new FakeModuleCatalog([Entry("misc", "System")]), new FakeBaseTemplate(Base), saver,
            new FakeProfileReader(), new FakeFolderBrowser(null), new FakeAppSearch());
        vm.Categories[0].Modules[0].IsSelected = true;
        vm.GenerateCommand.Execute(null);

        vm.SaveCommand.Execute(null);

        Assert.Equal(vm.PreviewXml, saver.SavedContent);
        Assert.Contains("Saved to", vm.SavedPath);
    }

    [Fact]
    public void M2_adds_the_profile_apps_to_the_winget_catalog()
    {
        var profile = new FakeProfileReader(new ProfileSummary(["Microsoft.VisualStudioCode", "Git.Git"], 0, [], []));
        var vm = new BuildViewModel(new FakeModuleCatalog([Entry("misc", "System")]),
            new FakeBaseTemplate(BaseWithDsc), new FakeFileSaver(), profile, new FakeFolderBrowser(@"C:\profile"), new FakeAppSearch());
        vm.Mode = BuildMode.CleanWithProfile;

        Assert.True(vm.RequiresProfile);
        vm.BrowseProfileCommand.Execute(null);
        Assert.Contains("2 app(s)", vm.ProfileInfo);

        vm.GenerateCommand.Execute(null);

        Assert.Contains("id: Microsoft.VisualStudioCode, source: winget", vm.PreviewXml);
        Assert.Contains("id: Git.Git, source: winget", vm.PreviewXml);
    }

    private static BuildViewModel WithAppSearch(FakeAppSearch search) =>
        new(new FakeModuleCatalog([Entry("misc", "System")]), new FakeBaseTemplate(BaseWithDsc),
            new FakeFileSaver(), new FakeProfileReader(), new FakeFolderBrowser(null), search);

    [Fact]
    public async Task Searching_then_picking_an_app_adds_it_to_the_winget_catalog()
    {
        var vm = WithAppSearch(new FakeAppSearch(
            [new AppSearchResult("Mozilla Firefox", "Mozilla.Firefox"), new AppSearchResult("Firefox ESR", "Mozilla.Firefox.ESR")]));

        vm.StartAddAppCommand.Execute(null);
        Assert.True(vm.IsAddingApp);
        vm.AppSearchQuery = "firefox";
        await vm.SearchAppsAsync();

        Assert.Equal(2, vm.AppSearchResults.Count);
        vm.PickAppCommand.Execute(vm.AppSearchResults[0]);

        var picked = Assert.Single(vm.Apps);
        Assert.Equal("Mozilla.Firefox", picked.Id);
        Assert.False(vm.IsAddingApp);   // the panel closes once an app is picked

        vm.GenerateCommand.Execute(null);
        Assert.Contains("id: Mozilla.Firefox, source: winget", vm.PreviewXml);
    }

    [Fact]
    public void Picking_the_same_app_twice_keeps_a_single_entry()
    {
        var vm = WithAppSearch(new FakeAppSearch());

        vm.PickAppCommand.Execute(new AppSearchResult("Git", "Git.Git"));
        vm.PickAppCommand.Execute(new AppSearchResult("Git (again)", "git.git"));   // same id, different case

        Assert.Single(vm.Apps);
    }

    [Fact]
    public void Removing_a_picked_app_drops_it_from_the_list()
    {
        var vm = WithAppSearch(new FakeAppSearch());
        vm.PickAppCommand.Execute(new AppSearchResult("Git", "Git.Git"));
        var row = Assert.Single(vm.Apps);

        row.RemoveCommand.Execute(null);

        Assert.Empty(vm.Apps);
    }

    [Fact]
    public async Task A_search_with_no_matches_reports_it()
    {
        var vm = WithAppSearch(new FakeAppSearch([]));
        vm.AppSearchQuery = "nothing-here";

        await vm.SearchAppsAsync();

        Assert.Empty(vm.AppSearchResults);
        Assert.Contains("No matches", vm.SearchError);
    }

    [Fact]
    public async Task A_failing_search_surfaces_the_error_without_crashing()
    {
        var vm = WithAppSearch(new FakeAppSearch(failure: new InvalidOperationException("winget missing")));
        vm.AppSearchQuery = "firefox";

        await vm.SearchAppsAsync();

        Assert.Contains("winget missing", vm.SearchError);
        Assert.False(vm.Searching);
    }

    private const string BaseWithDisk =
        "<unattend xmlns:wcm=\"x\">\n" +
        "  <settings pass=\"windowsPE\">\n" +
        "    <component name=\"Microsoft-Windows-Setup\">\n" +
        "      <!-- [InDows:module] disk -->\n" +
        "    </component>\n" +
        "  </settings>\n" +
        "</unattend>\n";

    private static ModuleCatalogEntry DiskEntry() =>
        new("disk", "Advanced", ModuleRisk.Risky, ModuleKind.Snippet, "disk",
            "automated disk layout", "zero-click install", "wipes the disk", "<!-- static -->", [], []);

    [Fact]
    public void The_disk_module_generates_a_partition_layout_from_its_editor()
    {
        var vm = new BuildViewModel(new FakeModuleCatalog([DiskEntry()]), new FakeBaseTemplate(BaseWithDisk),
            new FakeFileSaver(), new FakeProfileReader(), new FakeFolderBrowser(null), new FakeAppSearch());
        var module = vm.Categories[0].Modules[0];
        Assert.True(module.IsDiskEditor);
        module.IsSelected = true;                       // a whole-module snippet: ticking selects it

        vm.GenerateCommand.Execute(null);

        Assert.Contains("<DiskConfiguration>", vm.PreviewXml);
        Assert.Contains("<WillWipeDisk>true</WillWipeDisk>", vm.PreviewXml);
        Assert.Contains("<InstallTo>", vm.PreviewXml);
        Assert.Null(vm.Error);
    }

    [Fact]
    public void An_invalid_disk_layout_becomes_an_error_not_a_broken_file()
    {
        var vm = new BuildViewModel(new FakeModuleCatalog([DiskEntry()]), new FakeBaseTemplate(BaseWithDisk),
            new FakeFileSaver(), new FakeProfileReader(), new FakeFolderBrowser(null), new FakeAppSearch());
        var module = vm.Categories[0].Modules[0];
        module.IsSelected = true;
        foreach (var p in module.DiskEditor!.Partitions)
        {
            p.ClearInstall();                           // no install target left -> invalid layout
        }

        vm.GenerateCommand.Execute(null);

        Assert.Null(vm.PreviewXml);
        Assert.Contains("Mark which partition", vm.Error);
    }

    [Fact]
    public void A_profile_ticks_its_setting_modules_and_notes_the_missing_ones()
    {
        var summary = new ProfileSummary([], 0, [], ["explorer-ui", "not-a-module"]);
        var vm = new BuildViewModel(new FakeModuleCatalog([Entry("explorer-ui", "UI & shell"), Entry("misc", "System")]),
            new FakeBaseTemplate(BaseWithDsc), new FakeFileSaver(), new FakeProfileReader(summary), new FakeFolderBrowser(@"C:\profile"), new FakeAppSearch());
        vm.Mode = BuildMode.CleanWithProfile;

        vm.BrowseProfileCommand.Execute(null);

        var explorer = vm.Categories.SelectMany(c => c.Modules).Single(m => m.Name == "explorer-ui");
        Assert.Equal((bool?)true, explorer.IsSelected);          // the profile's module was ticked
        Assert.Contains("1 setting module(s) selected", vm.ProfileInfo);
        Assert.Contains("not available in InDows: not-a-module", vm.ProfileInfo);
    }
}

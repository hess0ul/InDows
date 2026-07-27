namespace InDows.Core.Build;

/// <summary>The install InDows will produce. Determined by the ReDows profile provided, or chosen explicitly.</summary>
public enum BuildMode
{
    /// <summary>A clean, debloated Windows 11 with no personal profile (M1).</summary>
    Clean,

    /// <summary>Clean, plus the apps and settings from a ReDows profile (M2).</summary>
    CleanWithProfile,

    /// <summary>Clean, plus the profile, plus restoring a full ReDows backup (M3).</summary>
    FullRestore,
}

/// <summary>How a module plugs into the base: an XML snippet, or a PowerShell script.</summary>
public enum ModuleKind
{
    Snippet,
    Script,
}

/// <summary>How risky a module is, for the colour code in the checklist.</summary>
public enum ModuleRisk
{
    /// <summary>Reversible preference or cleanup, no security or stability impact.</summary>
    Safe,

    /// <summary>Changes behaviour, or is hardware/driver-dependent.</summary>
    Advanced,

    /// <summary>Can reduce security or break a Windows feature. Opt-in, clearly warned.</summary>
    Risky,
}

/// <summary>One individually-toggleable setting inside a module (e.g. "Show file extensions").</summary>
/// <remarks><see cref="Content"/> is the script/registry fragment this tweak contributes when selected;
/// <see cref="Default"/> is whether it starts ticked. <see cref="Description"/> is a plain-language line
/// (what it does / what changes) shown on hover; empty when none was authored.</remarks>
public sealed record ModuleTweak(
    string Id,
    string Label,
    ModuleRisk Risk,
    bool Default,
    string Content,
    string Description = "");

/// <summary>What kind of input a module parameter needs.</summary>
public enum ParamKind
{
    Text,
    Password,
    Choice,

    /// <summary>A whole number (e.g. a day count). Same substitution as <see cref="Text"/>, digits-only input.</summary>
    Number,

    /// <summary>A file path, with a "Browse…" button that opens an open-file dialog. Substituted like <see cref="Text"/>.</summary>
    File,
}

/// <summary>One entry in a <see cref="ParamKind.Choice"/> dropdown: what the user sees, and the value substituted.</summary>
public sealed record ParamOption(string Label, string Value);

/// <summary>One user-filled field for a module whose content has a <c>__KEY__</c> placeholder; the entered
/// (or chosen) value replaces every <c>__KEY__</c> at generation. <see cref="Options"/> is the dropdown list
/// for a <see cref="ParamKind.Choice"/>, empty otherwise.</summary>
public sealed record ModuleParam(
    string Key,
    string Label,
    ParamKind Kind,
    string Default,
    IReadOnlyList<ParamOption> Options);

/// <summary>One module in the bundled catalog: its metadata for the checklist, plus the content to graft.</summary>
/// <remarks><see cref="Description"/> is what it does; <see cref="Why"/> is the motivation; <see cref="RiskNote"/>
/// is the module-specific consequence — the three lines of the hover panel. <see cref="Tweaks"/> holds the
/// individual settings when the module has been decomposed; empty means the module is applied as one block.</remarks>
public sealed record ModuleCatalogEntry(
    string Name,
    string Category,
    ModuleRisk Risk,
    ModuleKind Kind,
    string Anchor,
    string Description,
    string Why,
    string RiskNote,
    string Content,
    IReadOnlyList<ModuleTweak> Tweaks,
    IReadOnlyList<ModuleParam> Params);

/// <summary>Provides the modules InDows can add. A seam, so the Build screen is testable off a fake.</summary>
public interface IModuleCatalog
{
    IReadOnlyList<ModuleCatalogEntry> Load();
}

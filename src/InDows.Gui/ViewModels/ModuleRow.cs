using InDows.Core.Build;

namespace InDows.Gui.ViewModels;

/// <summary>
/// One module in the Build checklist. A decomposed module (<see cref="HasTweaks"/>) expands into individual
/// <see cref="TweakRow"/> settings, and its own checkbox is tri-state: all / some / none. A plain module has
/// no tweaks and its checkbox is a simple on/off.
/// </summary>
public sealed class ModuleRow : ViewModelBase
{
    private readonly Action _onChanged;
    private bool _wholeSelected;
    private bool _isExpanded;

    public ModuleRow(ModuleCatalogEntry entry, Action onChanged)
    {
        Entry = entry;
        _onChanged = onChanged;
        Tweaks = entry.Tweaks.Select(t => new TweakRow(t, OnTweakChanged)).ToList();
        Params = entry.Params.Select(p => new ParamRow(p)).ToList();

        // The `disk` module is special: instead of tweaks/fields it drives a bespoke partition-table editor,
        // and its graft is generated from that layout rather than the bundled static snippet.
        if (string.Equals(entry.Name, "disk", StringComparison.Ordinal))
        {
            DiskEditor = new DiskEditorViewModel();
        }
    }

    public ModuleCatalogEntry Entry { get; }

    public IReadOnlyList<TweakRow> Tweaks { get; }

    public IReadOnlyList<ParamRow> Params { get; }

    /// <summary>The partition-table editor for the <c>disk</c> module; null for every other module.</summary>
    public DiskEditorViewModel? DiskEditor { get; }

    public bool IsDiskEditor => DiskEditor is not null;

    public bool HasTweaks => Tweaks.Count > 0;

    public bool HasParams => Params.Count > 0;

    /// <summary>Whether the module unfolds — into its settings, its fields, or the disk editor.</summary>
    public bool HasDrawer => HasTweaks || HasParams || IsDiskEditor;

    /// <summary>Footer label on the drawer: the settings count, a fields prompt, or the disk-editor label.</summary>
    public string DrawerLabel => IsDiskEditor ? "Partition layout" : HasTweaks ? $"{Tweaks.Count} options" : "Configure";

    public string Name => Entry.Name;

    public string Description => Entry.Description;

    public string Why => Entry.Why;

    public string RiskNote => Entry.RiskNote;

    public ModuleRisk Risk => Entry.Risk;

    public string RiskLabel => Entry.Risk.ToString().ToLowerInvariant();

    public bool IsExpanded
    {
        get => _isExpanded;
        set => Set(ref _isExpanded, value);
    }

    /// <summary>All ticked → true, none → false, a mix → null (indeterminate). Setting it applies to every tweak.</summary>
    public bool? IsSelected
    {
        get
        {
            if (!HasTweaks)
            {
                return _wholeSelected;
            }

            var selected = Tweaks.Count(t => t.IsSelected);
            return selected == 0 ? false : selected == Tweaks.Count ? true : null;
        }
        set
        {
            if (!HasTweaks)
            {
                Set(ref _wholeSelected, value ?? false);
                _onChanged();
                return;
            }

            var target = value ?? false;
            foreach (var tweak in Tweaks)
            {
                tweak.SetFromParent(target);
            }

            Raise(nameof(IsSelected));
            _onChanged();
        }
    }

    /// <summary>How many settings this module contributes: selected tweaks, or 1 for a ticked whole module.</summary>
    public int SelectedCount => HasTweaks ? Tweaks.Count(t => t.IsSelected) : _wholeSelected ? 1 : 0;

    /// <summary>The graft this module contributes to generation, or null if nothing in it is selected. For a
    /// decomposed module, the deselected settings are commented out of its script.</summary>
    public ModuleGraft? ToGraft()
    {
        if (SelectedCount == 0)
        {
            return null;
        }

        // The disk module builds its snippet from the partition editor (may throw on an invalid layout, which
        // Generate() surfaces as an error), instead of grafting a static snippet.
        if (IsDiskEditor)
        {
            return new ModuleGraft(Entry.Name, Entry.Kind, Entry.Anchor, DiskConfigGenerator.BuildSnippet(DiskEditor!.ToSpec()));
        }

        var content = HasTweaks
            ? AutounattendGenerator.AssembleScript(
                Entry.Content,
                Tweaks.Where(t => t.IsSelected).Select(t => t.Content).ToList(),
                Tweaks.Where(t => !t.IsSelected).Select(t => t.Content).ToList())
            : Entry.Content;

        if (HasParams)
        {
            var values = Params.ToDictionary(p => p.Key, p => p.EffectiveValue);
            content = AutounattendGenerator.FillParams(content, values);        // __KEY__ placeholders (snippets)
            content = AutounattendGenerator.FillAssignments(content, values);   // marked assignment lines (presets)
        }

        return new ModuleGraft(Entry.Name, Entry.Kind, Entry.Anchor, content);
    }

    private void OnTweakChanged()
    {
        Raise(nameof(IsSelected));
        _onChanged();
    }
}

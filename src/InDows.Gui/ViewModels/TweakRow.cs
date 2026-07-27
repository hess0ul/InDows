using InDows.Core.Build;

namespace InDows.Gui.ViewModels;

/// <summary>One individually-toggleable setting under a decomposed module.</summary>
public sealed class TweakRow : ViewModelBase
{
    private readonly Action _onChanged;
    private bool _isSelected;

    public TweakRow(ModuleTweak tweak, Action onChanged)
    {
        Tweak = tweak;
        // Start unticked: nothing is imposed, the user opts in. (Tweak.Default is reserved for a future
        // "Recommended" quick-pick, so we keep it in the data but don't pre-select from it.)
        _isSelected = false;
        _onChanged = onChanged;
    }

    public ModuleTweak Tweak { get; }

    public string Label => Tweak.Label;

    /// <summary>The exact registry/command fragment this option applies — shown on hover.</summary>
    public string Content => Tweak.Content;

    /// <summary>A plain-language explanation of what this option does — shown on hover above the fragment.</summary>
    public string Description => Tweak.Description;

    public bool HasDescription => !string.IsNullOrWhiteSpace(Tweak.Description);

    public ModuleRisk Risk => Tweak.Risk;

    public string RiskLabel => Tweak.Risk.ToString().ToLowerInvariant();

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            Set(ref _isSelected, value);
            _onChanged();
        }
    }

    /// <summary>Set the state from the parent's "select all/none" without firing the parent callback back.</summary>
    public void SetFromParent(bool value)
    {
        if (_isSelected != value)
        {
            _isSelected = value;
            Raise(nameof(IsSelected));
        }
    }
}

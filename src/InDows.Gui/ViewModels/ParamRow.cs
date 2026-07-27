using InDows.Core.Build;

namespace InDows.Gui.ViewModels;

/// <summary>One editable field for a module parameter; its <see cref="EffectiveValue"/> fills the module's placeholder.</summary>
public sealed class ParamRow : ViewModelBase
{
    /// <summary>Sentinel option value: when picked, the user types a custom value in a text field instead.</summary>
    public const string CustomOption = "__CUSTOM__";

    private string _value;
    private string _customPrimary = "";
    private string _customSecondary = "";

    public ParamRow(ModuleParam param)
    {
        Param = param;
        _value = param.Default;
    }

    public ModuleParam Param { get; }

    public string Key => Param.Key;

    public string Label => Param.Label;

    public bool IsText => Param.Kind == ParamKind.Text;

    public bool IsPassword => Param.Kind == ParamKind.Password;

    public bool IsChoice => Param.Kind == ParamKind.Choice;

    public bool IsNumber => Param.Kind == ParamKind.Number;

    public bool IsFile => Param.Kind == ParamKind.File;

    public IReadOnlyList<ParamOption> Options => Param.Options;

    /// <summary>True while the "Custom…" option is picked, so the two free-text fields show.</summary>
    public bool IsCustom => _value == CustomOption;

    /// <summary>First custom entry (e.g. primary DNS).</summary>
    public string CustomPrimary
    {
        get => _customPrimary;
        set => Set(ref _customPrimary, value);
    }

    /// <summary>Second custom entry (e.g. secondary DNS); optional.</summary>
    public string CustomSecondary
    {
        get => _customSecondary;
        set => Set(ref _customSecondary, value);
    }

    public string Value
    {
        get => _value;
        set
        {
            Set(ref _value, value);
            Raise(nameof(IsCustom));
        }
    }

    /// <summary>What actually gets substituted: the two custom entries as a PowerShell string array (dropping a
    /// blank second one) when "Custom…" is picked, otherwise the field/selection value.</summary>
    public string EffectiveValue => IsCustom ? AsPowerShellArray(_customPrimary, _customSecondary) : _value;

    private static string AsPowerShellArray(params string[] items)
    {
        var parts = items.Select(s => s.Trim()).Where(s => s.Length > 0);
        return "@(" + string.Join(", ", parts.Select(p => $"'{p}'")) + ")";
    }
}

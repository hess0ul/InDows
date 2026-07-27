using InDows.Gui.Navigation;

namespace InDows.Gui.ViewModels;

/// <summary>
/// A named group of modules in the Build checklist. The "Advanced" group is <see cref="IsGated"/>: its
/// higher-risk modules stay hidden behind a disclaimer until the user accepts it (<see cref="AcceptCommand"/>).
/// </summary>
public sealed class ModuleCategory : ViewModelBase
{
    private bool _revealed;

    public ModuleCategory(string name, IReadOnlyList<ModuleRow> modules)
    {
        Name = name;
        Modules = modules;
        IsGated = name == "Advanced";
        _revealed = !IsGated;
        AcceptCommand = new RelayCommand(_ => Reveal());
    }

    public string Name { get; }

    public IReadOnlyList<ModuleRow> Modules { get; }

    /// <summary>True for the "Advanced" group, whose modules are hidden until the disclaimer is accepted.</summary>
    public bool IsGated { get; }

    public bool ShowModules => _revealed;

    public bool ShowDisclaimer => IsGated && !_revealed;

    public RelayCommand AcceptCommand { get; }

    private void Reveal()
    {
        _revealed = true;
        Raise(nameof(ShowModules));
        Raise(nameof(ShowDisclaimer));
    }
}

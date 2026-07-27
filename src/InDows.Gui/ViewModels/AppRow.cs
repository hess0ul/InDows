using InDows.Core.Build;
using InDows.Gui.Navigation;

namespace InDows.Gui.ViewModels;

/// <summary>One app the user picked to install (name + winget id), with a command to remove it.</summary>
public sealed class AppRow
{
    public AppRow(AppSearchResult app, Action<AppRow> onRemove)
    {
        Name = app.Name;
        Id = app.Id;
        RemoveCommand = new RelayCommand(_ => onRemove(this));
    }

    public string Name { get; }

    public string Id { get; }

    public RelayCommand RemoveCommand { get; }
}

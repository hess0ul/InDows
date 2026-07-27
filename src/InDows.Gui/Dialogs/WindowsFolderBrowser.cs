using Microsoft.Win32;

namespace InDows.Gui.Dialogs;

/// <summary>The real folder picker: the Windows common folder dialog.</summary>
public sealed class WindowsFolderBrowser : IFolderBrowser
{
    public string? PickFolder(string title)
    {
        var dialog = new OpenFolderDialog { Title = title };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}

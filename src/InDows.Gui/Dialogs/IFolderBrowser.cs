namespace InDows.Gui.Dialogs;

/// <summary>Picks a folder for the user. A seam so view-models can be tested without opening a real dialog.</summary>
public interface IFolderBrowser
{
    /// <summary>Show a folder picker with the given title. Returns the chosen path, or null if cancelled.</summary>
    string? PickFolder(string title);
}

using InDows.Gui.Dialogs;

namespace InDows.Gui.Tests;

/// <summary>A folder picker that returns a fixed path (or null for "cancelled"), no dialog involved.</summary>
internal sealed class FakeFolderBrowser(string? pick) : IFolderBrowser
{
    public string? PickFolder(string title) => pick;
}

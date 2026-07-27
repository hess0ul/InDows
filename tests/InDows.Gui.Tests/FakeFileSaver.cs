using InDows.Gui.Dialogs;

namespace InDows.Gui.Tests;

/// <summary>A file saver that records what it was asked to save and returns a preset path (null = cancelled).</summary>
internal sealed class FakeFileSaver(string? path = @"C:\out\autounattend.xml") : IFileSaver
{
    public string? SavedContent { get; private set; }

    public string? Save(string suggestedName, string content)
    {
        SavedContent = content;
        return path;
    }
}

namespace InDows.Gui.Dialogs;

/// <summary>Saves generated text to a file the user picks. A seam, so the Build screen is testable off a fake.</summary>
public interface IFileSaver
{
    /// <summary>Prompts for a location and writes <paramref name="content"/> there. Returns the saved path, or null if cancelled.</summary>
    string? Save(string suggestedName, string content);
}

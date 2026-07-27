namespace InDows.Gui.Settings;

/// <summary>The app's persisted preferences. A shared, mutable holder: the Settings screen edits it live and
/// other screens read it (e.g. the file saver uses <see cref="OutputFolder"/> as its starting folder).</summary>
public sealed class AppSettings
{
    /// <summary>Folder the "Save as" dialog opens in for the generated autounattend.xml; empty = the OS default.</summary>
    public string OutputFolder { get; set; } = "";
}

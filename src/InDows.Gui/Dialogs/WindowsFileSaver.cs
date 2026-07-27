using System.IO;
using InDows.Gui.Settings;
using Microsoft.Win32;

namespace InDows.Gui.Dialogs;

/// <summary>The real file saver: the Windows "Save as" dialog (starting in the settings' output folder), then
/// writes the content as UTF-8.</summary>
public sealed class WindowsFileSaver(AppSettings settings) : IFileSaver
{
    public string? Save(string suggestedName, string content)
    {
        var dialog = new SaveFileDialog
        {
            FileName = suggestedName,
            Filter = "Answer file (*.xml)|*.xml|All files (*.*)|*.*",
            DefaultExt = ".xml",
        };

        if (Directory.Exists(settings.OutputFolder))
        {
            dialog.InitialDirectory = settings.OutputFolder;
        }

        if (dialog.ShowDialog() != true)
        {
            return null;
        }

        File.WriteAllText(dialog.FileName, content);
        return dialog.FileName;
    }
}

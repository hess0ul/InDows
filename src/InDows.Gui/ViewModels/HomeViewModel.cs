using System.Collections.ObjectModel;
using InDows.Core;
using InDows.Core.Context;

namespace InDows.Gui.ViewModels;

/// <summary>
/// The Home screen's brain: it asks the (injected, read-only) context source for the machine context
/// and shapes it into a drive list plus a one-line summary. Pure enough to test off a fake source: it
/// touches no WPF type. A provider failure becomes <see cref="Error"/>, never a crash.
/// </summary>
public sealed class HomeViewModel(IContextSource source) : ViewModelBase
{
    private string _summary = "";
    private string? _error;

    public ObservableCollection<DriveRow> Drives { get; } = [];

    public string Summary
    {
        get => _summary;
        private set => Set(ref _summary, value);
    }

    public string? Error
    {
        get => _error;
        private set => Set(ref _error, value);
    }

    public void Load()
    {
        try
        {
            var context = source.Load();

            Drives.Clear();
            foreach (var drive in context.Drives)
            {
                string detail;
                if (drive.Ready)
                {
                    var size = drive.TotalBytes is { } total ? Format.Gigabytes(total) : "?";
                    var free = drive.FreeBytes is { } freeBytes ? Format.Gigabytes(freeBytes) : "?";
                    detail = $"{drive.Format}, {size} ({free} free)";
                }
                else
                {
                    detail = "not ready";
                }

                Drives.Add(new DriveRow(drive.Name, detail, drive.Ready));
            }

            Summary = $"{context.MachineName} · {context.OsDescription} · {context.Drives.Count} drive(s)";
            Error = null;
        }
        catch (Exception ex)
        {
            Error = $"Could not read this PC's context: {ex.GetType().Name}: {ex.Message}";
        }
    }
}

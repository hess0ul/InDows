using System.Runtime.InteropServices;
using InDows.Core.Context;

namespace InDows.Providers.Windows.Context;

/// <summary>
/// Reads the machine context from Windows: computer name, OS description, and the fixed and removable
/// drives. Read-only: it inspects the machine, it never changes anything. A drive that is not ready
/// (an empty card reader, an unformatted disk) is reported without sizes rather than throwing.
/// </summary>
public sealed class WindowsContextSource : IContextSource
{
    public MachineContext Load()
    {
        var drives = new List<DriveContext>();
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType is not (DriveType.Fixed or DriveType.Removable))
            {
                continue;
            }

            drives.Add(drive.IsReady
                ? new DriveContext(drive.Name, drive.DriveFormat, drive.TotalSize, drive.AvailableFreeSpace, true)
                : new DriveContext(drive.Name, "?", null, null, false));
        }

        return new MachineContext(Environment.MachineName, RuntimeInformation.OSDescription, drives);
    }
}

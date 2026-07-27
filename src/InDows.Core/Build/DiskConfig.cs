using System.Globalization;
using System.Text;

namespace InDows.Core.Build;

/// <summary>A partition's role in the layout. Maps to the autounattend <c>&lt;Type&gt;</c>.</summary>
public enum PartitionType
{
    /// <summary>EFI System Partition (the UEFI boot partition).</summary>
    EFI,

    /// <summary>Microsoft Reserved partition (no filesystem, no letter).</summary>
    MSR,

    /// <summary>A normal data/OS partition.</summary>
    Primary,

    /// <summary>Windows Recovery Environment partition.</summary>
    Recovery,
}

/// <summary>How a partition is formatted.</summary>
public enum PartitionFormat
{
    /// <summary>Not formatted here (e.g. an MSR partition).</summary>
    None,

    NTFS,
    FAT32,
}

/// <summary>One partition in a disk layout. <see cref="SizeMb"/> null means "extend to fill the rest of the disk".</summary>
public sealed record PartitionSpec(
    PartitionType Type,
    int? SizeMb,
    PartitionFormat Format,
    string Label,
    string? Letter,
    bool InstallHere);

/// <summary>A full disk layout: which disk, whether to wipe it first, and the partitions to create.</summary>
public sealed record DiskSpec(int DiskId, bool WipeDisk, IReadOnlyList<PartitionSpec> Partitions);

/// <summary>
/// Builds the <c>&lt;DiskConfiguration&gt;</c> + <c>&lt;ImageInstall&gt;</c> snippet for the windowsPE pass from a
/// <see cref="DiskSpec"/>. Pure and validated: an invalid layout throws with a plain-language message the Build
/// screen shows instead of producing a broken (and destructive) autounattend. Partition IDs follow creation order,
/// which is correct for a wiped disk (the reliable, recommended case).
/// </summary>
public static class DiskConfigGenerator
{
    /// <summary>The standard clean UEFI layout: EFI 300 MB, MSR 16 MB, Windows filling the rest. The editor's seed.</summary>
    public static IReadOnlyList<PartitionSpec> StandardUefiLayout =>
    [
        new PartitionSpec(PartitionType.EFI, 300, PartitionFormat.FAT32, "System", null, false),
        new PartitionSpec(PartitionType.MSR, 16, PartitionFormat.None, "", null, false),
        new PartitionSpec(PartitionType.Primary, null, PartitionFormat.NTFS, "Windows", "C", true),
    ];

    public static string BuildSnippet(DiskSpec spec)
    {
        Validate(spec);

        var wipe = spec.WipeDisk ? "true" : "false";
        var sb = new StringBuilder();
        sb.Append("<DiskConfiguration>\n");
        sb.Append("    <Disk wcm:action=\"add\">\n");
        sb.Append($"        <DiskID>{spec.DiskId}</DiskID>\n");
        sb.Append($"        <WillWipeDisk>{wipe}</WillWipeDisk>\n");

        sb.Append("        <CreatePartitions>\n");
        for (var i = 0; i < spec.Partitions.Count; i++)
        {
            var p = spec.Partitions[i];
            sb.Append("            <CreatePartition wcm:action=\"add\">\n");
            sb.Append($"                <Order>{i + 1}</Order>\n");
            sb.Append($"                <Type>{p.Type}</Type>\n");
            sb.Append(p.SizeMb is null
                ? "                <Extend>true</Extend>\n"
                : $"                <Size>{p.SizeMb}</Size>\n");
            sb.Append("            </CreatePartition>\n");
        }

        sb.Append("        </CreatePartitions>\n");

        sb.Append("        <ModifyPartitions>\n");
        for (var i = 0; i < spec.Partitions.Count; i++)
        {
            var p = spec.Partitions[i];
            if (p.Format == PartitionFormat.None && string.IsNullOrEmpty(p.Label) && string.IsNullOrEmpty(p.Letter))
            {
                continue;   // e.g. MSR: created but nothing to modify.
            }

            sb.Append("            <ModifyPartition wcm:action=\"add\">\n");
            sb.Append($"                <Order>{i + 1}</Order>\n");
            sb.Append($"                <PartitionID>{i + 1}</PartitionID>\n");
            if (p.Format != PartitionFormat.None)
            {
                sb.Append($"                <Format>{p.Format}</Format>\n");
            }

            if (!string.IsNullOrEmpty(p.Label))
            {
                sb.Append($"                <Label>{p.Label}</Label>\n");
            }

            if (!string.IsNullOrEmpty(p.Letter))
            {
                sb.Append($"                <Letter>{p.Letter}</Letter>\n");
            }

            sb.Append("            </ModifyPartition>\n");
        }

        sb.Append("        </ModifyPartitions>\n");
        sb.Append("    </Disk>\n");
        sb.Append("</DiskConfiguration>\n");

        var install = spec.Partitions.Select((p, i) => (p, i)).First(x => x.p.InstallHere).i + 1;
        sb.Append("<ImageInstall>\n");
        sb.Append("    <OSImage>\n");
        sb.Append("        <InstallTo>\n");
        sb.Append($"            <DiskID>{spec.DiskId}</DiskID>\n");
        sb.Append($"            <PartitionID>{install}</PartitionID>\n");
        sb.Append("        </InstallTo>\n");
        sb.Append("    </OSImage>\n");
        sb.Append("</ImageInstall>");

        return sb.ToString();
    }

    private static void Validate(DiskSpec spec)
    {
        if (spec.DiskId < 0)
        {
            throw new InvalidOperationException("The target disk number must be 0 or higher.");
        }

        if (spec.Partitions.Count == 0)
        {
            throw new InvalidOperationException("Add at least one partition.");
        }

        if (spec.Partitions.Count(p => p.SizeMb is null) > 1)
        {
            throw new InvalidOperationException("Only one partition can fill the rest of the disk; give the others a size.");
        }

        foreach (var p in spec.Partitions.Where(p => p.SizeMb is <= 0))
        {
            throw new InvalidOperationException("Partition sizes must be greater than 0 MB (or blank to fill the rest).");
        }

        var installs = spec.Partitions.Count(p => p.InstallHere);
        if (installs != 1)
        {
            throw new InvalidOperationException(installs == 0
                ? "Mark which partition Windows installs to."
                : "Only one partition can be the Windows install target.");
        }

        var target = spec.Partitions.First(p => p.InstallHere);
        if (target.Type != PartitionType.Primary || target.Format != PartitionFormat.NTFS)
        {
            throw new InvalidOperationException("Windows must install to a Primary, NTFS-formatted partition.");
        }
    }

    /// <summary>Parse a size field: blank/whitespace = "fill the rest" (null); otherwise a positive whole number of MB.</summary>
    public static int? ParseSize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var mb) && mb > 0
            ? mb
            : throw new InvalidOperationException($"'{text}' is not a valid size in MB (use a whole number, or leave blank to fill the rest).");
    }
}

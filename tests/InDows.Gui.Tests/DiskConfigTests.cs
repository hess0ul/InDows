using System.Xml;
using InDows.Core.Build;
using Xunit;

namespace InDows.Gui.Tests;

public class DiskConfigTests
{
    private static DiskSpec Standard(int diskId = 0, bool wipe = true) =>
        new(diskId, wipe, DiskConfigGenerator.StandardUefiLayout);

    [Fact]
    public void The_standard_layout_builds_well_formed_disk_configuration_xml()
    {
        var xml = DiskConfigGenerator.BuildSnippet(Standard());

        // Wrap so the two sibling elements parse as one document.
        new XmlDocument().LoadXml($"<r xmlns:wcm=\"x\">{xml}</r>");

        Assert.Contains("<DiskID>0</DiskID>", xml, StringComparison.Ordinal);
        Assert.Contains("<WillWipeDisk>true</WillWipeDisk>", xml, StringComparison.Ordinal);
        Assert.Contains("<Type>EFI</Type>", xml, StringComparison.Ordinal);
        Assert.Contains("<Extend>true</Extend>", xml, StringComparison.Ordinal);   // the Windows partition fills the rest
        Assert.Contains("<Letter>C</Letter>", xml, StringComparison.Ordinal);
        // MSR (partition 2) is created but not modified.
        Assert.Contains("<PartitionID>3</PartitionID>", xml, StringComparison.Ordinal);   // Windows = partition 3
        Assert.DoesNotContain("<PartitionID>2</PartitionID>", xml, StringComparison.Ordinal);
        // Installs to the marked partition.
        Assert.Contains("<InstallTo>\n            <DiskID>0</DiskID>\n            <PartitionID>3</PartitionID>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void The_target_disk_number_flows_into_both_the_layout_and_the_install_target()
    {
        var xml = DiskConfigGenerator.BuildSnippet(Standard(diskId: 2));

        Assert.Contains("<DiskID>2</DiskID>", xml, StringComparison.Ordinal);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(xml, "<DiskID>2</DiskID>").Count);   // disk + install
    }

    [Fact]
    public void Not_wiping_the_disk_is_reflected()
    {
        Assert.Contains("<WillWipeDisk>false</WillWipeDisk>", DiskConfigGenerator.BuildSnippet(Standard(wipe: false)), StringComparison.Ordinal);
    }

    [Fact]
    public void A_layout_with_no_install_target_is_rejected()
    {
        var spec = new DiskSpec(0, true, [new PartitionSpec(PartitionType.Primary, null, PartitionFormat.NTFS, "Windows", "C", false)]);
        var ex = Assert.Throws<InvalidOperationException>(() => DiskConfigGenerator.BuildSnippet(spec));
        Assert.Contains("Mark which partition", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Two_partitions_filling_the_rest_are_rejected()
    {
        var spec = new DiskSpec(0, true,
        [
            new PartitionSpec(PartitionType.Primary, null, PartitionFormat.NTFS, "A", "C", true),
            new PartitionSpec(PartitionType.Primary, null, PartitionFormat.NTFS, "B", "D", false),
        ]);
        Assert.Throws<InvalidOperationException>(() => DiskConfigGenerator.BuildSnippet(spec));
    }

    [Fact]
    public void The_install_target_must_be_primary_and_ntfs()
    {
        var spec = new DiskSpec(0, true, [new PartitionSpec(PartitionType.EFI, 300, PartitionFormat.FAT32, "System", null, true)]);
        var ex = Assert.Throws<InvalidOperationException>(() => DiskConfigGenerator.BuildSnippet(spec));
        Assert.Contains("Primary, NTFS", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("  ", null)]
    [InlineData("200", 200)]
    public void ParseSize_reads_blank_as_fill_the_rest_and_a_number_as_mb(string input, int? expected) =>
        Assert.Equal(expected, DiskConfigGenerator.ParseSize(input));

    [Fact]
    public void ParseSize_rejects_a_non_number()
    {
        Assert.Throws<InvalidOperationException>(() => DiskConfigGenerator.ParseSize("lots"));
    }
}

using InDows.Core.Restore;
using Xunit;

namespace InDows.Gui.Tests;

public class RestorePlanTests
{
    [Fact]
    public void Normal_file_goes_back_to_its_original_location()
    {
        var plan = new RestorePlan([], toOriginalLocations: true, targetFolder: null);

        var targets = plan.TargetsFor("C/Users/tom/notes.txt");

        Assert.Equal(new[] { @"C:\Users\tom\notes.txt" }, targets);
    }

    [Fact]
    public void Deduplicated_file_goes_to_every_place_it_belonged()
    {
        var map = new[] { new RestoreMapEntry("C/Backup/photo.jpg", new[] { "C:/Pictures/photo.jpg", "D:/Copy/photo.jpg" }) };
        var plan = new RestorePlan(map, toOriginalLocations: true, targetFolder: null);

        var targets = plan.TargetsFor("C/Backup/photo.jpg");

        Assert.Equal(new[] { @"C:\Pictures\photo.jpg", @"D:\Copy\photo.jpg" }, targets);
    }

    [Fact]
    public void Folder_mode_rebuilds_under_the_chosen_folder_keeping_the_drive_as_folder_layout()
    {
        var plan = new RestorePlan([], toOriginalLocations: false, targetFolder: @"E:\restore");

        var targets = plan.TargetsFor("C/Users/tom/notes.txt");

        Assert.Equal(new[] { @"E:\restore\C\Users\tom\notes.txt" }, targets);
    }
}

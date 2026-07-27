namespace InDows.Gui.ViewModels;

/// <summary>One drive row on the Home screen: its mount name, a one-line detail, and whether it is usable.</summary>
public sealed record DriveRow(string Name, string Detail, bool Ready);

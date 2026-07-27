using System.Collections.ObjectModel;
using InDows.Core.Build;
using InDows.Gui.Navigation;

namespace InDows.Gui.ViewModels;

/// <summary>
/// One editable partition row in the disk layout. Wraps a <see cref="PartitionSpec"/> as bindable fields; the
/// size is text so "blank = fill the rest" round-trips. Picking "install here" tells the parent to clear the
/// others, so exactly one target stays marked.
/// </summary>
public sealed class PartitionRowVm : ViewModelBase
{
    private readonly Action<PartitionRowVm> _onInstallPicked;
    private PartitionType _type;
    private string _sizeText;
    private PartitionFormat _format;
    private string _label;
    private string _letter;
    private bool _installHere;

    public PartitionRowVm(PartitionSpec seed, Action<PartitionRowVm> onRemove, Action<PartitionRowVm> onInstallPicked)
    {
        _type = seed.Type;
        _sizeText = seed.SizeMb?.ToString() ?? "";
        _format = seed.Format;
        _label = seed.Label;
        _letter = seed.Letter ?? "";
        _installHere = seed.InstallHere;
        _onInstallPicked = onInstallPicked;
        RemoveCommand = new RelayCommand(_ => onRemove(this));
    }

    // Instance properties (not static) so the per-row ComboBoxes can bind to them by path.
    public PartitionType[] TypeOptions => Enum.GetValues<PartitionType>();

    public PartitionFormat[] FormatOptions => Enum.GetValues<PartitionFormat>();

    public PartitionType Type
    {
        get => _type;
        set => Set(ref _type, value);
    }

    /// <summary>Size in MB, or blank to fill the rest of the disk.</summary>
    public string SizeText
    {
        get => _sizeText;
        set => Set(ref _sizeText, value);
    }

    public PartitionFormat Format
    {
        get => _format;
        set => Set(ref _format, value);
    }

    public string Label
    {
        get => _label;
        set => Set(ref _label, value);
    }

    public string Letter
    {
        get => _letter;
        set => Set(ref _letter, value);
    }

    public bool InstallHere
    {
        get => _installHere;
        set
        {
            Set(ref _installHere, value);
            if (value)
            {
                _onInstallPicked(this);
            }
        }
    }

    public RelayCommand RemoveCommand { get; }

    /// <summary>Untick this row's install flag without firing the parent callback (used to enforce single-select).</summary>
    public void ClearInstall()
    {
        if (_installHere)
        {
            _installHere = false;
            Raise(nameof(InstallHere));
        }
    }

    public PartitionSpec ToSpec() => new(
        Type,
        DiskConfigGenerator.ParseSize(SizeText),
        Format,
        Label?.Trim() ?? "",
        string.IsNullOrWhiteSpace(Letter) ? null : Letter.Trim(),
        InstallHere);
}

/// <summary>
/// The bespoke editor behind the (gated) <c>disk</c> module: pick the target disk, whether to wipe it, and build
/// a partition table row by row. Seeded with the standard clean UEFI layout so a user edits a sane default rather
/// than starting blank. <see cref="ToSpec"/> feeds <see cref="DiskConfigGenerator"/> at generation time.
/// </summary>
public sealed class DiskEditorViewModel : ViewModelBase
{
    private string _diskIdText = "0";
    private bool _wipeDisk = true;

    public DiskEditorViewModel()
    {
        foreach (var p in DiskConfigGenerator.StandardUefiLayout)
        {
            AddRow(p);
        }

        AddPartitionCommand = new RelayCommand(_ =>
            AddRow(new PartitionSpec(PartitionType.Primary, 51200, PartitionFormat.NTFS, "Data", null, false)));
    }

    public ObservableCollection<PartitionRowVm> Partitions { get; } = [];

    /// <summary>Target disk number (0 = first disk). Kept as text so an empty box reads as an error, not disk 0.</summary>
    public string DiskIdText
    {
        get => _diskIdText;
        set => Set(ref _diskIdText, value);
    }

    public bool WipeDisk
    {
        get => _wipeDisk;
        set => Set(ref _wipeDisk, value);
    }

    public RelayCommand AddPartitionCommand { get; }

    public DiskSpec ToSpec()
    {
        if (!int.TryParse(DiskIdText?.Trim(), out var id))
        {
            throw new InvalidOperationException("The target disk number must be a whole number (0, 1, 2…).");
        }

        return new DiskSpec(id, WipeDisk, Partitions.Select(p => p.ToSpec()).ToList());
    }

    private void AddRow(PartitionSpec seed) =>
        Partitions.Add(new PartitionRowVm(seed, r => Partitions.Remove(r), OnInstallPicked));

    private void OnInstallPicked(PartitionRowVm picked)
    {
        foreach (var row in Partitions)
        {
            if (!ReferenceEquals(row, picked))
            {
                row.ClearInstall();
            }
        }
    }
}

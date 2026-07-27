using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using InDows.Core.Build;

namespace InDows.Gui.Navigation;

/// <summary>Maps a <see cref="ModuleRisk"/> to the badge colour in the Build checklist.</summary>
public sealed class RiskToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        ModuleRisk.Safe => new SolidColorBrush(Color.FromRgb(0x5D, 0xD5, 0xA8)),
        ModuleRisk.Advanced => new SolidColorBrush(Color.FromRgb(0xE0, 0xB3, 0x41)),
        ModuleRisk.Risky => new SolidColorBrush(Color.FromRgb(0xE8, 0x6A, 0x6A)),
        _ => Brushes.Gray,
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}

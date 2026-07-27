using System.Globalization;
using System.Windows.Data;

namespace InDows.Gui.Navigation;

/// <summary>
/// Binds a radio button (a bool) to one value of an enum: true when the bound enum equals the parameter,
/// and on check it sets the enum back to that value. Lets a group of radios drive a single enum property.
/// </summary>
public sealed class EnumToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null && string.Equals(value.ToString(), parameter?.ToString(), StringComparison.Ordinal);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true && parameter is not null ? Enum.Parse(targetType, parameter.ToString()!) : Binding.DoNothing;
}

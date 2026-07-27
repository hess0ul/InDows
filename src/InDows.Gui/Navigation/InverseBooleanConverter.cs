using System.Globalization;
using System.Windows.Data;

namespace InDows.Gui.Navigation;

/// <summary>Inverts a bool for two-way bindings (e.g. a second radio: "a folder" = not "whole disk").</summary>
public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool b ? !b : false;
}

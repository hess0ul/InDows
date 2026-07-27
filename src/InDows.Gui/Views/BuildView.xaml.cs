using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InDows.Gui.ViewModels;
using Microsoft.Win32;

namespace InDows.Gui.Views;

public partial class BuildView : UserControl
{
    public BuildView() => InitializeComponent();

    /// <summary>PasswordBox.Password isn't bindable, so bridge it to the row here.</summary>
    private void Param_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box && box.DataContext is ParamRow row)
        {
            row.Value = box.Password;
        }
    }

    /// <summary>"Browse…" on a file parameter: open an open-file dialog and put the chosen path in the field.
    /// Kept in code-behind (like the PasswordBox bridge) so no dialog seam has to thread through the view-models.</summary>
    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: ParamRow row })
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = row.Label,
            Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
            CheckFileExists = true,
        };
        if (dialog.ShowDialog() == true)
        {
            row.Value = dialog.FileName;
        }
    }

    /// <summary>Keep a number field digits-only: reject any keystroke that isn't a digit.</summary>
    private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e) =>
        e.Handled = !DigitsOnly().IsMatch(e.Text);

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex DigitsOnly();

    /// <summary>Let the mouse wheel scroll the page even when the pointer is over the read-only preview:
    /// swallow the wheel here and re-raise it on the parent so the outer ScrollViewer handles it.</summary>
    private void Preview_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled || sender is not TextBox box)
        {
            return;
        }

        e.Handled = true;
        var forwarded = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = box,
        };
        (box.Parent as UIElement)?.RaiseEvent(forwarded);
    }
}

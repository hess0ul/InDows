using System.Windows;
using System.Windows.Controls;
using InDows.Gui.ViewModels;

namespace InDows.Gui.Views;

public partial class RestoreView : UserControl
{
    public RestoreView() => InitializeComponent();

    /// <summary>A PasswordBox does not expose its value as a bindable property (by design), so we push it
    /// to the view-model here. The password lives only in memory, never in a binding or the session file.</summary>
    private void VaultPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RestoreViewModel vm && sender is PasswordBox box)
        {
            vm.VaultPassword = box.Password;
        }
    }
}

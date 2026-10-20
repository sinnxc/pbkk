using System.Windows;
using Sibumi.Models;
using Sibumi.ViewModels;
using Sibumi.Views;

namespace Sibumi.Core;

public static class DialogService
{
    public static bool Confirm(string title, string message) =>
        MessageBox.Show(Application.Current.MainWindow, message, title,
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;

    public static bool ShowBookingForm(Booking existing)
    {
        var vm = new BookingFormViewModel(existing);
        var win = new BookingFormWindow { DataContext = vm, Owner = Application.Current.MainWindow };
        vm.RequestClose += ok => win.DialogResult = ok;
        win.Loaded += async (_, _) =>
        {
            try { await vm.InitAsync(); } catch (Exception ex) { ErrorHandler.Handle(ex); win.DialogResult = false; }
        };
        return win.ShowDialog() == true;
    }
}
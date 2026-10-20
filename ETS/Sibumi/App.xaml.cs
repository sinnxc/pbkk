using System.Windows;
using Sibumi.Core;
using Sibumi.Services;
using Sibumi.Views;

namespace Sibumi;

public partial class App : Application
{
    public static bool IsLoggingOut;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        DispatcherUnhandledException += (_, a) => { ErrorHandler.Handle(a.Exception); a.Handled = true; };
        ThemeService.LoadSaved();

        try { await new AuthService().EnsureSeedAsync(); }
        catch (Exception ex)
        {
            Logger.Write(ex);
            MessageBox.Show("SIBUMI tidak dapat terhubung ke database.\n\nPeriksa SQL Server dan connection string di appsettings.json.",
                "SIBUMI", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }
        ShowLogin();
    }

    public static void ShowLogin()
    {
        var login = new LoginWindow();
        Current.MainWindow = login;
        if (login.ShowDialog() == true)
        {
            IsLoggingOut = false;
            var main = new MainWindow();
            Current.MainWindow = main;
            main.Show();
        }
        else Current.Shutdown();
    }
}
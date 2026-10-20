using System.Windows;
using Sibumi.Core;
using Sibumi.ViewModels;

namespace Sibumi.Views;

public partial class MainWindow : Window
{
    readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.LogoutRequested += OnLogout;
        Loaded += (_, _) => _vm.NavigateCommand.Execute("Dashboard");
        Closed += (_, _) =>
        {
            _vm.Dispose();
            if (!App.IsLoggingOut) Application.Current.Shutdown();
        };
    }

    void OnLogout()
    {
        App.IsLoggingOut = true;
        Session.End();
        Close();
        Dispatcher.BeginInvoke(App.ShowLogin);
    }
}
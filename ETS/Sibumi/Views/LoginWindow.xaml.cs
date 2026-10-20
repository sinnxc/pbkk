using System.Windows;
using Sibumi.ViewModels;

namespace Sibumi.Views;

public partial class LoginWindow : Window
{
    readonly LoginViewModel _vm = new();

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.LoginSucceeded += () => DialogResult = true;
        PwdBox.Password = _vm.Password;
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LoginViewModel.Password) && PwdBox.Password != _vm.Password)
                PwdBox.Password = _vm.Password;
        };
    }

    // PasswordBox tidak mendukung binding demi keamanan; cukup salin ke VM.
    void PwdBox_PasswordChanged(object sender, RoutedEventArgs e) => _vm.Password = PwdBox.Password;
}
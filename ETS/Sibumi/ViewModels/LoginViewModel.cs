using System.Windows.Input;
using Sibumi.Core;
using Sibumi.Services;

namespace Sibumi.ViewModels;

public class LoginViewModel : ViewModelBase
{
    readonly AuthService _auth = new();
    string _username = "petugas", _password = "Petugas#123", _error;
    bool _busy;

    public string Username { get => _username; set => Set(ref _username, value); }
    public string Password { get => _password; set => Set(ref _password, value); }
    public string Error { get => _error; set { Set(ref _error, value); OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsBusy { get => _busy; set => Set(ref _busy, value); }

    public event Action LoginSucceeded;
    public ICommand LoginCommand { get; }
    public ICommand QuickFillCommand { get; }

    public LoginViewModel()
    {
        LoginCommand = new AsyncCommand(async _ =>
        {
            Error = null;
            IsBusy = true;
            try
            {
                await _auth.LoginAsync(Username, Password);
                LoginSucceeded?.Invoke();
            }
            catch (BusinessException ex) { Error = ex.Message; }
            catch (Exception ex)
            {
                Logger.Write(ex);
                Error = "Tidak dapat terhubung ke database. Coba lagi nanti.";
            }
            finally { IsBusy = false; }
        });

        QuickFillCommand = new RelayCommand(p =>
        {
            var role = (string)p;
            if (role == "admin")
            {
                Username = "admin";
                Password = "Admin#123";
            }
            else
            {
                Username = "petugas";
                Password = "Petugas#123";
            }
        });
    }
}